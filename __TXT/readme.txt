Artigos uteis

Transformando uma WebApi numa OWIN Application
http://www.randrade.net/2016/04/18/transformando-uma-webapi-numa-owin-application/

Ativando o CORS numa Web API OWIN Application
http://www.randrade.net/2016/04/22/ativando-o-cors-numa-web-api-owin-application/	

Tokens de acesso: Primeiro passo para proteger WebApis
http://www.randrade.net/2016/06/24/tokens-de-acesso-primeiro-passo-para-proteger-webapis/
 
Creating OWIN Middleware using Microsoft Katana
https://www.scottbrady91.com/Katana/Creating-OWIN-Middleware-using-Microsoft-Katana

Supporting only JSON in ASP.NET Web API – the right way
http://www.strathweb.com/2013/06/supporting-only-json-in-asp-net-web-api-the-right-way/



PEDIDO DE AUTENTICAÇÃO

// post apiurl/token
x-www-form-urlencoded
username:blablabla@hotmail.com
password:123
grant_type:password

PEDIDOS DEPOIS DE AUTENTICAÇÃO
header
Authorization	Bearer  TOKEN


http://bitoftech.net/2014/10/27/json-web-token-asp-net-web-api-2-jwt-owin-authorization-server/

http://bitoftech.net/2015/03/31/asp-net-web-api-claims-authorization-with-asp-net-identity-2-1/


Represents an HTTP DELETE protocol method.

Represents an HTTP GET protocol method.

Represents an HTTP HEAD protocol method. The HEAD method is identical to GET
except that the server only returns message-headers in the response, without
a message-body.

Represents an HTTP OPTIONS protocol method.

Represents an HTTP POST protocol method that is used to post a new entity as
an addition to a URI.

Represents an HTTP PUT protocol method that is used to replace an entity identified
by a URI.

Represents an HTTP TRACE protocol method.
