using AIS.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using AIS.Utilities;
using System.Linq;

namespace AIS.Filters
{
    /// <summary>
    /// Ensures POST requests cannot bypass ModelState validation.
    /// </summary>
    public class PostModelValidationFilter : IActionFilter
    {
        private readonly ILogger<PostModelValidationFilter> _logger;

        public PostModelValidationFilter(ILogger<PostModelValidationFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                return;
            }

            if (context.ModelState.IsValid)
            {
                return;
            }

            var model = context.ActionArguments.Values.FirstOrDefault(v => v != null && v.GetType() != typeof(string));

            if (LoginRedirectHelper.IsApiRequest(context.HttpContext.Request))
            {
                var endpointName = (context.ActionDescriptor as ControllerActionDescriptor)?.ActionName
                    ?? context.ActionDescriptor?.DisplayName
                    ?? "Unknown";
                ValidationErrorHelper.LogValidationErrors(_logger, endpointName, context.ModelState);
                var payload = ValidationErrorHelper.BuildInvalidRequestResponse(context.ModelState);
                context.Result = new BadRequestObjectResult(payload);
                return;
            }

            if (context.Controller is Controller controller)
            {
                var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var viewResult = new ViewResult
                {
                    ViewName = descriptor?.ActionName,
                    ViewData = new ViewDataDictionary(controller.ViewData)
                    {
                        Model = model
                    }
                };

                viewResult.ViewData.ModelState.Merge(context.ModelState);
                context.Result = viewResult;
                return;
            }

            context.Result = new BadRequestObjectResult(context.ModelState);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
