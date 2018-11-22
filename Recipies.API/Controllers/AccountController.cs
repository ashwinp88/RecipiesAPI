using Microsoft.AspNet.Identity;
using Recipies.API.Filters;
using Recipies.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Recipies.API.Controllers
{
    [ValidateModelStateFilter]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private AuthRepository _repo = null;
        public AccountController()
        {
            _repo = new AuthRepository();
        }

        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel userModel)
        {
            IdentityResult result = await _repo.RegisterUser(userModel);

            return Ok();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
