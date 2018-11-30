using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Recipies.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            // GlobalConfiguration.Configure(config => config.MapHttpAttributeRoutes());
        }

        internal protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var res = HttpContext.Current.Response;
            var req = HttpContext.Current.Request;

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.End();
            }
        }
    }
}
