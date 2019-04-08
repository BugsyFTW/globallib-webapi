using GlobalLib.Database;
using GlobalLib.Extensions;
using GlobalLib.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace GlobalLib.WebApi.Controllers
{
    /// <summary>
    /// Extensão do controlador de API para acesso a dados da BD
    /// Depende da bibioteca de acesso a dados
    /// </summary>
    public class ControllerLogicCRUD : ControllerLogic
    {
        public DbContext DBContext;


        /// <summary>
        /// Construtor da classe
        /// Recebe como parametro o controller que vai ser utilizado 
        /// e a entrada da connectionstring da ligação à BD
        /// </summary>
        /// <param name="currentController">ApiController em uso</param>
        /// <param name="ConnectionString">Entrada da connection string do WebConfig</param>
        public ControllerLogicCRUD(ApiController currentController, string ConnectionString = "DefaultConnection") : base(currentController)
        {
            DBContext = new DbContext(ConnectionString);
        }

        /// <summary>
        /// Construtor da classe
        /// Recebe como parametro o controller que vai ser utilizado 
        /// o contexto de ligação à BD em uso
        /// </summary>
        /// <param name="currentController">ApiController em uso</param>
        /// <param name="currentDbContext"></param>
        public ControllerLogicCRUD(ApiController currentController, DbContext currentDbContext = null) : base(currentController)
        {
            DBContext = currentDbContext;
        }

        /// <summary>
        /// Executa um GET (SELECT) por ID do registo
        /// </summary>
        /// <typeparam name="T">Modelo da Tabela</typeparam>
        /// <param name="id">ID do registo</param>
        /// <returns>HTTP Response com o registo</returns>
        public virtual HttpResponseMessage GenericGet<T>(int id) where T : class
        {
            DbTable<T> tblData = new DbTable<T>(DBContext, UserID);

            try
            {
                var record = tblData.Get(id);
                ApiDataResponse res = new ApiDataResponse();
                res.data = record;
                res.recordsaffected = 1;
                tblData = null;

                return Controller.Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                tblData = null;
                return DefaultControllerException(Controller.Request, ex);
            }
        }



        /// <summary>
        /// Executa um GET ALL (SELECT da tabela inteira)
        /// </summary>
        /// <typeparam name="T">Modelo da Tabela</typeparam>
        /// <returns>HTTP Response com a tabela, paginada ou não</returns>
        public HttpResponseMessage GenericGetAll<T>() where T : class
        {
            HttpResponseMessage response = ResourceNotFound();

            DbTable<T> tblData = new DbTable<T>(DBContext, UserID);

            var QueryStringValidationResponse = ValidateQryString();

            if (QueryStringValidationResponse != null)
            {
                response = Controller.Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, QueryStringValidationResponse);
            }
            else
            {

                try
                {

                    PagedResults<T> foundItens = null;

                    IEnumerable<T> listAll = null;

                    if (HasPagingCommand)
                    {
                        foundItens = tblData.GetPagedResults(PageSize, PageNumber, GetOrderByFieldsList(), BuildWhereParams<T>());
                    }
                    else
                    {
                        listAll = tblData.GetAll(BuildWhereParams<T>());
                    }


                    if(!listAll.IsNullOrEmpty() || (foundItens != null) )
                    {
                        ApiDataResponse res = new ApiDataResponse();

                        // já temos os resultados, temos de ordenar?
                        if ( !HasPagingCommand && (!MatchedOrderFields.IsNullOrEmpty() && (MatchedOrderFields.Count > 0)) )
                        {
                            string order = GetOrderForQuery();

                            var listOrdered = listAll.AsQueryable().OrderBy(order);

                            listAll = listOrdered;
                        }

                        if(!HasPagingCommand)
                        {
                            res.data = listAll;
                            res.recordsaffected = listAll.Count();
                        }
                        else
                        {
                            res = buildTypedResponse<T>(foundItens, HasPagingCommand);
                        }

                        tblData = null;

                        response = Controller.Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);

                    }

                }
                catch (Exception ex)
                {
                    tblData = null;
                    response = DefaultControllerException(Controller.Request, ex);
                }
            }

            return response;

        }

        /// <summary>
        /// Executa um GET para uma dada query
        /// </summary>
        /// <typeparam name="T">Modelo da Tabela</typeparam>
        /// <param name="sqlQuery">Query SQL</param>
        /// <param name="stringKeyFieldName">Campo de Chave</param>
        /// <returns>HTTP Response com a tabela, paginada ou não</returns>
        public HttpResponseMessage GenericGetAllByQuery<T>(string sqlQuery, string stringKeyFieldName) where T : class
        {
            HttpResponseMessage response = ResourceNotFound();

            DbTable<T> tblData = new DbTable<T>(DBContext, UserID);

            var QueryStringValidationResponse = ValidateQryString();

            if (QueryStringValidationResponse != null)
            {
                response = Controller.Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, QueryStringValidationResponse);
            }
            else
            {

                try
                {

                    PagedResults<T> foundItens = null;

                    IEnumerable<T> listAll = null;

                    if (HasPagingCommand)
                    {
                        foundItens = tblData.CustomQueryPaged<T>(sqlQuery, stringKeyFieldName, GetOrderByFieldsList(), PageSize, PageNumber, BuildWhereParams<T>());
                    }
                    else
                    {
                        listAll = tblData.CustomQuery<T>(sqlQuery, stringKeyFieldName, GetOrderByFieldsList(), BuildWhereParams<T>());
                    }


                    if (!listAll.IsNullOrEmpty() || (foundItens != null))
                    {
                        ApiDataResponse res = new ApiDataResponse();

                        // já temos os resultados, temos de ordenar?
                        if (!HasPagingCommand && (!MatchedOrderFields.IsNullOrEmpty() && (MatchedOrderFields.Count > 0)))
                        {
                            string order = GetOrderForQuery();

                            var listOrdered = listAll.AsQueryable().OrderBy(order);

                            listAll = listOrdered;
                        }

                        if (!HasPagingCommand)
                        {
                            res.data = listAll;
                            res.recordsaffected = listAll.Count();
                        }
                        else
                        {
                            res = buildTypedResponse<T>(foundItens, HasPagingCommand);
                        }

                        tblData = null;

                        response = Controller.Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);

                    }

                }
                catch (Exception ex)
                {
                    tblData = null;
                    response = DefaultControllerException(Controller.Request, ex);
                }
            }

            return response;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public HttpResponseMessage GenericPost<T>(T item) where T : class
        {
            HttpResponseMessage response;
            DbTable<T> tblData = new DbTable<T>(DBContext, UserID);

            try
            {
                long itemid = 0;
                var itemIns = tblData.Insert(item, out itemid);

                ApiDataResponse res = new ApiDataResponse();
                res.data = itemIns;

                res.recordsaffected = 1;

                response = Controller.Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);

                // Erro no parametro request
                //UrlHelper url = new UrlHelper();
                //string uri = url.Link("DefaultApi", new { id = itemid });

                //response.Headers.Location = new Uri(uri);

            }
            catch (Exception ex)
            {
                tblData = null;
                response =  DefaultControllerException(Controller.Request, ex);
            }
            return response;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Id"></param>
        /// <param name="item"></param>
        /// <param name="activeTransaction"></param>
        /// <returns></returns>
        public HttpResponseMessage GenericPut<T>(int Id, T item, IDbTransaction activeTransaction = null) where T : class
        {
            HttpResponseMessage response;
            // descobrir campo chave e saber valor
            // se o valor do parametro nao for igual ao do campo chave
            // é erro, logo nem avança
            if (DatabaseHelpers.GetKeyValueOf(item) != Id)
            {
                ApiStatusResponse errorResponse = new ApiStatusResponse()
                {
                    Code = "101",
                    Detail = "Está a tentar executar a actulização de um objecto, mas tem incomsistência entre a chave do objecto e o ID do Uri",
                    Title = "Uri Inválido",
                    Type = "error"
                };

                response = Controller.Request.CreateResponse(HttpStatusCode.BadRequest, errorResponse);
            }
            else
            {
                DbTable<T> tblData = new DbTable<T>(DBContext, UserID);

                try
                {
                    var itemIns = tblData.Update(item, activeTransaction);
                    tblData = null;

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

                        response = Controller.Request.CreateResponse(HttpStatusCode.NotFound, errorResponse);
                    }
                    else
                    {
                        ApiDataResponse res = new ApiDataResponse();
                        res.data = item;
                        res.recordsaffected = 1;

                        response = Controller.Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);

                        // Erro no parametro request
                        //UrlHelper url = new UrlHelper();
                        //string uri = url.Link("DefaultApi", new { id = Id });

                        //response.Headers.Location = new Uri(uri);
                    }

                }
                catch (Exception ex)
                {
                    tblData = null;
                    response = DefaultControllerException(Controller.Request, ex);
                }

                tblData = null;
            }
            return response;

        }

        
        /// <summary>
        /// Constroi a resposta de acordo com os resultados e processamentos
        /// Executa a ordenação no caso de não ter paginação
        /// </summary>
        /// <typeparam name="TAnyClass">Classe com o modelo de dados da resposta</typeparam>
        /// <param name="foundItens">Lista de dados encontrados e a retornar</param>
        /// <param name="hasPages"></param>
        /// <returns></returns>
        public ApiDataResponse buildTypedResponse<TAnyClass>(PagedResults<TAnyClass> foundItens, bool hasPages = false) where TAnyClass : class
        {

            ApiDataResponse returnValue = new ApiDataResponse();

            // se não tem paginação
            // A ordenação não é feita na BD
            // é faita aqui.
            if ((MatchedCommands != null) && (!HasPagingCommand))
            {
                foundItens.Records = ExecuteOrderCommand<TAnyClass>(foundItens.Records);
            }

            returnValue.data = foundItens.Records;

            if (hasPages)
            {
                returnValue.pagination = foundItens.PageInfo;
                returnValue.recordsaffected = foundItens.PageInfo.CurrentPageSize;
            }
            else
            {
                returnValue.recordsaffected = foundItens.Records.Count();
            }

            return returnValue;
        }


        /// <summary>
        /// Apaga um registo da BD com base no id do registo
        /// </summary>
        /// <typeparam name="T">Modelo da tabela</typeparam>
        /// <param name="Id">ID do registo</param>
        /// <returns></returns>
        public HttpResponseMessage GenericDelete<T>(int Id) where T : class
        {

            HttpResponseMessage response;

            DbTable<T> tblData = new DbTable<T>(DBContext, UserID);

            try
            {
                var item = tblData.Get(Id);
                tblData.Delete(item);
                response = Controller.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                response = DefaultControllerException(Controller.Request, ex);
            }

            tblData = null;
            return response;
            
        }


        /// <summary>
        /// Apagar um registo da BD, com base num registo
        /// </summary>
        /// <typeparam name="T">Modelo da tabela</typeparam>
        /// <param name="item">Registo a eliminar</param>
        /// <returns></returns>
        public HttpResponseMessage GenericDelete<T>(T item) where T : class
        {

            HttpResponseMessage response;

            DbTable<T> tblData = new DbTable<T>(DBContext, UserID);

            try
            {
                tblData.Delete(item);
                response = Controller.Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                response = DefaultControllerException(Controller.Request, ex);
            }

            tblData = null;
            return response;
        }


    }
}
