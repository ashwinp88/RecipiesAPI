using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Recipies.API.Filters;
using Recipies.Objects;
using Ingredient = Recipies.API.Models.Ingredient;


namespace Recipies.API.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/Ingredients")]
    [ClientCacheControlFilter]
    public class IngredientsController : ApiController
    {
        private sealed class Response
        {
            public IQueryable<Ingredient> Ingredients { get; set; }
            public int Length { get; set; }
        }

       

        private RecipiesDbEntities db = new RecipiesDbEntities();

        #region HTTP Actions
        //GET
        public IHttpActionResult Get(int pageSize = 0, int pageNumber = 0)
        {
            IQueryable<Ingredient> pagedIngredients;
            if (pageSize == 0)
            {
                pagedIngredients = db.Ingredients.Select(r => new Ingredient {ID = r.ID, Description = r.Description}).
                                    OrderBy(r => r.Description);
            }
            else
            {
                pagedIngredients = db.Ingredients.Select(r => new Ingredient { ID = r.ID, Description = r.Description }).
                                   OrderBy(r => r.Description).Skip((pageNumber) * pageSize).Take(pageSize);
            }            
            var res = new Response { Ingredients = pagedIngredients, Length = db.Ingredients.Count() };
            //var msg = Request.CreateResponse(HttpStatusCode.OK, res);
            //return msg;
            return Ok(res);
        }

        //GET (by name)
        [Route("{description}")]
        public IHttpActionResult GetByName(string description, int pageSize = 10, int pageNumber = 0)
        {
            var ingredients = db.Ingredients.AsQueryable().
                              Where(r => r.Description.Contains(description)).Select(r => new Ingredient { ID = r.ID, Description = r.Description });
            var pagedIngredients = ingredients.
                                   OrderBy(r => r.Description).
                                   Skip((pageNumber) * pageSize).
                                   Take(pageSize);
            var res = new Response { Ingredients = pagedIngredients, Length = ingredients.Count() };
            //var msg = Request.CreateResponse(HttpStatusCode.OK, res);
            //return msg;
            return Ok(res);
        }

        // POST
        [Authorize]
        [ValidateModelStateFilter]
        public async Task<HttpResponseMessage> Post(Recipies.Objects.Ingredient ingredient)
        {
            if (!IngredientExists(ingredient.Description))
            {
                db.Ingredients.Add(ingredient);
                await db.SaveChangesAsync();
                var message = Request.CreateResponse(HttpStatusCode.OK, ingredient);
                message.Headers.Location = new Uri(Request.RequestUri + ingredient.ID.ToString());
                return message;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Ingredient already exists");
            }
        }

        // DELETE
        [Authorize]
        public async Task<IHttpActionResult> Delete(long id)
        {
            Recipies.Objects.Ingredient ingredient = await db.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }

            db.Ingredients.Remove(ingredient);
            await db.SaveChangesAsync();
            return Ok(ingredient);
        }

        //PUT
        [Authorize]
        [HttpPut, HttpPatch]
        public async Task<IHttpActionResult> Put(long id, Recipies.Objects.Ingredient ingredient)
        {
            db.Entry(ingredient).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IngredientExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(ingredient);
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
        private bool IngredientExists(long id)
        {
            return db.Ingredients.Count(e => e.ID == id) > 0;
        }

        private bool IngredientExists(string description)
        {
            return db.Ingredients.Count(e => e.Description == description) > 0;
        }
        #endregion

    }
}
