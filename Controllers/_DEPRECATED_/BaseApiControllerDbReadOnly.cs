using GlobalLib.Database;
using GlobalLib.WebApi.Models;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace GlobalLib.WebApi.Controllers
{
    /// <summary>
    /// Creates the base controler for direct read operations
    /// the Derived class must implement de constructor with indication of the connection string
    /// 
    /// All readonly system tables can safelly implement this controller.
    /// 
    /// All others must be derived from this
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseApiControllerDbReadOnly<T> : BaseApiControllerDbCRUD<T>  where T : class
    {
        public override HttpResponseMessage Post(T item)
        {
            ApiStatusResponse errorResponse;
            errorResponse = new ApiStatusResponse()
            {
                Code = "000",
                Detail = "Operação indisponível.",
                Title = "Operação indisponível.",
                Type = "error"
            };

            return Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, errorResponse);
        }

        public override HttpResponseMessage Put([FromUri] int id, [FromBody] T item)
        {
            ApiStatusResponse errorResponse;
            errorResponse = new ApiStatusResponse()
            {
                Code = "000",
                Detail = "Operação indisponível.",
                Title = "Operação indisponível.",
                Type = "error"
            };

            return Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, errorResponse);
        }

        public override HttpResponseMessage Delete(int id)
        {
            ApiStatusResponse errorResponse;
            errorResponse = new ApiStatusResponse()
            {
                Code = "000",
                Detail = "Operação indisponível.",
                Title = "Operação indisponível.",
                Type = "error"
            };

            return Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, errorResponse);
        }
    }
}
