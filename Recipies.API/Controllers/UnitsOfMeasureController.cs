using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using Recipies.API.Filters;
using Recipies.Objects;

namespace Recipies.API.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("UnitsOfMeasure")]
    [ClientCacheControlFilter]
    public class UnitsOfMeasureController : ApiController
    {
        private sealed class Response
        {
            public IQueryable<UnitsOfMeasurement> UnitsOfMeasurements { get; set; }
            public int Length { get; set; }
        }

        private RecipiesDbEntities db = new RecipiesDbEntities();

        #region HTTP Actions
        // GET
        public IHttpActionResult Get(int pageSize = 0, int pageNumber = 0)
        {
            IQueryable<UnitsOfMeasurement> pagedResults;
            if (pageSize == 0)
            {
                pagedResults = db.UnitsOfMeasurements.OrderBy(r => r.Abbreviation);
            }
            else
            {
                pagedResults = db.UnitsOfMeasurements.OrderBy(r => r.Abbreviation).Skip((pageNumber) * pageSize).Take(pageSize);
            }            
            var res = new Response { UnitsOfMeasurements = pagedResults, Length = db.UnitsOfMeasurements.Count() };
            return Ok(res);
        }

        // GET (by name)
        [Route("{description}")]
        public IHttpActionResult GetByName(string description, int pageSize = 10, int pageNumber = 0)
        {
            var results = db.UnitsOfMeasurements.AsQueryable().
                          Where(r => r.Description.Contains(description));
            var pagedResults = results.
                               OrderBy(r => r.Description).
                               Skip((pageNumber) * pageSize).
                               Take(pageSize);
            var res = new Response { UnitsOfMeasurements = pagedResults, Length = results.Count() };
            return Ok(res);
        }

        // POST
        [Authorize]
        [ValidateModelStateFilter]
        public async Task<HttpResponseMessage> Post(UnitsOfMeasurement unitsOfMeasurement)
        {
            if (!UnitsOfMeasurementExists(unitsOfMeasurement.Description))
            {
                db.UnitsOfMeasurements.Add(unitsOfMeasurement);
                await db.SaveChangesAsync();
                var message = Request.CreateResponse(HttpStatusCode.OK, unitsOfMeasurement);
                message.Headers.Location = new Uri(Request.RequestUri + unitsOfMeasurement.ID.ToString());
                return message;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Unit of measurement already exists");
            }
        }

        // DELETE
        [Authorize]
        public async Task<IHttpActionResult> Delete(long id)
        {
            UnitsOfMeasurement unitsOfMeasurement = await db.UnitsOfMeasurements.FindAsync(id);
            if (unitsOfMeasurement == null)
            {
                return NotFound();
            }

            db.UnitsOfMeasurements.Remove(unitsOfMeasurement);
            await db.SaveChangesAsync();
            return Ok(unitsOfMeasurement);
        }

        //PUT
        [Authorize]
        [HttpPut, HttpPatch]
        public async Task<IHttpActionResult> Put(long id, UnitsOfMeasurement unitOfMeasurement)
        {
            db.Entry(unitOfMeasurement).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UnitsOfMeasurementExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(unitOfMeasurement);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Helpers
        private bool UnitsOfMeasurementExists(long id)
        {
            return db.UnitsOfMeasurements.Count(e => e.ID == id) > 0;
        }

        private bool UnitsOfMeasurementExists(string description)
        {
            return db.UnitsOfMeasurements.Count(e => e.Description == description) > 0;
        }
        #endregion

    }
}