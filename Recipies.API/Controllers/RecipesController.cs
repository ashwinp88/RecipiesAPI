using Recipies.API.Filters;
using Recipies.Objects;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Routing;
using Recipies.API.Models;

namespace Recipies.API.Controllers
{
    [RoutePrefix("api/Recipes")]
    public class RecipesController : ApiController
    {
        private RecipiesDbEntities db = new RecipiesDbEntities();

        [HttpGet]
        [Route("Search")]
        public async Task<IHttpActionResult> Get(string title)
        {
            if (title.Length < 3)
                return StatusCode(HttpStatusCode.NotAcceptable);
            try
            {
                var ret = await SearchByTitle(title);
                removeCircularReferences(ret);
                return Ok(ret);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }


        [Authorize]
        [ValidateModelStateFilter]
        public async Task<IHttpActionResult> Post(CreateRecipeModel recipe)
        {
            try
            {
                await saveRecipe(recipe);
                return Ok(recipe.Recipe_);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return InternalServerError(e);
            }
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

#region Helpers
        private async Task<int> saveRecipe(CreateRecipeModel recipe)
        {
            try
            {
                db.Recipes.Add(recipe.Recipe_);
                await db.SaveChangesAsync();
                foreach (var recipeIngredient in recipe.RecipeIngredients_)
                {
                    recipeIngredient.Recipe = recipe.Recipe_;
                    if (recipeIngredient.Ingredient.ID == 0)
                    {
                        //Ingredient might be saved but front end did not get it for some reason
                        var ingredient = db.Ingredients.FirstOrDefault(i =>
                            i.Description == recipeIngredient.Ingredient.Description);
                        if (ingredient != null)
                        {
                            //found ingredient in db
                            recipeIngredient.Ingredient = ingredient;
                        }
                        //did not find it. Have to save it.
                        else
                        {
                            db.Ingredients.Add(recipeIngredient.Ingredient);
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        recipeIngredient.Ingredient = db.Ingredients.Find(recipeIngredient.Ingredient.ID);
                    }
                    if (recipeIngredient.UnitsOfMeasurement.ID == 0)
                    {
                        //Unit of measurement might be saved before but front end did not get it for some reason
                        var unitOfMeasure = db.UnitsOfMeasurements.FirstOrDefault(m =>
                            m.Description == recipeIngredient.UnitsOfMeasurement.Description);
                        if (unitOfMeasure != null)
                        {
                            recipeIngredient.UnitsOfMeasurement = unitOfMeasure;
                        }
                        else
                        {
                            //did not find it. Have to save it.
                            db.UnitsOfMeasurements.Add(recipeIngredient.UnitsOfMeasurement);
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        recipeIngredient.UnitsOfMeasurement =
                            db.UnitsOfMeasurements.Find(recipeIngredient.UnitsOfMeasurement.ID);
                    }
                }
               
                db.RecipeIngredients.AddRange(recipe.RecipeIngredients_);
                
                foreach (RecipeDirection recipeStep in recipe.RecipeSteps_)
                {
                    recipeStep.Recipe = recipe.Recipe_;
                    db.RecipeDirections.Add(recipeStep);
                }

                if (recipe.RecipeImage_ != null)
                {
                    recipe.RecipeImage_.ImageApplyID = recipe.Recipe_.ID;
                    db.Images.Add(recipe.RecipeImage_);
                }

                return await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<List<Recipe>> SearchByTitle(string title)
        {
            try
            {
                return await db.Recipes.Where(r => r.Title.Contains(title)).ToListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void removeCircularReferences(List<Recipe> recipes)
        {
            foreach (var recipe in recipes)
            {
                foreach (var recipeIngredient in recipe.RecipeIngredients)
                {
                    recipeIngredient.Recipe = null;
                    recipeIngredient.Ingredient.RecipeIngredients = null;
                    recipeIngredient.UnitsOfMeasurement.RecipeIngredients = null;
                }

                foreach (var recipeDirection in recipe.RecipeDirections)
                {
                    recipeDirection.Recipe = null;
                }

                
            }
        }
#endregion Helpers

    }
}
