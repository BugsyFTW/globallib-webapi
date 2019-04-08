using GlobalLib.Extensions;
using GlobalLib.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace GlobalLib.WebApi.Controllers
{
    public abstract class BaseApiController : ApiController
    {
        protected List<string> FieldsAllowedForQuery = new List<string>();
        protected List<string> CommandsAllowedForQuery = new List<string>();

        protected Dictionary<string, string> MatchedFields;
        protected Dictionary<string, string> MatchedCommands;

       protected IEnumerable<KeyValuePair<string, string>> QueryStringPairs;

        protected int MatchedFieldsCount
        {
            get
            {
                int res = 0;
                if (MatchedFields!=null)
                {
                    res = MatchedFields.Count(); 
                }
                return res;
            }
        }

        protected int MatchedCommandsCount
        {
            get
            {
                int res = 0;
                if (MatchedCommands != null)
                {
                    res = MatchedCommands.Count();
                }
                return res;
            }
        }

        public int UserID
        {
            get
            {
                var identity = (ClaimsIdentity)User.Identity;
                IEnumerable<Claim> claims = identity.Claims;

                return Convert.ToInt32(claims.First(x => x.Type.ToLowerInvariant().Contains("primarysid")).Value);
            }
        }
        public string UserName
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }


        /// <summary>
        /// Definir campos autorizados na querysytring
        /// </summary>
        protected virtual void DefineFieldsAllowedForQuery()
        {
            return;
        }

        /// <summary>
        /// Por defeito, autorizamos os seguintes comandos.
        /// 
        /// Para limitar os campos basta fazer override deste metodo
        /// No override limpar a lista e/ou adicionar só os que são necessários.
        /// </summary>
        protected virtual void DefineCommandsAllowedForQuery()
        {
            if (CommandsAllowedForQuery != null)
            {
                CommandsAllowedForQuery = null;
            }
            CommandsAllowedForQuery = new List<string>();

            CommandsAllowedForQuery.Add("@limit");
            CommandsAllowedForQuery.Add("@offset");
            CommandsAllowedForQuery.Add("@pagenumber");
            CommandsAllowedForQuery.Add("@order");
        }



        public virtual HttpResponseMessage Get(int id)
        {
            return DoGet(id);
        }

        /// <summary>
        /// Get Generico
        /// Trata o GetAll e Get com QueryString
        /// </summary>
        /// <returns></returns>
        public virtual HttpResponseMessage Get()
        {
            HttpResponseMessage response = null;

            if (QueryStringPairs != null)
            {
                QueryStringPairs = null;
            }

            QueryStringPairs = Request.GetQueryNameValuePairs();

            // se não têm querystring
            if (QueryStringPairs.Count() <= 0)
            {
                // standard get all
                try
                {
                    response = DoGetAll();
                }
                catch (Exception ex)
                {
                    response = ExceptionHandler(ex);
                }

            }
            else
            {
                // tem querystring mas é inválida
                if (QueryStringPairs == null)
                {
                    // retorna erro
                    ApiStatusResponse errorResponse = new ApiStatusResponse()
                    {
                        Code = "100",
                        Detail = "Está a tentar executar uma querystring inválida",
                        Title = "Querystring inválida",
                        Type = "error"
                    };

                    return Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, errorResponse);
                }

                // trata a query string
                try
                {

                    // a querystring é válida

                    // definir campos e comandos válidos
                    DefineFieldsAllowedForQuery();
                    DefineCommandsAllowedForQuery();


                    // agora vamos ver se temos campos na querystring
                    // exemplo: cmoeda (os campos não têm prefixo. só os comandos)
                    var QueryFields = from itemCommand in QueryStringPairs
                                      where !itemCommand.Key.ToLowerInvariant().StartsWith("@")
                                      select itemCommand;

                    if ((QueryFields != null) && (QueryFields.Count<KeyValuePair<string, string>>() == 0))
                    {
                        QueryFields = null;
                    }

                    // limpar
                    if (MatchedFields != null)
                    {
                        MatchedFields = null;
                    }

                    // temos campos, vamos ver se são da lista de campos
                    // válidos
                    if (!QueryFields.IsNullOrEmpty())
                    {
                        MatchedFields = (from itemField in QueryFields
                                        where FieldsAllowedForQuery.Contains(itemField.Key.ToLowerInvariant())
                                        select itemField).ToDictionary(kv => kv.Key, kv => kv.Value,
                                                                    StringComparer.OrdinalIgnoreCase);
                    }

                    if ((MatchedFields != null) && (MatchedFields.Count<KeyValuePair<string, string>>() == 0))
                    {
                        MatchedFields = null;
                    }


                    // agora vamos ver se temos comandos na querystring
                    // os comandos são identificados pelo perfixo @
                    // exemplo: @page
                    var QueryCommands = from itemCommand in QueryStringPairs
                                        where itemCommand.Key.ToLowerInvariant().StartsWith("@")
                                        select itemCommand;

                    if ((QueryCommands != null) && (QueryCommands.Count<KeyValuePair<string, string>>() == 0))
                    {
                        QueryCommands = null;
                    }

                    // limpar
                    if (MatchedCommands != null)
                    {
                        MatchedCommands = null;
                    }

                    // temos comandos, vamos ver se pertencem à lista de comandos válidos

                    if (!QueryCommands.IsNullOrEmpty())
                    {
                        MatchedCommands = (from itemCommand in QueryCommands
                                          where CommandsAllowedForQuery.Contains(itemCommand.Key.ToLowerInvariant())
                                          select itemCommand).ToDictionary(kv => kv.Key, kv => kv.Value,
                                                                    StringComparer.OrdinalIgnoreCase); ;
                    }

                    if ((MatchedCommands != null) && (MatchedCommands.Count<KeyValuePair<string, string>>() == 0))
                    {
                        MatchedCommands = null;
                    }


                    // se não tem campos e comandos 
                    var hasValidFields = !MatchedFields.IsNullOrEmpty();
                    var hasValidCommands = !MatchedCommands.IsNullOrEmpty();
                    if (!hasValidFields && !hasValidCommands)
                    {

                        ApiStatusResponse errorResponse = new ApiStatusResponse()
                        {
                            Code = "100",
                            Detail = "Está a tentar executar uma querystring inválida. Tem de indicar campos e/ou comandos válidos.",
                            Title = "Querystring inválida",
                            Type = "error"
                        };

                        return Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, errorResponse);
                    }


                    // tem comandos na qyerystring?
                    if (!QueryCommands.IsNullOrEmpty())
                    {
                        // mas não são válidos
                        if (!hasValidCommands)
                        {
                            ApiStatusResponse errorResponse = new ApiStatusResponse()
                            {
                                Code = "100",
                                Detail = "Está a tentar executar uma querystring inválida, utilizando comandos não suportados",
                                Title = "Querystring inválida",
                                Type = "error"
                            };

                            return Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, errorResponse);
                        }
                    }

                    // tem campos na query string?
                    if (!QueryFields.IsNullOrEmpty())
                    {
                        // são válidos?
                        if (!hasValidFields)
                        {
                            ApiStatusResponse errorResponse = new ApiStatusResponse()
                            {
                                Code = "100",
                                Detail = "Está a tentar executar uma querystring inválida, utilizando campos não suportados",
                                Title = "Querystring inválida",
                                Type = "error"
                            };

                            return Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, errorResponse);
                        }
                    }

                    // process querystring, temos comandos ou campos
                    response = DoGetByQueryString();


                }
                catch (Exception ex)
                {
                    response = ExceptionHandler(ex);
                }

            }
            return response;
        }



        /// <summary>
        /// Todas as entidades/itens da coleccção
        /// </summary>
        /// <returns></returns>
        protected virtual HttpResponseMessage DoGetAll()
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

        /// <summary>
        /// Ler a entidade/item com o Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual HttpResponseMessage DoGet(int id)
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


        //public virtual HttpResponseMessage Post()
        //{
        //    return DoPost();
        //}

        protected virtual HttpResponseMessage DoPost()
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


        //public virtual HttpResponseMessage Put()
        //{
        //    return DoPut();
        //}

        protected virtual HttpResponseMessage DoPut()
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


        public virtual HttpResponseMessage Delete(int id)
        {
            HttpResponseMessage response = null;

            if (!DoPreDeleteValidations(id))
            {
                response = Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            else
            {
                response = DoDelete(id);
            }

            return response;
        }

        /// <summary>
        /// All validation logic before delete data
        /// Override as needed
        /// </summary>
        /// <returns></returns>
        protected virtual bool DoPreDeleteValidations(int item)
        {
            return true;
        }

        public virtual HttpResponseMessage DoDelete(int id)
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

        //protected HttpResponseMessage ErrorResponse(HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
        //{
        //    HttpResponseMessage response;
        //    if (listErrors.Count > 0)
        //    {
        //        response = Request.CreateResponse<List<ApiStatusResponse>>(httpStatusCode, listErrors);
        //        listErrors.Clear();
        //    }
        //    else
        //    {
        //        response = Request.CreateResponse(httpStatusCode);
        //    }

        //    return response;
        //}


        protected virtual HttpResponseMessage DoGetByQueryString()
        {
            List<string> retval = new List<string>();
            foreach (var q in QueryStringPairs)
            {
                retval.Add("Key: " + q.Key + " Value: " + q.Value);
            }
            ApiDataResponse res = new ApiDataResponse();
            res.data = retval;
            return Request.CreateResponse<ApiDataResponse>(HttpStatusCode.OK, res);
        }


        /// <summary>
        /// Gere as excepções dentro dos controllers;
        /// </summary>
        /// <param name="ex"></param>
        protected HttpResponseMessage ExceptionHandler(Exception ex)
        {
            ApiStatusResponse error = new ApiStatusResponse
            {
                Code = "500",
                Title = "Internal Error",
                Detail = ex.Message,
                Type = "error"
            };

            return Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.InternalServerError, error);

        }

    }
}
