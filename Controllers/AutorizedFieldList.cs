using GlobalLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlobalLib.WebApi.Controllers
{
    /// <summary>
    /// Lista dos campos autorizados para ordenação/filtragem
    /// </summary>
    public class AutorizedFieldList
    {
        // Lista de campos
        private List<AutorizedField> _Items = new List<AutorizedField>();

        /// <summary>
        /// Retorna a contagem de campos
        /// </summary>
        public int Count
        {
            get
            {
                return _Items.Count();
            }

        }

        /// <summary>
        ///  Retorna a lista de campos
        /// </summary>
        public List<AutorizedField> Items
        {
            get
            {
                return _Items;
            }
        }

        /// <summary>
        /// Adiciona um campo à lista de campos
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="queryFieldName"></param>
        public void Add(string fieldName, string queryFieldName = null)
        {
            var item = _Items.FindAll(x => x.FieldName == fieldName.Trim().ToLowerInvariant());
            if (item.IsNullOrEmpty())
            {
                if (queryFieldName == null)
                {
                    queryFieldName = fieldName;
                }
                _Items.Add(new AutorizedField() { FieldName = fieldName.Trim().ToLowerInvariant(), QueryFieldName = queryFieldName });
            }
            else
            {
                if (queryFieldName != null)
                {
                    item.FirstOrDefault().QueryFieldName = queryFieldName;
                }
            }

        }

        /// <summary>
        /// Limpa a lista
        /// </summary>
        public void Clear()
        {
            _Items.Clear();
        }

        /// <summary>
        /// Remove um campo da lista
        /// </summary>
        /// <param name="fieldName"></param>
        public void Remove(string fieldName)
        {
            _Items.RemoveAll(x => x.FieldName == fieldName);
        }

        /// <summary>
        /// Procura o campo na lista
        /// </summary>
        /// <param name="fieldName">Nome do campo</param>
        /// <returns></returns>
        public AutorizedField Find(string fieldName)
        {
            return _Items.Find(x => x.FieldName == fieldName.Trim().ToLowerInvariant());
        }
    }
}
