using Dapper.Contrib.Extensions;
using GlobalLib.Database;
using GlobalLib.WebApi.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace GlobalLib.WebApi.Controllers
{

    /// <summary>
    /// Creates the base controler for direct crud operations
    /// <typeparam name="T"></typeparam>
    public abstract class BaseApiControllerDbCRUD<T> : BaseApiControllerCRUD<T> where T : class
    {
        protected DbTable<T> tblData;

        protected override HttpResponseMessage DoGet(int id)
        {
            HttpResponseMessage response = null;

            try
            {
                var record = tblData.Get(id);
                ApiDataResponse res = new ApiDataResponse();
                res.data = record;
                res.recordsaffected = 1;
                response = Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                response = ExceptionHandler(ex);
            }

            return response;
        }

        protected override HttpResponseMessage DoGetAll()
        {
            var listAll = tblData.GetAll(null);
            ApiDataResponse res = new ApiDataResponse();
            res.data = listAll;
            res.recordsaffected = listAll.Count();
            return Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);
        }



        protected override HttpResponseMessage DoPost(T item)
        {
            HttpResponseMessage response = null;

            try
            {
                long itemid = 0;
                var itemIns = tblData.Insert(item, out itemid);

                ApiDataResponse res = new ApiDataResponse();
                res.data = itemIns;

                res.recordsaffected = 1;

                response = Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);
                string uri = Url.Link("DefaultApi", new { id = itemid });
                response.Headers.Location = new Uri(uri);
            }
            catch (Exception ex)
            {
                response = ExceptionHandler(ex);
            }

            return response;
        }

        protected override HttpResponseMessage DoPut(int Id, T item)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            // descobrir campo chave e saber valor
            // se o valor do parametro nao for igual ao do campo chave
            // é erro, logo nem avança
            if (GetKeyValueOf(item) != Id)
            {
                ApiStatusResponse errorResponse = new ApiStatusResponse()
                {
                    Code = "101",
                    Detail = "Está a tentar executar a actulização de um objecto, mas tem incomsistência entre a chave do objecto e o ID do Uri",
                    Title = "Uri Inválido",
                    Type = "error"
                };

                response = Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
            }
            else
            {

                var itemIns = tblData.Update(item);

                if (itemIns == null)
                {
                    // não update, mas não deu erro... portanto elemento não existe
                    

                    ApiStatusResponse errorResponse = new ApiStatusResponse()
                    {
                        Code = "102",
                        Detail = "Está a tentar executar a actulização de um objecto, mas o objecto não existe",
                        Title = "Operação Inválida",
                        Type = "error"
                    };

                    response = Request.CreateResponse(HttpStatusCode.NotFound, errorResponse);
                }
                else
                {
                    ApiDataResponse res = new ApiDataResponse();
                    res.data = item;
                    res.recordsaffected = 1;

                    response = Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);
                    string uri = Url.Link("DefaultApi", new { id = Id });
                    response.Headers.Location = new Uri(uri);
                }

            }
            return response;
        }


        public override HttpResponseMessage DoDelete(int Id)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                var item = tblData.Get(Id);
                tblData.Delete(item);
                response = Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                response = ExceptionHandler(ex);
            }
            return response;
        }
    }
}
