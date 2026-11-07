using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Errors;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Authorization;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Timing;
using Volo.Abp.Validation;

namespace HR_Mangment2.Infrastructure;

public enum ProbeStatus
{
    Active,
    OnHold
}

public class ProbeAddressDto
{
    [Required]
    public string? CityName { get; set; }
}

public class ProbeDto
{
    public ProbeStatus Status { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly Time { get; set; }

    public TimeOnly? OptionalTime { get; set; }

    public DateTime At { get; set; }

    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public ProbeAddressDto? Address { get; set; }
}

public class ProbeFluentInput
{
    public string? DisplayName { get; set; }
}

public class ProbeFluentInputValidator : AbstractValidator<ProbeFluentInput>
{
    public ProbeFluentInputValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty();
    }
}

/// <summary>Test-only endpoints that exercise the platform behaviour shared by every future module.</summary>
[Route("test-probe")]
[IgnoreAntiforgeryToken]
public class ProbeController : AbpController
{
    private readonly IObjectValidator _objectValidator;
    private readonly IClock _clock;

    public ProbeController(IObjectValidator objectValidator, IClock clock)
    {
        _objectValidator = objectValidator;
        _clock = clock;
    }

    [HttpGet("format")]
    public ProbeDto GetFormat()
    {
        return new ProbeDto
        {
            Status = ProbeStatus.OnHold,
            Date = new DateOnly(2026, 3, 15),
            Time = new TimeOnly(8, 30),
            At = _clock.Normalize(new DateTime(2026, 3, 15, 6, 10, 0, DateTimeKind.Utc)),
            Amount = 12.5m
        };
    }

    [HttpPost("echo")]
    public ProbeDto Echo([FromBody] ProbeDto input)
    {
        return input;
    }

    [HttpPost("fluent")]
    public async Task<ProbeFluentInput> Fluent([FromBody] ProbeFluentInput input)
    {
        await _objectValidator.ValidateAsync(input);
        return input;
    }

    [HttpGet("page")]
    public PagedAndSortedResultRequestDto Page([FromQuery] PagedAndSortedResultRequestDto input)
    {
        return input;
    }

    [HttpGet("throw/{kind}")]
    public IActionResult Throw(string kind)
    {
        throw kind switch
        {
            "rule" => new BusinessException(HrErrorCodes.Common.IdempotencyKeyReused, details: "internal detail"),
            "password-gate" => new BusinessException(HrErrorCodes.Identity.PasswordChangeRequired),
            "mfa-gate" => new BusinessException(HrErrorCodes.Identity.MfaEnrollmentRequired),
            "other-business" => new BusinessException("Other:Module:Rule"),
            "concurrency" => new AbpDbConcurrencyException("stale"),
            "not-found" => new EntityNotFoundException(typeof(ProbeDto), "x"),
            "forbidden" => new AbpAuthorizationException("no"),
            "unhandled" => new InvalidOperationException("secret internal message"),
            _ => new ArgumentOutOfRangeException(nameof(kind))
        };
    }
}
