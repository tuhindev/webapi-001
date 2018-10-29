using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
namespace WebApplication3
{
    public class APIResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private JsonSerializerSettings _jsonConverterSettigs;
        //private string _requestId = null;

        public APIResponseMiddleware(RequestDelegate next)
        {
            _next = next;
            _jsonConverterSettigs = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            //_requestId = Guid.NewGuid().ToString("n");
        }

        public async Task Invoke(HttpContext context)
        {
            if (IsSwagger(context))
                await _next(context);
            else
            {
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    try
                    {
                        await _next.Invoke(context);

                        if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                        {
                            var body = await FormatResponse(context.Response);
                            await HandleSuccessRequestAsync(context, body, context.Response.StatusCode);

                        }
                        else if (context.Response.StatusCode == (int)HttpStatusCode.InternalServerError) {
                            await HandleExceptionAsync(context, null);
                        }
                        else
                        {
                            await HandleNotSuccessRequestAsync(context, context.Response.StatusCode);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        await HandleExceptionAsync(context, ex);
                    }
                    finally
                    {
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
            }

        }

        private static Task HandleExceptionAsync(HttpContext context, System.Exception exception)
        {
            ApiError apiError = null;
            APIResponse apiResponse = null;
            int code = 0;

            if (exception is ApiException)
            {
                var ex = exception as ApiException;
                apiError = new ApiError(ex.Message);
                apiError.ValidationErrors = ex.Errors;
                apiError.ReferenceErrorCode = ex.ReferenceErrorCode;
                apiError.ReferenceDocumentLink = ex.ReferenceDocumentLink;
                code = ex.StatusCode;
                context.Response.StatusCode = code;

            }
            else if (exception is UnauthorizedAccessException)
            {
                apiError = new ApiError("Unauthorized Access");
                code = (int)HttpStatusCode.Unauthorized;
                context.Response.StatusCode = code;
            }
            else
            {
#if !DEBUG
            var msg = "An exexpected error occurred.";
            string stack = null;
#else
                var msg = exception.GetBaseException().Message;
                string stack = exception.StackTrace;
#endif

                apiError = new ApiError(msg);
                apiError.Details = stack;
                code = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusCode = code;
            }

            context.Response.ContentType = "application/json";

            apiResponse = new APIResponse
                          (code, ResponseMessageEnum.Exception.ToString(), null, apiError, Guid.NewGuid().ToString("n"));

            var json = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            return context.Response.WriteAsync(json);
        }

        private static Task HandleNotSuccessRequestAsync(HttpContext context, int code)
        {
            context.Response.ContentType = "application/json";

            ApiError apiError = null;
            APIResponse apiResponse = null;

            if (code == (int)HttpStatusCode.NotFound)
                apiError = new ApiError
                           ("The specified URI does not exist. Please verify and try again.");
            else if (code == (int)HttpStatusCode.NoContent)
                apiError = new ApiError("The specified URI does not contain any content.");
            else
                apiError = new ApiError("Unable to process this request.");

            apiResponse = new APIResponse
                          (code, ResponseMessageEnum.Failure.ToString(), null, apiError, Guid.NewGuid().ToString("n"));
            context.Response.StatusCode = code;

            var json = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            return context.Response.WriteAsync(json);
        }

        private static Task HandleSuccessRequestAsync(HttpContext context, object body, int code)
        {
            context.Response.ContentType = "application/json";
            string jsonString, bodyText = string.Empty;
            APIResponse apiResponse = null;
            if (!body.ToString().IsValidJson())
                bodyText = JsonConvert.SerializeObject(body, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            else
                bodyText = body.ToString();

            dynamic bodyContent = JsonConvert.DeserializeObject<dynamic>(bodyText);
            Type type;

            type = bodyContent?.GetType();

            if (type.Equals(typeof(Newtonsoft.Json.Linq.JObject)))
            {
                apiResponse = JsonConvert.DeserializeObject<APIResponse>(bodyText);
                if (apiResponse.StatusCode != code)
                    jsonString = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                else if (apiResponse.Result != null)
                    jsonString = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                else
                {
                    apiResponse = new APIResponse
                                 (code, ResponseMessageEnum.Success.ToString(), bodyContent, null, Guid.NewGuid().ToString("n"));
                    jsonString = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                }
            }
            else
            {
                apiResponse = new APIResponse
                              (code, ResponseMessageEnum.Success.ToString(), bodyContent, null, Guid.NewGuid().ToString("n"));
                jsonString = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }

            return context.Response.WriteAsync(jsonString);
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var plainBodyText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return plainBodyText;
        }

        private bool IsSwagger(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/swagger");
        }
    }
}
