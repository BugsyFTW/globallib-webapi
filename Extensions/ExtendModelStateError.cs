using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLib.WebApi.Extensions
{
    public static class ExtendModelStateError
    {
        public static List<string> GetErrorList(this System.Web.Http.ModelBinding.ModelStateDictionary modelState, bool IgnoreExpections = true)
        {
            var query = from state in modelState.Values
                        from error in state.Errors
                        where ((error.Exception == null))
                        select error.ErrorMessage;

            var query2 = from state in modelState.Values
                         from error in state.Errors
                         where ((error.Exception != null))
                         select error.Exception.Message;

            var l1 = query.ToList();
            var l2 = query2.ToList();

            l1.AddRange(l2);

            return l1;
        }
    }
}
