namespace Common.Filters;

internal sealed class ModelStateEnvelopeFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var errors = new Dictionary<string, string[]>();
        foreach (var entry in context.ModelState)
        {
            if (entry.Value is null || entry.Value.ValidationState != ModelValidationState.Invalid)
                continue;

            var messages = entry.Value.Errors
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? error.Exception?.Message : error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Cast<string>()
                .ToArray();

            if (messages.Length > 0)
                errors[entry.Key] = messages;
        }

        var message = string.Join("; ", errors.SelectMany(error => error.Value));
        if (message.Length == 0)
            message = "Invalid request.";

        context.Result = new OkObjectResult(
            TResponse<object>.Fail(ResponseStatus.InvalidArgument, ErrorCodes.Business.Validation, message, errors));
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
