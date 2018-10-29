using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication3
{
    public static class ApiResponseMiddlewareExtension
    {
        public static IApplicationBuilder UseAPIResponseWrapperMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<APIResponseMiddleware>();
        }
    }
}
