using FluentValidation.Results;

namespace TaskManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown by the validation pipeline behaviour when one or more FluentValidation
/// rules fail. Mapped to HTTP 400 with a per-field error dictionary by the API layer.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(failure => failure.PropertyName, failure => failure.ErrorMessage)
            .ToDictionary(group => group.Key, group => group.ToArray());
    }

    /// <summary>Validation errors keyed by property name.</summary>
    public IDictionary<string, string[]> Errors { get; }
}
