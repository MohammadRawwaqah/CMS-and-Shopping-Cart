using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CmsShoppingCart
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute ----> means that we will route you to one controller and to specific action... and if you dont write it in url, So must be default there.

            // routes.MapRoute("Pages", "{page}", new { controller = "Pages", action = "Index" } -----> This mean that you want go to: localhost:54158/{page} but you dont write what controller and action SO WE WILL TAKE DEFAULT --> Pages/Index

            //we write namespace here because we have two controller called Pages, one inside (Admin/Controllere folder) and second inside (original Controler folder)
            //so to distinguish between them we write namespace to make server know what we mean.
            //FIRST ROUTE:
            //SECOND ROUTE:if you send me localhost:/{page} --- and here {page} is a variable ♥REMEMBER {id}♥ yes everything inside {} is a variable -- e.g: http://localhost:54158/dumb look at dumb ! dumb here is a {page} value.
            //THIRD ROUTE: if you send me an empty url e.g: http://localhost:54158/ So i will take you Pages controller / Index action ----
            //بنطل على الرابط تبعك كيف شكله وعلى اساسه احنا بنطبق
            //>>>>>>>>>>>>>>>>>>>>>> "Shop/{action}/{name}",    This means that if you don't write an action i will take an action that inside default
            routes.MapRoute("Account", "Account/{action}/{id}", new { controller = "Account", action = "Index", id = UrlParameter.Optional }, new[] { "CmsShoppingCart.Controllers" });//or this new string[] instead of just new[]
            routes.MapRoute("Cart", "Cart/{action}/{name}", new { controller = "Cart", action = "Index", name = UrlParameter.Optional }, new[] { "CmsShoppingCart.Controllers" });//or this new string[] instead of just new[]
            routes.MapRoute("Shop", "Shop/{action}/{name}", new { controller = "Shop", action = "Index", name = UrlParameter.Optional }, new[] { "CmsShoppingCart.Controllers" });//or this new string[] instead of just new[]
            routes.MapRoute("SidebarPartial", "Pages/SidebarPartial", new { controller = "Pages", action = "SidebarPartial" }, new[] { "CmsShoppingCart.Controllers" });//or this new string[] instead of just new[]
            routes.MapRoute("PagesMenuPartial", "Pages/PagesMenuPartial", new { controller = "Pages", action = "PagesMenuPartial" }, new[] { "CmsShoppingCart.Controllers" });//or this new string[] instead of just new[]
            routes.MapRoute("Pages", "{page}", new { controller = "Pages", action = "Index" }, new [] { "CmsShoppingCart.Controllers" });//or this new string[] instead of just new[]
            routes.MapRoute("Default", "", new { controller = "Pages", action = "Index" }, new [] { "CmsShoppingCart.Controllers" });//or this new string[] instead of just new[]
            
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //    //this is your default url when you run from any controller . BUT you must have Home controller and
            //    //Index action ... but if you run from page (HTML) so ok the page will show in your browser.
            //);
        }
    }
}
