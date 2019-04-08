using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalLib.WebApi.Auth
{
    public static class ConfigOAuthHelper
    {
        public static void AccesTokenConfig(IAppBuilder app, IUserApp loginManager, bool createRefreshToken = false)
        {
            /*
             * Permite acesso ao endereço de fornecimento do token de acesso sem precisar de HTTPS (AllowInsecureHttp = tue). 
             * Obviamente, num ambiente de produção, o valor deve ser false.
             *
             * Configurar o endereço do fornecimento do token de acesso (TokenEndpointPath).
             * Configurar por quanto tempo um token de acesso será válido (AccessTokenExpireTimeSpan).
             */

            // configurar criação de tokens
            var opcoesConfiguracaoToken = new OAuthAuthorizationServerOptions()
            {
                // AccessTokenFormat = new JwtFormat(jwtOptions.Audience, symmetricKeyIssuerSecurityTokenProvider),
#if DEBUG
                AllowInsecureHttp = true,
#endif
#if !DEBUG
                AllowInsecureHttp = false,
#endif
                TokenEndpointPath = new PathString("/api/token"),

#if DEBUG
                //AccessTokenExpireTimeSpan = TimeSpan.FromHours(4),
                AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(15),
#endif
#if !DEBUG
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1),
#endif
                // injetar provider com gestor de logins
                Provider = new AccessTokenProvider(loginManager),
            };

            if (createRefreshToken)
            {
                opcoesConfiguracaoToken.RefreshTokenProvider = new RefreshTokenProvider();
            }

            // activar o uso de access tokens     
            app.UseOAuthAuthorizationServer(opcoesConfiguracaoToken);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }
}

