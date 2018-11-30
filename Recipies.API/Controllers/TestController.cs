using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Recipies.API.Controllers
{
    [EnableCors("*","*","*")]
    [RoutePrefix("api/Test")]
    public class TestController : ApiController
    {
        static List<string> values = new List<string> { "1", "2" };

        // GET: api/Test
        public IEnumerable<string> Get()
        {
            return values;
        }

        // GET: api/Test/5
        public IHttpActionResult Get(int id)
        {
            if (values.Count - 1 >= id)
            {
                return Ok(values[id]);
            }
            return BadRequest();

        }

        // POST: api/Test
        [Authorize]
        public IHttpActionResult Post([FromBody]string value)
        {
            values.Add(value);
            return Ok();
        }

        // PUT: api/Test/5
        [Authorize]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Test/5
        [Authorize]
        public void Delete(int id)
        {
        }
    }
}
