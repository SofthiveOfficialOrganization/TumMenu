using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebUI.ModelBinding;

public sealed class DecimalModelBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
		if(valueProviderResult == ValueProviderResult.None)
			return Task.CompletedTask;

		bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

		var value = valueProviderResult.FirstValue;
		if(string.IsNullOrWhiteSpace(value))
		{
			if(Nullable.GetUnderlyingType(bindingContext.ModelType) is not null)
				bindingContext.Result = ModelBindingResult.Success(null);

			return Task.CompletedTask;
		}

		var culture = valueProviderResult.Culture;
		var parseCulture = ShouldPreferInvariant(value, culture)
			? CultureInfo.InvariantCulture
			: culture;

		if(decimal.TryParse(value, NumberStyles.Number, parseCulture, out var result))
		{
			bindingContext.Result = ModelBindingResult.Success(result);
			return Task.CompletedTask;
		}

		bindingContext.ModelState.TryAddModelError(
			bindingContext.ModelName,
			$"{bindingContext.ModelMetadata.GetDisplayName()} alanı geçerli bir sayı olmalıdır.");

		return Task.CompletedTask;
	}

	private static bool ShouldPreferInvariant(string value, CultureInfo culture)
	{
		var decimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
		return decimalSeparator != "." &&
		       value.Contains('.', StringComparison.Ordinal) &&
		       !value.Contains(decimalSeparator, StringComparison.Ordinal);
	}
}

public sealed class DecimalModelBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		var modelType = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
		return modelType == typeof(decimal) ? new DecimalModelBinder() : null;
	}
}
