using GlobalLib.WebApi;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GlobalLib.WebApi.Models;

namespace GlobalLib.WebApi
{
    public static class ApiHelpers
    {
        public static HttpResponseMessage DefaultControllerException(HttpRequestMessage request, Exception ex)
        {
            ApiStatusResponse error = new ApiStatusResponse
            {
                Code = "500",
                Title = "Internal Error",
                Detail = ex.Message,
                Type = "error"
            };

            return request.CreateResponse<ApiStatusResponse>(HttpStatusCode.InternalServerError, error);

        }

    }
}
