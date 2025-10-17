using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace VehicleRentalManagement.Models
{
    public class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            
            if (value == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

            var stringValue = value.FirstValue;

            if (string.IsNullOrEmpty(stringValue))
            {
                return Task.CompletedTask;
            }

            // Türkçe locale için virgülü noktaya çevir
            stringValue = stringValue.Replace(',', '.');

            if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, 
                    $"'{stringValue}' geçerli bir sayı değil.");
            }

            return Task.CompletedTask;
        }
    }

    public class DecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(decimal) || 
                context.Metadata.ModelType == typeof(decimal?))
            {
                return new DecimalModelBinder();
            }

            return null;
        }
    }
}
