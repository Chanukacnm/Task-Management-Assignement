using FluentValidation;
using MediatR;
using ValidationException = TaskManagement.Application.Common.Exceptions.ValidationException;

namespace TaskManagement.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that runs every registered FluentValidation
/// validator for a request before its handler executes. If any rule fails the
/// request is rejected with a <see cref="ValidationException"/>.
/// </summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(result => result.Errors.Count != 0)
                .SelectMany(result => result.Errors)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
