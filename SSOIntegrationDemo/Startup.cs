using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using SSOIntegrationDemo.Models;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;

[assembly: OwinStartupAttribute(typeof(SSOIntegrationDemo.Startup))]
namespace SSOIntegrationDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            ConfigureAuth(app);

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "http://id.edutechsso.test.vggdev.com",
                ClientId = "test-sample",
                AuthenticationType = "edutech.sso",

                AuthenticationMode = AuthenticationMode.Passive,


                Scope = "openid profile roles email",

                RedirectUri = "http://localhost:36737/account/postsso",
                ResponseType = "id_token",

                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,   //"Cookies"


                UseTokenLifetime = false,

                Notifications = new OpenIdConnectAuthenticationNotifications
                {

                    SecurityTokenValidated = n =>
                    {
                        //var id = n.AuthenticationTicket.Identity;

                        //// we want to keep first name, last name, subject and roles
                        //var givenName = id.FindFirst("preferred_username");
                        ////var familyName = id.FindFirst(Constants.ClaimTypes.FamilyName);
                        //var sub = id.FindFirst("sub");

                        //var idToken = n.ProtocolMessage.IdToken;
                        //var accToken = n.ProtocolMessage.AccessToken;

                        //// create new identity and set name and role claim type
                        //var nid = new ClaimsIdentity(
                        //    id.AuthenticationType,
                        //    "preferred_username",
                        //    "role");

                        //nid.AddClaim(givenName);
                        //nid.AddClaim(sub);
                        //nid.AddClaim(new Claim("id_Token", idToken));

                        //// add some other app specific claim
                        //nid.AddClaim(new Claim("app_specific", "some data"));

                        //n.AuthenticationTicket = new AuthenticationTicket(
                        //    nid,
                        //    n.AuthenticationTicket.Properties);
                        var email = n.AuthenticationTicket.Identity.FindFirst("email");
                        if (email == null)
                        {
                            n.AuthenticationTicket = null;
                        }
                        else
                        {
                            n.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                            using (var db = new ApplicationDbContext())
                            {
                                var user = db.Users.SingleOrDefault(d => d.Email.ToUpper() == email.Value.ToUpper());
                                if (user == null)
                                {
                                    n.AuthenticationTicket = null;
                                }
                                else
                                {
                                    n.AuthenticationTicket.Identity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id));
                                }
                            }
                        }

                        //n.AuthenticationTicket.Identity.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));

                        return Task.FromResult(0);
                    }
                }
            });
        }


    }
}
