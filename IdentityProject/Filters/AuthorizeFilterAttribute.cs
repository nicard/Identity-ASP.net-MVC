using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace IdentityProject.Filters
{
    public class AuthorizeFilterAttribute : ActionFilterAttribute
    {
        override public void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var request = filterContext.HttpContext.Request;
            var identity = request.LogonUserIdentity;
            var nome = identity.Name;
            

            //if (request.IsAjaxRequest())
            //{
            //    var respose = filterContext.HttpContext.Response;
            //}
            //else
            //{
            //    filterContext.Result = new RedirectToRouteResult(
            //        new RouteValueDictionary(
            //            new { controller = "Home", action = "index" }
            //        ));
            //}


            base.OnActionExecuting(filterContext);
        }


        override public void OnResultExecuted(ResultExecutedContext filterContext)
        {



            base.OnResultExecuted(filterContext);
        }
    }
}