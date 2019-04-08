using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLib.WebApi.Auth
{
    /// <summary>
    /// Interface que define um utilizador na Aplicação
    /// </summary>
    public interface IUserApp
    {
        int UserID { get; }
        string UserName { get; }
        string Password { get; }
        string AuthType { get; }
        
        string UserData { get; }

        bool CheckUser(string userName, string userPassword, string AuthType);

        bool GetUserByID(long userID);

        bool ValidatePath(string absolutePath, string httpVerb);
    }
}
