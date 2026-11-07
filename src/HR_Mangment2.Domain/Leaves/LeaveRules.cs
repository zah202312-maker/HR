using System;
using System.Collections.Generic;

namespace HR_Mangment2.Leaves;

/// <summary>Personal leave years. Pure date rules, so every case can be tested directly.</summary>
public static class LeaveYearMath
{
    /// <summary>The start of the leave year in a calendar year. Someone hired on 29 February starts on the 28th in a non-leap year.</summary>
    public static DateOnly Anniversary(DateOnly hireDate, int year)
    {
        var day = hireDate is { Month: 2, Day: 29 } && !DateTime.IsLeapYear(year) ? 28 : hireDate.Day;
        return new DateOnly(year, hireDate.Month, day);
    }

    /// <summary>The leave year containing a date. A date before hiring falls in the first year, which starts at the hire date.</summary>
    public static (DateOnly Start, DateOnly End) YearContaining(DateOnly hireDate, DateOnly date)
    {
        var year = Math.Max(date.Year, hireDate.Year);
        var start = Anniversary(hireDate, year);
        if (date < start && year > hireDate.Year)
        {
            start = Anniversary(hireDate, year - 1);
        }

        return (start, NextStart(hireDate, start).AddDays(-1));
    }

    public static DateOnly NextStart(DateOnly hireDate, DateOnly yearStart) => Anniversary(hireDate, yearStart.Year + 1);

    /// <summary>Quantities keep four decimal places, the contract's precision for days and hours.</summary>
    public static decimal Round(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);
}

/// <summary>What a balance has to offer. The held figures are negative or zero, as they are in the ledger.</summary>
public record LeavePools(
    decimal CarriedNow,
    decimal EntitlementNow,
    decimal FutureCarriedHeld,
    decimal FutureEntitlementHeld,
    decimal NextYearEntitlement);

public record LeaveDemand(bool IsFutureYear, decimal Days);

public record LeaveAllocation(decimal FromCarried, decimal FromEntitlement);

public record LeaveAllocationResult(
    IReadOnlyList<LeaveAllocation> Allocations,
    decimal AvailableBefore,
    decimal RemainingAfter,
    decimal Shortage,
    decimal FromCarried,
    decimal FromEntitlement);

/// <summary>
/// The deduction order: added days first, then the year's own. A part in the coming year can use what is expected to
/// carry over plus that year's entitlement, so days already promised to the coming year cannot also be spent now.
/// </summary>
public static class LeaveAllocator
{
    public static LeaveAllocationResult Allocate(LeavePools pools, IReadOnlyList<LeaveDemand> demands)
    {
        // Days reserved for the coming year's carry-over come off the end of the deduction order, the entitlement first.
        var reserved = -pools.FutureCarriedHeld;
        var entitlementLeft = Math.Max(0, pools.EntitlementNow - reserved);
        var carriedLeft = Math.Max(0, pools.CarriedNow - Math.Max(0, reserved - Math.Max(0, pools.EntitlementNow)));

        var futureEntitlementLeft = Math.Max(0, pools.NextYearEntitlement + pools.FutureEntitlementHeld);
        var hasFutureDemand = false;
        foreach (var demand in demands)
        {
            hasFutureDemand |= demand.IsFutureYear && demand.Days > 0;
        }

        var availableBefore = carriedLeft + entitlementLeft + (hasFutureDemand ? futureEntitlementLeft : 0);

        var allocations = new List<LeaveAllocation>();
        decimal shortage = 0, fromCarried = 0, fromEntitlement = 0, requested = 0;

        // Current days go first even when listed later: what is left of them is what carries into the coming year.
        var currentTotal = (decimal)0;
        foreach (var demand in demands)
        {
            if (!demand.IsFutureYear)
            {
                currentTotal += demand.Days;
            }
        }

        var expectedCarry = Math.Max(0, carriedLeft + entitlementLeft - currentTotal);

        foreach (var demand in demands)
        {
            requested += demand.Days;
            decimal carried;
            decimal entitlement;

            if (demand.IsFutureYear)
            {
                carried = Math.Min(demand.Days, expectedCarry);
                expectedCarry -= carried;
                entitlement = Math.Min(demand.Days - carried, futureEntitlementLeft);
                futureEntitlementLeft -= entitlement;
            }
            else
            {
                carried = Math.Min(demand.Days, carriedLeft);
                carriedLeft -= carried;
                entitlement = Math.Min(demand.Days - carried, entitlementLeft);
                entitlementLeft -= entitlement;
            }

            shortage += demand.Days - carried - entitlement;
            fromCarried += carried;
            fromEntitlement += entitlement;
            allocations.Add(new LeaveAllocation(LeaveYearMath.Round(carried), LeaveYearMath.Round(entitlement)));
        }

        return new LeaveAllocationResult(
            allocations,
            LeaveYearMath.Round(availableBefore),
            LeaveYearMath.Round(Math.Max(0, availableBefore - requested)),
            LeaveYearMath.Round(shortage),
            LeaveYearMath.Round(fromCarried),
            LeaveYearMath.Round(fromEntitlement));
    }
}
