using System.Collections.Generic;
using System.Linq;
using AIS.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace AIS.Utilities
    {
    public static class ValidationErrorHelper
        {
        public static IDictionary<string, string[]> BuildModelErrors(ModelStateDictionary modelState)
            {
            return modelState
                .Where(entry => entry.Value.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value.Errors.Select(error => error.ErrorMessage).ToArray());
            }

        public static ValidationErrorResponse BuildInvalidRequestResponse(ModelStateDictionary modelState, string message = "Invalid request")
            {
            return new ValidationErrorResponse
                {
                Status = false,
                Message = message,
                Errors = BuildModelErrors(modelState)
                };
            }

        public static ValidationErrorResponse BuildInvalidRequestResponse(string field, string error, string message = "Invalid request")
            {
            return new ValidationErrorResponse
                {
                Status = false,
                Message = message,
                Errors = new Dictionary<string, string[]>
                    {
                    [field] = new[] { error }
                    }
                };
            }

        public static void LogValidationErrors(ILogger logger, string endpointName, ModelStateDictionary modelState)
            {
            if (logger == null)
                {
                return;
                }

            var errors = BuildModelErrors(modelState);
            logger.LogWarning("Validation failed for {Endpoint}. Errors: {@Errors}", endpointName, errors);
            }
        }
    }
