using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GlobalLib.WebApi.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        private static HttpContent _ResponseAsFile(Stream fileBytes, string fileName, string fileFormat = "PDF")
        {
            HttpResponseMessage httpResp = new HttpResponseMessage();

            var contentLength = fileBytes.Length;

            httpResp.Content = new StreamContent(fileBytes);

            string mediaType = "";
            string defaultExtension = "";

            switch (fileFormat)
            {
                case "WORD":
                    {
                        mediaType = "application/msword";
                        defaultExtension = "doc";
                        break;
                    }
                case "EXCEL":
                    {
                        mediaType = "application/vnd.ms-excel";
                        defaultExtension = "xls";
                        break;
                    }
                case "IMAGE":
                    {
                        mediaType = "image/bmp";
                        defaultExtension = "bmp";
                        break;
                    }
                default:
                    {
                        mediaType = "application/pdf";
                        defaultExtension = "pdf";
                        break;
                    }
            }

            httpResp.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            httpResp.Content.Headers.ContentLength = contentLength;

            ContentDispositionHeaderValue contentDisposition = null;

            if (ContentDispositionHeaderValue.TryParse($"inline; filename={fileName}.{defaultExtension}", out contentDisposition))
            {
                httpResp.Content.Headers.ContentDisposition = contentDisposition;
            }

            return httpResp.Content;
        }
        public static HttpResponseMessage ResponseAsFile(this HttpResponseMessage httpResp, byte[] fileBytes, string fileName, string fileFormat = "PDF")
        {
            httpResp.Content = _ResponseAsFile(new MemoryStream(fileBytes), fileName, fileFormat);
            return httpResp;
        }

        public static HttpResponseMessage ResponseAsFile(this HttpResponseMessage httpResp, Stream fileBytes, string fileName, string fileFormat = "PDF")
        {
            httpResp.Content = _ResponseAsFile(fileBytes, fileName, fileFormat);
            return httpResp;
        }
    }
}
