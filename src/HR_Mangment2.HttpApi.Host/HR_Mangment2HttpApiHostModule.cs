using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using OpenIddict.Server.AspNetCore;
using HR_Mangment2.EntityFrameworkCore;
using HR_Mangment2.MultiTenancy;
using HR_Mangment2.Controllers;
using HR_Mangment2.HealthChecks;
using HR_Mangment2.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;
using Volo.Abp.Users;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using HR_Mangment2.Complaints;
using HR_Mangment2.Finance;
using HR_Mangment2.Swagger;
using Microsoft.OpenApi;
using Volo.Abp;
using Volo.Abp.Studio;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Autofac;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Microsoft.AspNetCore.Hosting;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Studio.Client.AspNetCore;
using Volo.Abp.Security.Claims;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Principal;

namespace HR_Mangment2;

[DependsOn(
    typeof(HR_Mangment2HttpApiModule),
    typeof(AbpStudioClientAspNetCoreModule),
    typeof(AbpAspNetCoreMvcUiBasicThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(HR_Mangment2ApplicationModule),
    typeof(ComplaintsApplicationModule),
    typeof(ComplaintsHttpApiModule),
    typeof(FinanceApplicationModule),
    typeof(FinanceHttpApiModule),
    typeof(HR_Mangment2EntityFrameworkCoreModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSignalRModule),
    typeof(AbpAspNetCoreSerilogModule)
    )]
public class HR_Mangment2HttpApiHostModule : AbpModule
{
    public const string TestingEnvironmentName = "Testing";

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("HR_Mangment2");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        // The Testing environment behaves like production but has no certificate file.
        if (!hostingEnvironment.IsDevelopment() && !hostingEnvironment.IsEnvironment(TestingEnvironmentName))
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        // Full security artifacts are never logged, because they contain tokens.
        if (hostingEnvironment.IsDevelopment())
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });
            
            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        if (hostingEnvironment.IsDevelopment())
        {
            context.Services.AddRazorPages()
                .AddRazorRuntimeCompilation();
        }

        ConfigureStudio(hostingEnvironment);
        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles(hostingEnvironment);
        ConfigureExceptionHandling(hostingEnvironment);
        ConfigureOAuthFlows(context);

        // The API uses bearer tokens. Antiforgery stays on for the cookie-based sign-in pages only.
        Configure<Volo.Abp.AspNetCore.Mvc.AntiForgery.AbpAntiForgeryOptions>(options =>
        {
            options.AutoValidateFilter = type => type.Namespace?.StartsWith("HR_Mangment2.Controllers", StringComparison.Ordinal) != true;
        });
        ConfigureRateLimiting(context, configuration);

        Configure<AbpApplicationConfigurationOptions>(options =>
        {
            options.Contributors.Add(new AppConfigAccessContributor());
        });

        Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.Add(new HiddenAccountPagesConvention());
        });
        ConfigureTenancy(configuration);
        ConfigureHealthChecks(context);
        ConfigureSwagger(context, configuration);
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
    }

    private void ConfigureStudio(IHostEnvironment hostingEnvironment)
    {
        if (!hostingEnvironment.IsDevelopment())
        {
            Configure<AbpStudioClientOptions>(options =>
            {
                options.IsLinkEnabled = false;
            });
        }
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
        context.Services.Configure<Microsoft.AspNetCore.Identity.DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = HR_Mangment2.Identity.HrAccountConsts.PasswordResetLinkLifetime;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
        });
    }

    private void ConfigureBundles(IHostEnvironment hostingEnvironment)
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                BasicThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );

            options.ScriptBundles.Configure(
                BasicThemeBundles.Scripts.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                }
            );
        });
    }


    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<HR_Mangment2DomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}HR_Mangment2.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<HR_Mangment2DomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}HR_Mangment2.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<HR_Mangment2ApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}HR_Mangment2.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<HR_Mangment2ApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}HR_Mangment2.Application"));
            });
        }
    }

    /// <summary>The contract offers only Authorization Code with PKCE and refresh tokens.</summary>
    private static void ConfigureOAuthFlows(ServiceConfigurationContext context)
    {
        context.Services.PostConfigure<OpenIddictServerOptions>(options =>
        {
            options.GrantTypes.Clear();
            options.GrantTypes.Add(OpenIddictConstants.GrantTypes.AuthorizationCode);
            options.GrantTypes.Add(OpenIddictConstants.GrantTypes.RefreshToken);

            options.ResponseTypes.Clear();
            options.ResponseTypes.Add(OpenIddictConstants.ResponseTypes.Code);

            options.CodeChallengeMethods.Clear();
            options.CodeChallengeMethods.Add(OpenIddictConstants.CodeChallengeMethods.Sha256);
            options.RequireProofKeyForCodeExchange = true;
        });
    }

    /// <summary>Limits guessing on password and code endpoints. Over the limit the answer is 429 with Retry-After.</summary>
    private static void ConfigureRateLimiting(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimit:PermitPerMinute", 20);
        var window = TimeSpan.FromMinutes(1);

        context.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (rejected, _) =>
            {
                rejected.HttpContext.Response.Headers.RetryAfter = ((int)window.TotalSeconds).ToString();
                return ValueTask.CompletedTask;
            };

            options.AddPolicy(HrRateLimiting.DevicePolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = HrRateLimiting.DevicePermitPerMinute, Window = TimeSpan.FromMinutes(1) }));

            options.AddPolicy(HrRateLimiting.SensitivePolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.User.FindUserId()?.ToString() ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = permitLimit, Window = window }));

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                string.Equals(httpContext.Request.Path, "/connect/token", StringComparison.OrdinalIgnoreCase)
                    ? RateLimitPartition.GetFixedWindowLimiter(
                        "token:" + (httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"),
                        _ => new FixedWindowRateLimiterOptions { PermitLimit = permitLimit * 3, Window = window })
                    : RateLimitPartition.GetNoLimiter("none"));
        });
    }

    private void ConfigureExceptionHandling(IHostEnvironment hostingEnvironment)
    {
        Configure<AbpExceptionHandlingOptions>(options =>
        {
            options.SendExceptionsDetailsToClients = hostingEnvironment.IsDevelopment();
            options.SendStackTraceToClients = false;
        });
    }

    private void ConfigureTenancy(IConfiguration configuration)
    {
        var tenantName = configuration["App:TenantName"];
        if (string.IsNullOrWhiteSpace(tenantName))
        {
            throw new InvalidOperationException("App:TenantName is not configured.");
        }

        // PostConfigure runs after the framework has added its own resolvers.
        Configure<AbpTenantResolveOptions>(_ => { });
        PostConfigure<AbpTenantResolveOptions>(options =>
        {
            options.TenantResolvers.Clear();
            options.TenantResolvers.Add(new ServerTenantResolveContributor(tenantName));
        });
    }

    private static void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOidc(
            configuration["AuthServer:Authority"]!,
            ["HR_Mangment2"],
            [AbpSwaggerOidcFlows.AuthorizationCode],
            null,
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "HR_Mangment2 API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(HrSwaggerNames.SchemaId);
                options.CustomOperationIds(api => api.ActionDescriptor.AttributeRouteInfo?.Name);
                options.MapType<TimeOnly>(() => new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Pattern = "^([01][0-9]|2[0-3]):[0-5][0-9]$"
                });
                options.MapType<decimal>(() => new OpenApiSchema { Type = JsonSchemaType.Number, Format = "decimal" });
            });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddHR_Mangment2HealthChecks();
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        // Status-code pages redirect browsers to the error page. API and OAuth clients must get the real status.
        app.Use((httpContext, next) =>
        {
            var path = httpContext.Request.Path;
            if (path.StartsWithSegments("/api") || path.StartsWithSegments("/connect"))
            {
                var pages = httpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodePagesFeature>();
                if (pages != null)
                {
                    pages.Enabled = false;
                }
            }

            return next();
        });

        app.UseRouting();
        app.MapAbpStaticAssets();
        app.UseAbpStudioLink();
        app.UseAbpSecurityHeaders();
        app.UseCors();
        // SignalR clients send the token in the query string, because WebSockets have no headers.
        app.Use((httpContext, next) =>
        {
            if (httpContext.Request.Path.StartsWithSegments("/signalr-hubs") &&
                httpContext.Request.Query.TryGetValue("access_token", out var token) &&
                !httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                httpContext.Request.Headers.Authorization = "Bearer " + token;
            }

            return next();
        });

        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseAbpExceptionHandling();
        app.UseDynamicClaims();
        app.UseMiddleware<AccessGateMiddleware>();
        app.UseRateLimiter();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "HR_Mangment2 API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
