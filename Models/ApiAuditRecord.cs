using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLib.WebApi.Models
{
    public class ApiAuditRecord
    {
        public int? InsertUserID { get; set; }
        public string InsertUserName { get; set; }
        public DateTime? InsertDate { get; set; }

        public int? UpdateUserID { get; set; }
        public string UpdateUserName { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
