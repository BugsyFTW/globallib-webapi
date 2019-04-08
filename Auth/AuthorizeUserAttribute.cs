using System.Web.Http;
using System.Web.Http.Controllers;

namespace GlobalLib.WebApi.Auth
{
    /// <summary>
    /// Atributo custom para validação do acesso aos controladores da API
    /// </summary>
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Classe com os dados do utilizador
        /// </summary>
        public IUserApp UserManager { get; set; }

        /// <summary>
        /// Atributo de Autorização
        /// </summary>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {

            var isAuthorized = base.IsAuthorized(actionContext);

            if (!isAuthorized)
            {
                return false;
            }

            /* TODO: SÓ QUANDO FOR DECIDIDO COMO FAZER
            string userName = HttpContext.Current.User.Identity.Name;

            IEnumerable<Claim> claims = ClaimsPrincipal.Current.Claims;

            long userID = Convert.ToInt64(claims.First(x => x.Type.ToLowerInvariant().Contains("primarysid")).Value);

            UserManager = new UserLoginManager();

            bool getUser = UserManager.GetUserByID(userID);

            if (!getUser)
            {
                return false;
            }

            string methodType = actionContext.Request.Method.Method;
            string absolutePath = actionContext.Request.RequestUri.AbsolutePath;

            isAuthorized = UserManager.ValidatePath(absolutePath, methodType);
            */
            return isAuthorized;
        }

    }
}
