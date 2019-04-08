using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobalLib.WebApi.Models
{
    public class ApiDataResponse : IApiResponse
    {
        public object data { get; set; }
        public object pagination { get; set; }
        public object meta { get; set; }

        public int recordsaffected { get; set; }

        public string uri { get; set; }
    }
}