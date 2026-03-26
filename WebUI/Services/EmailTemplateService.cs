using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebUI.Models.Email;

namespace WebUI.Services
{
    public interface IEmailTemplateService
    {
        Task<string> GenerateRegistrationEmail(RegistrationEmailModel model);
        Task<string> GeneratePasswordResetEmail(PasswordResetEmailModel model);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public EmailTemplateService(
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GenerateRegistrationEmail(RegistrationEmailModel model)
        {
            return await RenderViewToStringAsync("/Views/EmailTemplates/RegistrationEmail.cshtml", model);
        }

        public async Task<string> GeneratePasswordResetEmail(PasswordResetEmailModel model)
        {
            return await RenderViewToStringAsync("/Views/EmailTemplates/PasswordResetEmail.cshtml", model);
        }

        private async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.GetView(null, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} bulunamadı.");
                }

                var viewDictionary = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}
