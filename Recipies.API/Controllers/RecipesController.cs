using Recipies.API.Filters;
using Recipies.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Recipies.API.Controllers
{
    [RoutePrefix("Recipes")]
    public class RecipesController : ApiController
    {
        // public IHttpActionResult GetRe
        private RecipiesDbEntities db = new RecipiesDbEntities();

        [Authorize]
        [ValidateModelStateFilter]
        public IHttpActionResult Post(Recipe recipe)
        {
            db.Recipes.Add(recipe);
            db.SaveChanges();
            return Ok(recipe);
        }

        [Authorize]
        public async Task<IHttpActionResult> MarkRecipeAsComplete(long recipeID)
        {
            var recipe = await db.Recipes.FindAsync(recipeID);
            if (recipe == null)
            {
                return NotFound();
            }
            recipe.Complete = true;
            db.SaveChanges();
            return Ok();
        }
    }
}
