using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace GlobalLib.WebApi.Auth
{
    /// <summary>
    /// Gerador de token OAuth 
    /// </summary>
    public class AccessTokenProvider : OAuthAuthorizationServerProvider
    {
        // Dados do utilizador
        private IUserApp _userManager;

        // recebe a injecção da class de gestão de acessos
        public AccessTokenProvider(IUserApp UserManager)
        {
            _userManager = UserManager;
        }

        /// <summary>
        /// Valida o token no pedido 
        /// </summary>
        /// <param name="context">Contexto de execução</param>
        /// <returns></returns>
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // add basic auth por client id e client secret
            //string id, secret;

            //if (context.TryGetBasicCredentials(out id, out secret))

            //{
            //    context.OwinContext.Set<string>("as:client_id", id);
            //    context.Validated();

            //}

            context.Validated();
            return Task.FromResult<object>(null);
        }


        /// <summary>
        /// Valida as credenciais e se forem validas gera o token de autenticação
        /// </summary>
        /// <param name="context">Contexto de execução</param>
        /// <returns></returns>
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var form = await context.Request.ReadFormAsync();

            var auth_type = form["AuthType"];
            if (auth_type == null)
            {
                auth_type = "";
            }

            bool userCheck = _userManager.CheckUser(context.UserName, context.Password, auth_type);

            if (!userCheck)
            {
                context.SetError("invalid_grant", "Utilizador ou password inválidos.");
                return;
                //return Task.FromResult<object>(null);
            }

            ClaimsIdentity identity = CreateIdentity();

            AuthenticationProperties properties = new AuthenticationProperties();

            System.DateTime currentUtc = DateTime.UtcNow;

            DateTime expireUtc = currentUtc.Add(context.Options.AccessTokenExpireTimeSpan);

            properties.IssuedUtc = currentUtc;
            properties.ExpiresUtc = expireUtc;

            string clientID = Guid.NewGuid().ToString();

            // client id para o refresh token
            properties.Dictionary.Add("as:client_id", clientID);

            AuthenticationTicket ticket = new AuthenticationTicket(identity, properties);

            // passar o client id para o pipeline do owin
            context.OwinContext.Set<string>("as:client_id", clientID);

            context.Validated(ticket);
            return;// Task.FromResult<object>(null);

        }

        private ClaimsIdentity CreateIdentity()
        {
            // emitir o token com informacoes extras
            // se o utilizador existe
            ClaimsIdentity identity = new ClaimsIdentity("JWT");

            // adiciona as claims com os dados simples de id do utilizador (UserID e UserName)
            identity.AddClaim(new Claim(ClaimTypes.Name, _userManager.UserName));
            identity.AddClaim(new Claim(ClaimTypes.PrimarySid, _userManager.UserID.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Role, _userManager.AuthType == null ? "" : _userManager.AuthType));

            identity.AddClaim(new Claim(ClaimTypes.UserData, _userManager.UserData == null ? "" : _userManager.UserData));
            return identity;
        }

#pragma warning disable 1998
        public override async Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            // update das claims
            ClaimsIdentity identity = CreateIdentity();
            var newTicket = new AuthenticationTicket(identity, context.Ticket.Properties);

            context.Validated(newTicket);

        }
    }
#pragma warning restore 1998
}