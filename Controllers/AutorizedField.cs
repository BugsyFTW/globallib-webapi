using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLib.WebApi.Controllers
{
    /// <summary>
    /// Representa um campo de parametro de ordenão ou filtragem aceite 
    /// para a query
    /// </summary>
    public class AutorizedField
    {
        /// <summary>
        /// Nome do campo no modelo/Nome do parametro
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Nome do campo na query
        /// </summary>
        public string QueryFieldName { get; set; }

        /// <summary>
        /// Valor em string
        /// </summary>
        public string Value { get; set; }

        public AutorizedField()
        {
            FieldName = null;
            QueryFieldName = null;
            Value = null;
        }
    }
}
