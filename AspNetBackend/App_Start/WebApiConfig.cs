using System.Web.Http;
using System.Web.Http.Cors;
using System.Configuration;
using System;

namespace AspNetBackend.App_Start
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // CORS 설정 추가
            string urls = ConfigurationManager.AppSettings["AllowedUrls"];
            var allowedOrigins = urls.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var cors = new EnableCorsAttribute(string.Join(",", allowedOrigins), "*", "*");

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