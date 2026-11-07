using System;
using HR_Mangment2.Leaves;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

public class LeaveRulesTests
{
    private static DateOnly D(int year, int month, int day) => new(year, month, day);

    [Fact]
    public void A_year_runs_from_the_hire_anniversary_and_not_the_calendar()
    {
        var (start, end) = LeaveYearMath.YearContaining(D(2026, 3, 15), D(2027, 3, 14));
        start.ShouldBe(D(2026, 3, 15));
        end.ShouldBe(D(2027, 3, 14));

        LeaveYearMath.YearContaining(D(2026, 3, 15), D(2027, 3, 15)).Start.ShouldBe(D(2027, 3, 15));
    }

    [Theory]
    [InlineData(2025, 28)]
    [InlineData(2027, 28)]
    [InlineData(2028, 29)]
    public void Someone_hired_on_29_February_starts_on_the_28th_in_a_non_leap_year(int year, int expectedDay)
    {
        LeaveYearMath.Anniversary(D(2024, 2, 29), year).ShouldBe(D(year, 2, expectedDay));
    }

    [Fact]
    public void The_year_before_a_29_February_anniversary_ends_the_day_before_it()
    {
        var (start, end) = LeaveYearMath.YearContaining(D(2024, 2, 29), D(2027, 6, 1));
        start.ShouldBe(D(2027, 2, 28));
        end.ShouldBe(D(2028, 2, 28));
    }

    [Fact]
    public void A_date_before_hiring_falls_in_the_first_year()
    {
        LeaveYearMath.YearContaining(D(2026, 3, 15), D(2025, 1, 1)).Start.ShouldBe(D(2026, 3, 15));
    }

    [Fact]
    public void Added_days_go_first()
    {
        var result = LeaveAllocator.Allocate(new LeavePools(6, 21, 0, 0, 21), new[] { new LeaveDemand(false, 10) });

        result.Allocations[0].FromCarried.ShouldBe(6);
        result.Allocations[0].FromEntitlement.ShouldBe(4);
        result.AvailableBefore.ShouldBe(27);
        result.RemainingAfter.ShouldBe(17);
        result.Shortage.ShouldBe(0);
    }

    [Fact]
    public void A_shortage_never_makes_the_remainder_negative()
    {
        var result = LeaveAllocator.Allocate(new LeavePools(0, 3, 0, 0, 21), new[] { new LeaveDemand(false, 5) });

        result.Shortage.ShouldBe(2);
        result.RemainingAfter.ShouldBe(0);
        result.Allocations[0].FromEntitlement.ShouldBe(3);
    }

    [Fact]
    public void A_part_in_the_coming_year_uses_the_expected_carry_over_before_that_years_entitlement()
    {
        var result = LeaveAllocator.Allocate(
            new LeavePools(0, 10, 0, 0, 21),
            new[] { new LeaveDemand(false, 4), new LeaveDemand(true, 8) });

        result.Allocations[0].FromEntitlement.ShouldBe(4);
        result.Allocations[1].FromCarried.ShouldBe(6);
        result.Allocations[1].FromEntitlement.ShouldBe(2);
        result.AvailableBefore.ShouldBe(31);
        result.RemainingAfter.ShouldBe(19);
    }

    [Fact]
    public void Days_promised_to_the_coming_year_cannot_be_spent_now()
    {
        var result = LeaveAllocator.Allocate(new LeavePools(0, 10, -7, 0, 21), new[] { new LeaveDemand(false, 5) });

        result.AvailableBefore.ShouldBe(3);
        result.Shortage.ShouldBe(2);
    }

    [Fact]
    public void The_coming_years_entitlement_does_not_help_a_current_leave()
    {
        var result = LeaveAllocator.Allocate(new LeavePools(0, 2, 0, 0, 21), new[] { new LeaveDemand(false, 5) });

        result.AvailableBefore.ShouldBe(2);
        result.Shortage.ShouldBe(3);
    }

    [Fact]
    public void Fractions_keep_four_decimal_places()
    {
        LeaveYearMath.Round(1m / 3m).ShouldBe(0.3333m);
    }
}
