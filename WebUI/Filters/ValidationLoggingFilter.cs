using Microsoft.AspNetCore.Mvc.Filters;

namespace WebUI.Filters;

public class ValidationLoggingFilter(ILogger<ValidationLoggingFilter> logger) : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		if(!context.ModelState.IsValid)
		{
			var errors = context.ModelState
				.Where(x => x.Value?.Errors.Count > 0)
				.ToDictionary(
					kvp => kvp.Key,
					kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
				);

			logger.LogWarning("Validation failed for {Action}. Errors: {@Errors}", 
				context.ActionDescriptor.DisplayName, 
				errors);
		}
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
		// No action needed after execution
	}
}
