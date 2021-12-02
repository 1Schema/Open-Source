using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Decia.Business.Common
{
    public static class MethodUtils
    {
        public const BindingFlags PrivateMethod_BindingFlags = (BindingFlags.Instance | BindingFlags.NonPublic);

        #region Page-Context Getter Methods

        public static T Controller<T>(this WebViewPage page)
            where T : ControllerBase
        {
            var controller = (page.ViewContext.Controller as T);
            return controller;
        }

        public static T Controller<T>(this ControllerBase controller)
           where T : ControllerBase
        {
            return (controller as T);
        }

        #endregion

        #region GetActionUrl Methods

        public static string GetActionUrl<T>(this WebViewPage page, Expression<Func<T, object>> expression)
            where T : Controller
        {
            return GetActionUrl<T>(page, expression, null);
        }

        public static string GetActionUrl<T>(this WebViewPage page, Expression<Func<T, object>> expression, object routeValues)
            where T : Controller
        {
            var controller = (page.ViewContext.Controller as Controller);
            return GetActionUrl<T>(controller, expression, routeValues);
        }

        public static string GetActionUrl<T>(this Controller controller, Expression<Func<T, object>> expression)
            where T : Controller
        {
            return GetActionUrl<T>(controller, expression, null);
        }

        public static string GetActionUrl<T>(this Controller controller, Expression<Func<T, object>> expression, object routeValues)
            where T : Controller
        {
            var controllerType = typeof(T);
            var actionName = GetMethodName(expression);
            return GetActionUrl(controller, controllerType, actionName, routeValues);
        }

        public static string GetActionUrl(this WebViewPage page, Expression<Func<object>> expression)
        {
            return GetActionUrl(page, expression, null);
        }

        public static string GetActionUrl(this WebViewPage page, Expression<Func<object>> expression, object routeValues)
        {
            var controller = (page.ViewContext.Controller as Controller);
            return GetActionUrl(controller, controller.GetType(), expression, routeValues);
        }

        public static string GetActionUrl(this WebViewPage page, Type controllerType, Expression<Func<object>> expression)
        {
            return GetActionUrl(page, controllerType, expression, null);
        }

        public static string GetActionUrl(this WebViewPage page, Type controllerType, Expression<Func<object>> expression, object routeValues)
        {
            var controller = (page.ViewContext.Controller as Controller);
            return GetActionUrl(controller, controllerType, expression, routeValues);
        }

        public static string GetActionUrl(this Controller controller, Expression<Func<object>> expression)
        {
            return GetActionUrl(controller, expression, null);
        }

        public static string GetActionUrl(this Controller controller, Expression<Func<object>> expression, object routeValues)
        {
            return GetActionUrl(controller, controller.GetType(), expression, routeValues);
        }

        public static string GetActionUrl(this Controller controller, Type controllerType, Expression<Func<object>> expression)
        {
            return GetActionUrl(controller, controllerType, expression, null);
        }

        public static string GetActionUrl(this Controller controller, Type controllerType, Expression<Func<object>> expression, object routeValues)
        {
            var actionName = GetMethodName(expression);
            return GetActionUrl(controller, controllerType, actionName, routeValues);
        }

        public static string GetActionUrl(this Controller controller, Type controllerType, string actionName)
        {
            return GetActionUrl(controller, controllerType, actionName, null);
        }

        public static string GetActionUrl(this Controller controller, Type controllerType, string actionName, object routeValues)
        {
            var urlHelper = controller.Url;
            var controllerName = controllerType.Name.Replace("Controller", string.Empty);
            var routeValues_Typed = (routeValues as RouteValueDictionary);
            var url = string.Empty;

            if (routeValues_Typed != null)
            { url = urlHelper.Action(actionName, controllerName, routeValues_Typed); }
            else if (routeValues != null)
            { url = urlHelper.Action(actionName, controllerName, routeValues); }
            else
            { url = urlHelper.Action(actionName, controllerName); }

            return url;
        }

        #endregion

        #region RedirectToAction Methods

        public static RedirectToRouteResult RedirectToAction<T>(this WebViewPage page, Expression<Func<T, object>> expression)
            where T : Controller
        {
            return RedirectToAction<T>(page, expression, null);
        }

        public static RedirectToRouteResult RedirectToAction<T>(this WebViewPage page, Expression<Func<T, object>> expression, object routeValues)
            where T : Controller
        {
            var controller = (page.ViewContext.Controller as Controller);
            return RedirectToAction<T>(controller, expression, routeValues);
        }

        public static RedirectToRouteResult RedirectToAction<T>(this Controller controller, Expression<Func<T, object>> expression)
            where T : Controller
        {
            return RedirectToAction<T>(controller, expression, null);
        }

        public static RedirectToRouteResult RedirectToAction<T>(this Controller controller, Expression<Func<T, object>> expression, object routeValues)
            where T : Controller
        {
            var controllerType = typeof(T);
            var actionName = GetMethodName(expression);
            return RedirectToAction(controller, controllerType, actionName, routeValues);
        }

        public static RedirectToRouteResult RedirectToAction(this WebViewPage page, Expression<Func<object>> expression)
        {
            return RedirectToAction(page, expression, null);
        }

        public static RedirectToRouteResult RedirectToAction(this WebViewPage page, Expression<Func<object>> expression, object routeValues)
        {
            var controller = (page.ViewContext.Controller as Controller);
            return RedirectToAction(controller, controller.GetType(), expression, routeValues);
        }

        public static RedirectToRouteResult RedirectToAction(this WebViewPage page, Type controllerType, Expression<Func<object>> expression)
        {
            return RedirectToAction(page, controllerType, expression, null);
        }

        public static RedirectToRouteResult RedirectToAction(this WebViewPage page, Type controllerType, Expression<Func<object>> expression, object routeValues)
        {
            var controller = (page.ViewContext.Controller as Controller);
            return RedirectToAction(controller, controllerType, expression, routeValues);
        }

        public static RedirectToRouteResult RedirectToAction(this Controller controller, Expression<Func<object>> expression)
        {
            return RedirectToAction(controller, expression, null);
        }

        public static RedirectToRouteResult RedirectToAction(this Controller controller, Expression<Func<object>> expression, object routeValues)
        {
            return RedirectToAction(controller, controller.GetType(), expression, routeValues);
        }

        public static RedirectToRouteResult RedirectToAction(this Controller controller, Type controllerType, Expression<Func<object>> expression)
        {
            return RedirectToAction(controller, controllerType, expression, null);
        }

        public static RedirectToRouteResult RedirectToAction(this Controller controller, Type controllerType, Expression<Func<object>> expression, object routeValues)
        {
            var actionName = GetMethodName(expression);
            return RedirectToAction(controller, controllerType, actionName, routeValues);
        }

        public static RedirectToRouteResult RedirectToAction(this Controller controller, Type controllerType, string actionName)
        {
            return RedirectToAction(controller, controllerType, actionName, null);
        }

        public static RedirectToRouteResult RedirectToAction(this Controller controller, Type controllerType, string actionName, object routeValues)
        {
            var urlHelper = controller.Url;
            var controllerName = controllerType.Name.Replace("Controller", string.Empty);
            var routeValues_Typed = (routeValues as RouteValueDictionary);
            var route = (RedirectToRouteResult)null;

            if (routeValues_Typed != null)
            {
                var redirectMethod = controllerType.GetMethod("RedirectToAction", PrivateMethod_BindingFlags, Type.DefaultBinder, new Type[] { typeof(string), typeof(string), typeof(RouteValueDictionary) }, null);
                route = (RedirectToRouteResult)redirectMethod.Invoke(controller, new object[] { actionName, controllerName, routeValues_Typed });
            }
            else if (routeValues != null)
            {
                var redirectMethod = controllerType.GetMethod("RedirectToAction", PrivateMethod_BindingFlags, Type.DefaultBinder, new Type[] { typeof(string), typeof(string), typeof(object) }, null);
                route = (RedirectToRouteResult)redirectMethod.Invoke(controller, new object[] { actionName, controllerName, routeValues });
            }
            else
            {
                var redirectMethod = controllerType.GetMethod("RedirectToAction", PrivateMethod_BindingFlags, Type.DefaultBinder, new Type[] { typeof(string), typeof(string) }, null);
                route = (RedirectToRouteResult)redirectMethod.Invoke(controller, new object[] { actionName, controllerName });
            }

            return route;
        }

        #endregion

        #region GetMethodName Methods

        public static string GetMethodName<T>(this Expression<Func<T, object>> expression)
        {
            var bodyAsString = expression.Body.ToString();
            return GetMethodName(bodyAsString);
        }

        public static string GetMethodName(this Expression<Func<object>> expression)
        {
            var bodyAsString = expression.Body.ToString();
            return GetMethodName(bodyAsString);
        }

        public static string GetMethodName(string expressionText)
        {
            int nestCount = 0;
            int? lastRootOpenParenIndex = null;

            for (int i = 0; i < expressionText.Length; i++)
            {
                var currChar = expressionText[i];

                if (currChar == '(')
                {
                    if (nestCount == 0)
                    { lastRootOpenParenIndex = i; }
                    nestCount++;
                }
                else if (currChar == ')')
                {
                    nestCount--;
                }
            }

            if (!lastRootOpenParenIndex.HasValue)
            { return string.Empty; }

            expressionText = expressionText.Remove(lastRootOpenParenIndex.Value);

            var dotParts = expressionText.Split(new char[] { '.' });
            var methodNamePart = dotParts.LastOrDefault();

            return methodNamePart;
        }

        #endregion
    }
}