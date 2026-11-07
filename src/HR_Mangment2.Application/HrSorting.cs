using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using Volo.Abp.Validation;

namespace HR_Mangment2;

/// <summary>
/// Applies the contract's "field direction" sorting from an allow-list, so a client can only sort on the fields
/// a list offers. Anything else is a 400 on the sorting parameter.
/// </summary>
public static class HrSorting
{
    /// <param name="tieBreaker">The unique field that keeps paging stable. Rows without an Id name their own.</param>
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sorting,
        string defaultSorting,
        IReadOnlyDictionary<string, string> fields,
        string tieBreaker = "Id")
    {
        var resolved = string.IsNullOrWhiteSpace(sorting) ? defaultSorting : Resolve(sorting, fields);
        return query.OrderBy(resolved + ", " + tieBreaker);
    }

    private static string Resolve(string sorting, IReadOnlyDictionary<string, string> fields)
    {
        var parts = new List<string>();
        foreach (var part in sorting.Split(',', StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = tokens.Length is 1 or 2
                ? fields.FirstOrDefault(x => string.Equals(x.Key, tokens[0], StringComparison.OrdinalIgnoreCase)).Value
                : null;
            var direction = tokens.Length == 2 ? tokens[1].ToLowerInvariant() : "asc";

            if (field == null || direction is not ("asc" or "desc"))
            {
                throw new AbpValidationException(
                    "Invalid sorting.",
                    new List<ValidationResult> { new("Invalid sorting.", new[] { "sorting" }) });
            }

            parts.Add(field + " " + direction);
        }

        return string.Join(", ", parts);
    }
}
