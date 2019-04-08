using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Reflection;
using Dapper.Contrib.Extensions;

namespace GlobalLib.WebApi.Controllers
{

    /// <summary>
    /// Creates the base controler for direct crud operations
    /// <typeparam name="T"></typeparam>
    public abstract class BaseApiControllerCRUD<T> : BaseApiController where T : class
    {
        public readonly T Model;

        public BaseApiControllerCRUD() : base()
        {
            // criamos uma instancia  do objecto para poder consultar as propriedades
            Model = (T)Activator.CreateInstance(typeof(T), new object[] { });
        }


        /// <summary>
        /// Por defeito, todos os campos do modelo da tabela são adicionados como
        /// autorizados.
        /// 
        /// Para limitar os campos basta fazer override deste metodo
        /// No override limpar a lista e adicionar só os que são necessários.
        protected override void DefineFieldsAllowedForQuery()
        {
            if (FieldsAllowedForQuery != null)
            {
                FieldsAllowedForQuery = null;
            }

            FieldsAllowedForQuery = new List<string>();

            //lista os campos do modelo que admitimos utilizar na consulta
            foreach (var prop in Model.GetType().GetProperties())
            {
                string fieldName = prop.Name.ToLowerInvariant();
                FieldsAllowedForQuery.Add(fieldName);
            }
        }

        /// <summary>
        /// Inserts a single record
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual HttpResponseMessage Post([FromBody] T item)
        {
            HttpResponseMessage response = null;
            
            if (!DoPreInsertValidations(ref item))
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
            {
                response = DoPost(item);
            }

            return response;
        }


        /// <summary>
        /// Updates a single record identified by id
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPut]
        public virtual HttpResponseMessage Put([FromUri] int id, [FromBody] T item)
        {
            HttpResponseMessage response = null;

            if (!DoPreUpdateValidations(ref item))
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
            {
                response = DoPut(id, item);
            }
            return response;

        }

        /// <summary>
        /// Cria um novo item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract HttpResponseMessage DoPost(T item);


        /// <summary>
        /// Actualiza um item
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract HttpResponseMessage DoPut(int Id, T item);



        /// <summary>
        /// All validation logic before insert new data
        /// Override as needed
        /// </summary>
        /// <returns></returns>
        protected virtual bool DoPreInsertValidations(ref T item)
        {
            return true;
        }

        /// <summary>
        /// All validation logic before update data
        /// Override as needed
        /// </summary>
        /// <returns></returns>
        protected virtual bool DoPreUpdateValidations(ref T item)
        {
            return true;
        }


        protected int GetKeyValueOf(T item)
        {
            Type t = item.GetType();
            var props = t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(KeyAttribute)));
            var value = props.First<PropertyInfo>().GetValue(item, null);

            return Convert.ToInt32(value);

        }


    }
}
