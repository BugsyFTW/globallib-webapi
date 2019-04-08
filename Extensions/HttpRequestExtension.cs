using Microsoft.Owin;
using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;

namespace GlobalLib.WebApi.Extensions
{
    public static class HttpRequestExtension
    {

        public static IPAddress GetClientIpAddress(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return IPAddress.Parse(((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress);
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                return IPAddress.Parse(((RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name]).Address);
            }

            if (request.Properties.ContainsKey("MS_OwinContext"))
            {
                return IPAddress.Parse(((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress);
            }

            throw new Exception("Client IP Address Not Found in HttpRequest");
        }
    }
}
