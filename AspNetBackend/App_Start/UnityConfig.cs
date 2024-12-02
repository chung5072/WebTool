using AspNetBackend.Models.Services;
using System.Web;
using System.Web.Http;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.WebApi;

namespace AspNetBackend
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // HttpContextBase를 HttpContextWrapper로 등록
            container.RegisterFactory<HttpContextBase>(
                c => new HttpContextWrapper(HttpContext.Current),
                new HierarchicalLifetimeManager()
            );

            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IDocumentService, DocumentService>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}