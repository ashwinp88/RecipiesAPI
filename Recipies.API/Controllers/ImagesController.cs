using Recipies.API.Filters;
using Recipies.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Recipies.API.Controllers
{
    [RoutePrefix("Images")]
    public class ImagesController : ApiController
    {
        private RecipiesDbEntities db = new RecipiesDbEntities();

        [HttpGet]
        public IHttpActionResult GetRecipeImage(long recipeID)
        {
            var img = db.Images.FirstOrDefault(i => i.ImageApplyID == recipeID && 
                                               i.ImageType == (byte)Shared.ImageType.Recipe);
            if (img == null)
            {
                return NotFound();
            }
            return Ok(img);
        }

        [HttpGet]
        public IHttpActionResult GetRecipeStepImage(long stepID)
        {
            var img = db.Images.FirstOrDefault(i => i.ImageApplyID == stepID &&
                                               i.ImageType == (byte)Shared.ImageType.RecipeStep);
            if (img == null)
            {
                return NotFound();
            }
            return Ok(img);
        }

        [ValidateModelStateFilter, Authorize]
        public IHttpActionResult Post(Image image)
        {
            db.Images.Add(image);
            return Ok(image);
        }
    }
}
