using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Recipies.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            // STEP 1:  Any logic you want to do BEFORE the rest of the action filter chain is 
            //          called, and BEFORE the action method itself.
            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, actionContext.ModelState);
            }

          
            // STEP 2: Call the rest of the action filter chain
            await base.OnActionExecutingAsync(actionContext, cancellationToken);

            // STEP 3: Any logic you want to do AFTER the other action filters, but BEFORE
            //         the action method itself is called.
        }
    }
}