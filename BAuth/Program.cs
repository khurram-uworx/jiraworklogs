using Formix.Authentication.Basic;
using System.Security.Claims;

namespace BAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.UseBasicAuthentication(creds =>
            {
                bool ok = false;

                ok = (null != creds && "uworx" == creds.Password && "khurram" == creds.UserName);

                if (ok)
                    return new ClaimsPrincipal(new[]
                    {
                        new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, creds.UserName) }, "uworx-auth-type")
                    });
                else
                    return null;
            }, "uworx-realm", 3000);

            app.MapGet("/", () => "Hello World!");
            app.MapGet("/auth", () => "Hello World!");

            app.Run();
        }
    }
}
