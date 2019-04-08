using GlobalLib.Database;
using GlobalLib.WebApi.Models;
using GlobalLib.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Dynamic;
using System.ComponentModel;
using System.Web.Http.ModelBinding;


namespace GlobalLib.WebApi.Controllers
{
    public delegate HttpResponseMessage DoGetHandler();

    /// <summary>
    /// Classe a injectar em todos os controladores de API
    /// Contém os métodos e estruturas de dados necessários ao processamento de pedidos de api.
    /// </summary>
    public class ControllerLogic 
    {
        // lista de campos pelos quais se pode filtrar na querystring
        public AutorizedFieldList FieldsAllowedForQuery = new AutorizedFieldList();

        // lista de campos pelos quais se pode ordenar na querystring
        public AutorizedFieldList FieldsAllowedForOrder = new AutorizedFieldList();

        // comandos suportados na querystring
        public List<string> CommandsAllowedForQuery = new List<string>();


        // lista de campos de filtro, de ordem e comandos que foram encontrados na querystring 
        // e correspondentes valores
        public Dictionary<string, AutorizedField> MatchedFields;
        public Dictionary<string, string> MatchedCommands;
        public Dictionary<string, OrderByField> MatchedOrderFields;

        // lista de comandos e campos na querystring
        public Dictionary<string, string> QueryFields;
        public Dictionary<string, string> QueryCommands;

        // campos de ordenação
        public Dictionary<string, OrderByField> OrderByFields;

        // Estrutura genérica dos valores na querystring
        protected IEnumerable<KeyValuePair<string, string>> QueryStringPairs;

        // eventos que podem ser capturados nos pedidos de GET
        public event DoGetHandler DoGetAll;
        public event DoGetHandler DoGetByQueryString;

        // se o pedido contém comando de paging
        public bool HasPagingCommand { get; set; }
        // tamanho de pagina
        public int PageSize { get; set; }
        // número de pagina
        public int PageNumber { get; set; }

        // podemos limitar o número de campos a processar na querystring
        public int MaxNumberFieldsOnQueryString { get; set; }

        // referência ao controller de api em uso
        protected ApiController Controller;

        // retira o UserID (primarysid) do token de autenticação
        public int UserID
        {
            get
            {
                var identity = (ClaimsIdentity)Controller.User.Identity;
                IEnumerable<Claim> claims = identity.Claims;

                if (claims.IsNullOrEmpty())
                {
                    return -1;
                }

                return Convert.ToInt32(claims.First(x => x.Type.ToLowerInvariant().Contains("primarysid")).Value);
            }
            set
            {

            }
        }

        // retira o username do utilizador corrente de identity
        public string UserName
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }

        // tipo de autenticação utilizado
        public string AuthType
        {
            get
            {
                var identity = (ClaimsIdentity)Controller.User.Identity;
                IEnumerable<Claim> claims = identity.Claims;

                if (claims.IsNullOrEmpty())
                {
                    return "";
                }

                return claims.First(x => x.Type.ToLowerInvariant().Contains("role")).Value;
            }
            set
            {

            }
        }

        // quantidade campos de filtragem encontrados
        public int MatchedFieldsCount
        {
            get
            {
                int res = 0;
                if (MatchedFields != null)
                {
                    res = MatchedFields.Count();
                }
                return res;
            }
        }

        // quantidade campos de ordem validos encontrados na querystring
        public int MatchedOrderFieldsCount
        {
            get
            {
                int res = 0;
                if (MatchedOrderFields != null)
                {
                    res = MatchedOrderFields.Count();
                }
                return res;
            }
        }

        // quantidade campos de comando validos encontrados na querystring
        public int MatchedCommandsCount
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

        /// <summary>
        /// Construtor da classe
        /// Recebe como parametro o controller que vai ser utilizado e a o tamanho padrão de pagina a utilizar
        /// </summary>
        /// <param name="currentController">ApiController em uso</param>
        /// <param name="DefaultPageSize">Tamanho default das paginas de resultado. Por defeito está 25 linhas</param>
        public ControllerLogic(ApiController currentController, int DefaultPageSize = 25) 
        {
            Controller = currentController;
            DefaultCommandsAllowedForQueryString();

            MaxNumberFieldsOnQueryString = 0;

            HasPagingCommand = false;
            PageSize = DefaultPageSize;
            PageNumber = 1;
        }

        /// <summary>
        /// Resposta standard para pedidos com recurso não encontrado
        /// </summary>
        /// <returns>HTTP Response: Resource Not Found</returns>
        public HttpResponseMessage ResourceNotFound()
        {
            ApiStatusResponse errorResponse = new ApiStatusResponse()
            {
                Code = "001",
                Detail = "Recursos não encontrados",
                Title = "Recursos não encontrados",
                Type = "error"
            };

            return Controller.Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.NotFound, errorResponse);
        }

        /// <summary>
        /// Tratamento de erros por default
        /// </summary>
        /// <param name="request">Pedido HTTP</param>
        /// <param name="ex">Erro a tratar</param>
        /// <returns>HTTP Response: erro e mensagem</returns>
        public HttpResponseMessage DefaultControllerException(HttpRequestMessage request, Exception ex)
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

        /// <summary>
        /// Processamento do evento de pedido de querystring
        /// </summary>
        /// <returns>O resultado do processamento do evento</returns>
        public HttpResponseMessage OnDoGetByQueryString()
        {
            HttpResponseMessage response = null;

            if (DoGetByQueryString != null)
            {
                if( (MatchedFields!=null) || (MatchedCommands!=null) )
                {
                    response = DoGetByQueryString();
                }
                
            }
            else
            {
                // retorna erro
                ApiStatusResponse errorResponse = new ApiStatusResponse()
                {
                    Code = "000",
                    Detail = "Operação indisponível.",
                    Title = "Operação indisponível.",
                    Type = "error"
                };

                response = Controller.Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, errorResponse);
            }


            return response;
        }

        /// <summary>
        /// Processamento do evento de pedido de get do tipo Get_ALL
        /// </summary>
        /// <returns>O resultado do processamento do evento</returns>
        public HttpResponseMessage OnDoGetAll()
        {
            HttpResponseMessage response = null;

            if (DoGetAll != null)
            {
                response = DoGetAll();
            }
            else
            {
                // retorna erro
                ApiStatusResponse errorResponse = new ApiStatusResponse()
                {
                    Code = "000",
                    Detail = "Operação indisponível.",
                    Title = "Operação indisponível.",
                    Type = "error"
                };

                response = Controller.Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, errorResponse);
            }
                

            return response;

        }


        /// <summary>
        /// Constroi ou reconstroi a lista standard de comandos autorizados na querystring
        /// Os comados podem ser retirados ou adicionados
        /// Os comandos por defeito são
        /// @limit = número de registos por paginas
        /// @page = pagina de resultados
        /// @order = seguido do nome dos campos, separados por virgula representa os campos e modo de ordenação
        ///     exe: @order=nome,-idade
        ///     ordena ascendente pelo nome e descendente (-) pela idade
        ///     
        /// </summary>
        public void DefaultCommandsAllowedForQueryString()
        {
            if (CommandsAllowedForQuery != null)
            {
                CommandsAllowedForQuery = null;
            }
            CommandsAllowedForQuery = new List<string>();

            CommandsAllowedForQuery.Add("@limit");
            CommandsAllowedForQuery.Add("@page");
            CommandsAllowedForQuery.Add("@order");
        }

        /// <summary>
        /// Baseado numa classe, adiciona as propriadades (nome e valor) à lista de campos indicada
        /// </summary>
        /// <typeparam name="T">Classe/Tipo</typeparam>
        /// <param name="list">Lista onde vai ser adicionada</param>
        private void AddDefaultAutorized<T>(ref AutorizedFieldList list) where T : class
        {
            // criamos uma instancia  do objecto para poder consultar as propriedades
            var Model = (T)Activator.CreateInstance(typeof(T), new object[] { });

            // limpar
            list = null;

            list = new AutorizedFieldList();
            //lista os campos do modelo que admitimos utilizar na consulta
            foreach (var prop in Model.GetType().GetProperties())
            {
                string fieldName = prop.Name.ToLowerInvariant();
                list.Add(fieldName, prop.Name);
            }
        }


        /// <summary>
        /// Definir campos autorizados na querysytring, baseada numa classe modelo
        /// todos os campos do modelo são adicionados como autorizados.
        /// </summary>
        public void DefaultFieldsAllowedForQuery<T>() where T : class
        {
            AddDefaultAutorized<T>(ref FieldsAllowedForQuery);
        }

        /// <summary>
        /// Definir campos autorizados para ordenação, baseada numa classe modelo
        /// todos os campos do modelo são adicionados como autorizados.
        /// </summary>
        public void DefaultFieldsAllowedForOrder<T>() where T : class
        {
            AddDefaultAutorized<T>(ref FieldsAllowedForOrder);
        }

        /// <summary>
        /// Validação e processamento da querystring
        /// Neste método está toda a inteligencia necessária para separar e preparar 
        /// a querystring para execução do controller
        /// </summary>
        /// <returns> Erros a retornar pela API</returns>
        public ApiStatusResponse ValidateQryString()
        {
            QueryStringPairs = null;
            QueryStringPairs = Controller.Request.GetQueryNameValuePairs();

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

                return errorResponse;
            }

            // se não têm querystring
            if (QueryStringPairs.Count() <= 0)
            {
                // não há nada a validar
                return null;
            }

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
            MatchedFields = null;
            
            // temos campos, vamos ver se são da lista de campos
            // válidos
            if (!QueryFields.IsNullOrEmpty())
            {
                var tempMatchedFields = (from itemField in QueryFields
                                     where FieldsAllowedForQuery.Find(itemField.Key.ToLowerInvariant()) != null
                                     select itemField).ToDictionary(kv => kv.Key, kv => kv.Value,
                                                            StringComparer.OrdinalIgnoreCase);

                MatchedFields = new Dictionary<string, AutorizedField>();

                foreach (var match in tempMatchedFields)
                {
                    var foundField = FieldsAllowedForQuery.Find(match.Key.ToLowerInvariant());

                    if((match.Value.ToString().IndexOf('§') >= 0) )
                    {
                        foundField.Value = match.Value.Replace('§', '%');
                    }
                    else
                    {
                        foundField.Value = match.Value;
                    }
                    

                    MatchedFields.Add(match.Key, foundField);
                }
            }

            if ((MatchedFields != null) && (MatchedFields.Count<KeyValuePair<string, AutorizedField>>() == 0))
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
            MatchedCommands = null;

            // temos comandos, vamos ver se pertencem à lista de comandos válidos
            if (!QueryCommands.IsNullOrEmpty())
            {
                MatchedCommands = (from itemCommand in QueryCommands
                                   where CommandsAllowedForQuery.Contains(itemCommand.Key.ToLowerInvariant())
                                   select itemCommand).ToDictionary(kv => kv.Key, kv => kv.Value,
                                                            StringComparer.OrdinalIgnoreCase); 
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

                return errorResponse;
            }


            // tem comandos na qyerystring?
            if (!QueryCommands.IsNullOrEmpty())
            {
                // mas não são válidos ou tem a mais
                if (!hasValidCommands | (MatchedCommands.Count() != QueryCommands.Count() ))
                {
                    MatchedCommands.Clear();
                    ApiStatusResponse errorResponse = new ApiStatusResponse()
                    {
                        Code = "100",
                        Detail = "Está a tentar executar uma querystring inválida, utilizando comandos não suportados",
                        Title = "Querystring inválida",
                        Type = "error"
                    };

                    return errorResponse;
                }
                else
                {
                    // comandos válidos encontrados, vamos validar os comandos standard os restantes não são validados
                    //  @limit
                    //  @page
                    //  @order
                    foreach (var item in MatchedCommands)
                    {
                        string command = item.Key.ToLowerInvariant();

                        switch (command)
                        {
       
                            case "@order":
                                {
                                    if ((FieldsAllowedForOrder == null) || (FieldsAllowedForOrder.Count <= 0))
                                    {
                                        ApiStatusResponse errorResponse = new ApiStatusResponse()
                                        {
                                            Code = "100",
                                            Detail = "Está a tentar executar uma querystring inválida, utilizando campos para ordenação não suportados",
                                            Title = "Querystring inválida",
                                            Type = "error"
                                        };

                                        return errorResponse;
                                    }

                                    OrderByFields = null;
                                    OrderByFields = new Dictionary<string, OrderByField>();

                                    string[] fields = item.Value.Split(',');

                                    foreach (string s in fields)
                                    {
                                        OrderByField field = new OrderByField() { FieldName = s };
                                        OrderByFields.Add(field.FieldName, field);
                                    }

                                    if (!OrderByFields.IsNullOrEmpty())
                                    {
                                        MatchedOrderFields = (from itemCommand in OrderByFields
                                                              where FieldsAllowedForOrder.Find(itemCommand.Key.ToLowerInvariant()) != null
                                                              select itemCommand).ToDictionary(kv => kv.Key, kv => kv.Value,
                                                                                    StringComparer.OrdinalIgnoreCase);
                                    }

                                    if ((MatchedOrderFields != null) && (MatchedOrderFields.Count<KeyValuePair<string, OrderByField>>() == 0))
                                    {
                                        MatchedOrderFields = null;
                                    }

                                    var hasValidOrderFields = !MatchedOrderFields.IsNullOrEmpty();

                                    if (!hasValidOrderFields || (MatchedOrderFields.Count() != OrderByFields.Count()))
                                    {
                                        ApiStatusResponse errorResponse = new ApiStatusResponse()
                                        {
                                            Code = "100",
                                            Detail = "Está a tentar executar uma querystring inválida, utilizando campos para ordenação não suportados",
                                            Title = "Querystring inválida",
                                            Type = "error"
                                        };

                                        return errorResponse;
                                    }


                                    break;
                                }


                            // comandos de paginação
                            case "@limit":
                            case "@page" :  
                                {
                                    HasPagingCommand = true;

                                    if (command == "@limit")
                                    {
                                        try
                                        {
                                            PageSize = Convert.ToInt32(item.Value);


                                            if (PageSize <= 0)
                                            {
                                                throw new ArgumentException();
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            ApiStatusResponse errorResponse = new ApiStatusResponse()
                                            {
                                                Code = "100",
                                                Detail = "Está a tentar executar uma querystring inválida, utilizando um valor invalido para o comando @limit",
                                                Title = "Querystring inválida",
                                                Type = "error"
                                            };

                                            return errorResponse;
                                        }
                                        
                                    }

                                    if (command == "@page")
                                    {
                                        try
                                        {
                                            PageNumber = Convert.ToInt32(item.Value);

                                            if(PageNumber<=0)
                                            {
                                                throw new ArgumentException();
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            ApiStatusResponse errorResponse = new ApiStatusResponse()
                                            {
                                                Code = "100",
                                                Detail = "Está a tentar executar uma querystring inválida, utilizando um valor invalido para o comando @page",
                                                Title = "Querystring inválida",
                                                Type = "error"
                                            };

                                            return errorResponse;
                                        }
                                        
                                    }

                                    break;
                                }

                            default:
                                {
                                    if (!item.Value.IsNaturalNumber())
                                    {
                                        ApiStatusResponse errorResponse = new ApiStatusResponse()
                                        {
                                            Code = "100",
                                            Detail = $"Está a tentar executar uma querystring inválida, com  valores dos comandos @limit, @offset e @pagenumber invlálidos. Valor: {item.Value}",
                                            Title = "Querystring inválida",
                                            Type = "error"
                                        };

                                        return errorResponse;
                                    }
                                    break;
                                }
                        }

                    }

                }

            }

            // tem campos os na query string?
            if (!QueryFields.IsNullOrEmpty())
            {
                // erro se não têm campos válidos ou se além de válidos tem outros não reconhecidos
                if (!hasValidFields || (MatchedFields.Count() != QueryFields.Count()))
                {
                    ApiStatusResponse errorResponse = new ApiStatusResponse()
                    {
                        Code = "100",
                        Detail = "Está a tentar executar uma querystring inválida, utilizando campos não suportados",
                        Title = "Querystring inválida",
                        Type = "error"
                    };

                    return errorResponse;
                }

                // temos campos, mas só vamos permitir MaxNumberFieldsOnQueryString
                // se este for maior que 0
                if ((MaxNumberFieldsOnQueryString > 0) && (MatchedFieldsCount > MaxNumberFieldsOnQueryString))
                {
                    ApiStatusResponse errorResponse = new ApiStatusResponse()
                    {
                        Code = "100",
                        Detail = $"Não é possivel executar porque a querystring. Só pode filtrar por {MaxNumberFieldsOnQueryString} campo(s) de cada vez.",
                        Title = "Querystring inválida",
                        Type = "error"
                    };

                    return errorResponse;

                }
            }


            return null;
        }



        public static T GetTfromString<T>(string mystring)
        {
            var foo = TypeDescriptor.GetConverter(typeof(T));
            return (T)(foo.ConvertFromInvariantString(mystring));
        }

        /// <summary>
        /// Utiliza o modelo da classe e os campos autorizados e encontrados na querystring 
        /// para construir a estrutura de filtragem
        /// </summary>
        /// <typeparam name="T">Classe Modelo</typeparam>
        /// <returns>Estrutura com os campos e valores de filtragen</returns>
        public List<WhereParameter> BuildWhereParams<T>()
        {

            // se não temos campos encontrados/validos não há nada a fazer
            if (MatchedFields.IsNullOrEmpty())
            {
                return null;
            }

            // cria uma instancia da classe de modelo para ler as propriedades
            var Model = (T)Activator.CreateInstance(typeof(T), new object[] { });

            var modelProps = Model.GetType().GetProperties();

            List<WhereParameter> whereParameters = new List<WhereParameter>();

            // itera por cada campo de filtragem, encontra a propriedade equivalente e constroi a estrutura com o nome e valor
            foreach (var item in MatchedFields)
            {
                Type paramType = null;

                // 2018.03.02 - pmatos
                // se é de um tipo nulable (por exemplo int?), o que nos interessa é o tipo genérico
                paramType = modelProps.First(x => x.Name.ToLowerInvariant() == item.Key.ToLowerInvariant()).PropertyType.GenericTypeArguments.FirstOrDefault();
                if(paramType == null)
                {
                    // ok, não é nullable, logo é "normal"
                    paramType = modelProps.First(x => x.Name.ToLowerInvariant() == item.Key.ToLowerInvariant()).PropertyType;
                }

                var paraValue = Convert.ChangeType(item.Value.Value, paramType);

                whereParameters.Add(new WhereParameter() {
                    ParamName = item.Value.FieldName ?? item.Value.QueryFieldName,
                    FieldName = item.Value.QueryFieldName ?? item.Value.FieldName,
                    Value = paraValue
                });
            }

            return whereParameters;
        }

        /// <summary>
        /// Get Generico
        /// Trata o GetAll e Get com QueryString
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage ProcessQueryString()
        {
            HttpResponseMessage response;

            var QueryStringValidationResponse = ValidateQryString();
            if (QueryStringValidationResponse != null)
            {
                response = Controller.Request.CreateResponse<ApiStatusResponse>(HttpStatusCode.BadRequest, QueryStringValidationResponse);
            }
            else
            {
                try
                {
                    if((MatchedFields == null) && (MatchedCommands == null))
                    {
                        response = OnDoGetAll();
                    }
                    else
                    {
                        response = OnDoGetByQueryString();
                    }

                    
                }
                catch (Exception ex)
                {
                    response = DefaultControllerException(Controller.Request, ex);
                }
            }
            return response;
        }

        /// <summary>
        /// Constroi a string de ordem, baseada nos paramentros e valores lidos da querystring
        /// Tem em consideração se é ascendente ou descendente
        /// </summary>
        /// <returns></returns>
        public string GetOrderForQuery()
        {
            string orderBy = "";
            foreach (var item in MatchedOrderFields)
            {
                if (orderBy != "")
                {
                    orderBy += ",";
                }

                AutorizedField param = FieldsAllowedForOrder.Find(item.Key);
                orderBy += $"{param.QueryFieldName} {item.Value.SortOrder}".Trim();
            }
            return orderBy;
        }

        public List<OrderByField> GetOrderByFieldsList()
        {
            if (MatchedOrderFields.IsNullOrEmpty())
                return null;

            List<OrderByField> ret = new List<OrderByField>();

            foreach (var item in MatchedOrderFields)
            {
                //AutorizedField param = FieldsAllowedForOrder.Find(item.Key);
                ret.Add(item.Value);
            }
            return ret;
        }

        /// <summary>
        /// Executa a ordenação numa lista de resultados
        /// Utiliza o OrderBy do LINQ for Objects
        /// </summary>
        /// <typeparam name="TAnyClass">Classe modelo</typeparam>
        /// <param name="listAll">Lista de resultados a ordenar</param>
        /// <returns></returns>
        public IEnumerable<TAnyClass> ExecuteOrderCommand<TAnyClass>(IEnumerable<TAnyClass> listAll) where TAnyClass : class
        {
            IEnumerable<TAnyClass> retVal;

            retVal = listAll;

            // já temos os resultados, temos de ordenar?
            if (!MatchedOrderFields.IsNullOrEmpty() && (MatchedOrderFields.Count > 0))
            {
                string order = GetOrderForQuery();

                var listOrdered = listAll.AsQueryable().OrderBy(order);

                retVal = listOrdered;
            }

            return retVal;
        }

        /// <summary>
        /// Valida o modelo e constroi a lista de erros
        /// </summary>
        /// <param name="ModelState">Modelo</param>
        /// <returns>Lista de Erros</returns>
        public HttpResponseMessage getErrors(ModelStateDictionary ModelState)
        {
            var errors = new List<string>();
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            return Controller.Request.CreateResponse(HttpStatusCode.BadRequest, errors);
        }

    }
}
