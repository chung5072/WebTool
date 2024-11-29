using System.Web.Http;
using System.Web.Http.Cors;

namespace AspNetBackend.App_Start
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //// CORS 설정 추가
            var cors = new EnableCorsAttribute("http://localhost:50142", "*", "*");

            config.EnableCors(cors);

            // Web API 라우팅
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}