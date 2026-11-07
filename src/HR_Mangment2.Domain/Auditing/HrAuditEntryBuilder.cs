using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Volo.Abp.Auditing;

namespace HR_Mangment2.Auditing;

public sealed record HrAuditDraft(AuditActionType Action, string? EntityType, string? EntityId, string[] ChangedFields, bool Succeeded);

/// <summary>
/// Turns one audited request into audit log rows. A request with a known meaning (role, permission, setting, password,
/// complaint handler) gives one row of that type. Any other write gives one row per changed entity, or one row from its
/// path when the entity is not tracked, such as ABP's users and roles.
/// </summary>
public static class HrAuditEntryBuilder
{
    private const string ChangedFieldsKey = "hr:changedFields";
    private const string ComplaintsNamespace = "HR_Mangment2.Complaints.";
    private const string Guid36 = "[0-9a-fA-F]{8}(?:-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}";

    private static readonly HashSet<string> BookkeepingFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "ConcurrencyStamp", "ExtraProperties", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId",
        "DeletionTime", "DeleterId", "IsDeleted", "TenantId"
    };

    /// <summary>Writes that say nothing worth auditing, and the device endpoint that is not a user action.</summary>
    private static readonly string[] IgnoredPrefixes =
    {
        "/api/app/notification", "/api/app/device-token", "/api/attendance-devices"
    };

    private sealed record Rule(string Verbs, Regex Path, AuditActionType Action, Func<Match, string> EntityType, bool OwnUserIsEntity = false);

    private static Rule Make(string verbs, string pattern, AuditActionType action, string entityType, bool ownUserIsEntity = false) =>
        new(verbs, new Regex("^" + pattern + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), action, _ => entityType, ownUserIsEntity);

    private static readonly Rule[] Rules =
    {
        new("PUT", new Regex("^/api/app/system-settings/(?<section>[a-z-]+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            AuditActionType.SettingChanged, m => "SystemSettings" + ToPascal(m.Groups["section"].Value)),
        Make("PUT", "/api/app/attendance-settings", AuditActionType.SettingChanged, "AttendanceSettings"),
        Make("PUT", "/api/app/weekly-off-setting", AuditActionType.SettingChanged, "WeeklyOffSetting"),

        Make("PUT", $"/api/app/role/{Guid36}/permission", AuditActionType.PermissionChanged, "Role"),
        Make("POST", "/api/app/role", AuditActionType.RoleChanged, "Role"),
        Make("PUT,DELETE", $"/api/app/role/{Guid36}", AuditActionType.RoleChanged, "Role"),

        Make("PUT", $"/api/app/user-account/{Guid36}/permission", AuditActionType.PermissionChanged, "UserAccount"),
        Make("PUT", $"/api/app/user-account/{Guid36}/roles", AuditActionType.RoleChanged, "UserAccount"),
        Make("POST", $"/api/app/user-account/{Guid36}/reset-password", AuditActionType.PasswordReset, "UserAccount"),
        Make("POST", "/api/app/account/change-password", AuditActionType.PasswordReset, "UserAccount", ownUserIsEntity: true),
        Make("POST", "/account/resetpassword", AuditActionType.PasswordReset, "UserAccount"),
        Make("POST", "/api/app/account/mfa/(?:enroll|confirm|recovery-codes)", AuditActionType.Update, "Mfa", ownUserIsEntity: true),
        Make("POST", $"/api/app/user-account/{Guid36}/mfa/reset", AuditActionType.Update, "Mfa"),

        Make("POST,PUT,DELETE", $"/api/app/complaint-handler(?:/{Guid36}(?:/[a-z-]+)?)?", AuditActionType.ComplaintHandlerAssignment, "ComplaintHandler")
    };

    /// <summary>Names the changed fields for a change the entity tracking cannot see, such as a JSON settings document.</summary>
    public static void SetChangedFields(IAuditingManager auditingManager, IEnumerable<string> fields)
    {
        var scope = auditingManager.Current;
        if (scope != null)
        {
            scope.Log.ExtraProperties[ChangedFieldsKey] = fields.ToArray();
        }
    }

    /// <summary>Top-level JSON properties that differ, in camelCase. The values themselves never leave this method.</summary>
    public static string[] DiffJson(string before, string after)
    {
        using var oldDocument = JsonDocument.Parse(before);
        using var newDocument = JsonDocument.Parse(after);

        var oldValues = oldDocument.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetRawText());
        return newDocument.RootElement.EnumerateObject()
            .Where(x => !oldValues.TryGetValue(x.Name, out var old) || old != x.Value.GetRawText())
            .Select(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name))
            .ToArray();
    }

    public static List<HrAuditDraft> Build(AuditLogInfo log)
    {
        var drafts = BuildCore(log);
        log.ExtraProperties.Remove(ChangedFieldsKey);
        return drafts;
    }

    private static List<HrAuditDraft> BuildCore(AuditLogInfo log)
    {
        var verb = (log.HttpMethod ?? string.Empty).ToUpperInvariant();
        if (verb is not ("POST" or "PUT" or "PATCH" or "DELETE"))
        {
            return new List<HrAuditDraft>();
        }

        var path = (log.Url ?? string.Empty).Split('?')[0].TrimEnd('/');
        var succeeded = log.Exceptions.Count == 0 && (log.HttpStatusCode == null || log.HttpStatusCode < 400);
        var changes = log.EntityChanges
            .Where(x => x.EntityTypeFullName?.StartsWith(ComplaintsNamespace, StringComparison.Ordinal) != true)
            .ToList();

        var rule = Rules.FirstOrDefault(x => x.Verbs.Split(',').Contains(verb) && x.Path.IsMatch(path));
        if (rule != null)
        {
            var fields = log.ExtraProperties.TryGetValue(ChangedFieldsKey, out var explicitFields) && explicitFields is string[] named
                ? named
                : succeeded ? FieldsOf(changes) : Array.Empty<string>();
            var entityId = rule.OwnUserIsEntity ? log.UserId?.ToString() : FirstGuid(path);

            return new List<HrAuditDraft>
            {
                new(rule.Action, rule.EntityType(rule.Path.Match(path)), entityId, fields, succeeded)
            };
        }

        // Plain writes need a known user. Anonymous ones are the sign-in pages, token calls and failed probes.
        if (log.UserId == null || !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
            IgnoredPrefixes.Any(x => path.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
        {
            return new List<HrAuditDraft>();
        }

        // A failed request is rolled back, so the changes it reached did not happen.
        if (!succeeded || changes.Count == 0)
        {
            return new List<HrAuditDraft>
            {
                new(ActionOf(verb), EntityTypeFromPath(path), FirstGuid(path), Array.Empty<string>(), succeeded)
            };
        }

        return changes
            .Select(x => new HrAuditDraft(ActionOf(x.ChangeType), ShortName(x.EntityTypeFullName), x.EntityId, FieldsOf(new[] { x }), true))
            .ToList();
    }

    private static string[] FieldsOf(IEnumerable<EntityChangeInfo> changes) =>
        changes.SelectMany(x => x.PropertyChanges ?? new List<EntityPropertyChangeInfo>())
            .Select(x => x.PropertyName)
            .Where(x => !string.IsNullOrEmpty(x) && !BookkeepingFields.Contains(x!))
            .Select(x => JsonNamingPolicy.CamelCase.ConvertName(x!))
            .Distinct()
            .ToArray();

    private static AuditActionType ActionOf(string verb) => verb switch
    {
        "POST" => AuditActionType.Create,
        "DELETE" => AuditActionType.Delete,
        _ => AuditActionType.Update
    };

    private static AuditActionType ActionOf(Volo.Abp.Auditing.EntityChangeType type) => type switch
    {
        Volo.Abp.Auditing.EntityChangeType.Created => AuditActionType.Create,
        Volo.Abp.Auditing.EntityChangeType.Deleted => AuditActionType.Delete,
        _ => AuditActionType.Update
    };

    private static string? ShortName(string? fullName) => fullName?.Split('.').Last();

    private static string? FirstGuid(string path)
    {
        var match = Regex.Match(path, Guid36, RegexOptions.CultureInvariant);
        return match.Success ? match.Value : null;
    }

    private static string? EntityTypeFromPath(string path)
    {
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var first = parts.Length > 2 && parts[1].Equals("app", StringComparison.OrdinalIgnoreCase) ? parts[2] : parts.ElementAtOrDefault(1);
        return first == null ? null : ToPascal(first);
    }

    private static string ToPascal(string kebab) =>
        string.Concat(kebab.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
}
