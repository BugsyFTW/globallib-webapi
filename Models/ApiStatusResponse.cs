using System;
using System.Web;

namespace GlobalLib.WebApi.Models
{
    public class ApiStatusResponse : IApiResponse
    {
        private string _Type;

        public string Code { get; set; }
        public string Source { get; set; }

        public string Type {
            get
            {
                return _Type;
            }
            set
            {
                string val = value.Trim().ToLowerInvariant();

                if ( (val == "error") ||  (val=="status") || (val == "information"))
                {
                    _Type = value;
                }
                else
                {
                    throw new Exception("Type must be Status, Information or Error");
                }
            }
        }
        public string Title { get; set; }
        public string Detail { get; set; }

        public object Data { get; set; }

        public ApiStatusResponse()
        {
            //Source = HttpContext.Current.Request.Url.AbsoluteUri;
        }
    }
}