using Recipies.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Recipies.API.Controllers
{
    [Authorize, RoutePrefix("User")]
    public class UserController : ApiController
    {
        private RecipiesDbEntities db = new RecipiesDbEntities();

        [HttpGet, Authorize]
        public IHttpActionResult Get(string userName)
        {
            var user = db.AspNetUsers.FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user.Id);
        }
    }
}
