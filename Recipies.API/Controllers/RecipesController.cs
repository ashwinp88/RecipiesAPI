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
                await SaveRecipe(recipe);
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
        private async Task<int> SaveRecipe(CreateRecipeModel recipe)
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
                
                foreach (var recipeStep in recipe.RecipeSteps_)
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

        private async Task<List<FetchRecipeModel>> SearchByTitle(string title)
        {
            var ret = new List<FetchRecipeModel>();
            try
            {
                var recipes = await db.Recipes.Where(r => r.Title.Contains(title)).ToListAsync();
                foreach (var recipe in recipes)
                {
                    //Recipe Header
                    var newRecipe = new Models.Recipe
                    {
                        ID = recipe.ID,
                        Title = recipe.Title,
                        Description = recipe.Description,
                        TotalTimeNeededHours = recipe.TotalTimeNeededHours,
                        TotalTimeNeededMinutes = recipe.TotalTimeNeededMinutes,
                        CreatedByUser = GetUserName(recipe.CreatedByUser),
                        AverageRecipieRating = recipe.AverageRecipieRating,
                        Complete = recipe.Complete
                    };
                    //Recipe Image
                    var newRecipeImage = new Models.Image();
                    var img = db.Images.FirstOrDefault(i => i.ImageApplyID == recipe.ID && i.ImageType == 0);
                    if (img != null)
                    {
                        newRecipeImage.ID = img.ID;
                        newRecipeImage.ImageApplyID = img.ImageApplyID;
                        newRecipeImage.ImageLocation = img.ImageLocation;
                        newRecipeImage.ImageType = img.ImageType;
                        newRecipeImage.seq = img.seq;
                    }
                    //Recipe Ingredients
                    var newRecipeIngredients = recipe.RecipeIngredients.Select(ing => new Models.RecipeIngredient
                    {
                        ID = ing.ID,
                        Ingredient = new Models.Ingredient
                            {ID = ing.Ingredient.ID, Description = ing.Ingredient.Description},
                        UnitOfMeasurement = new Models.UnitsOfMeasurement
                        {
                            ID = ing.UnitsOfMeasurement.ID, Description = ing.UnitsOfMeasurement.Description,
                            Abbreviation = ing.UnitsOfMeasurement.Abbreviation
                        },
                        Quantity = ing.Quantity
                    }).ToList();
                    //Recipe Directions
                    var newRecipeDirections = recipe.RecipeDirections.Select(step => new Models.RecipeDirection
                        {
                            ID = step.ID,
                            Step = step.Step,
                            StepTitle = step.StepTitle,
                            StepInstructions = step.StepInstructions,
                            TimeSpanHours = step.TimeSpanHours,
                            TimeSpanMinutes = step.TimeSpanMinutes
                        }).ToList();
                    //Construct fetch response
                    ret.Add(new FetchRecipeModel
                    {
                        Recipe = newRecipe,
                        RecipeImage = newRecipeImage,
                        RecipeIngredients = newRecipeIngredients,
                        RecipeDirections = newRecipeDirections
                    });
                }

                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        //private void RemoveCircularReferences(IEnumerable<Recipe> recipes)
        //{
        //    foreach (var recipe in recipes)
        //    {
        //        recipe.CreatedByUser = GetUserName(recipe.CreatedByUser);
        //        foreach (var recipeIngredient in recipe.RecipeIngredients)
        //        {
        //            recipeIngredient.Recipe = null;
        //            recipeIngredient.Ingredient.RecipeIngredients = null;
        //            recipeIngredient.UnitsOfMeasurement.RecipeIngredients = null;
        //        }

        //        foreach (var recipeDirection in recipe.RecipeDirections)
        //        {
        //            recipeDirection.Recipe = null;
        //        }

                
        //    }
        //}

        private string GetUserName(string uid)
        {
            var user = db.AspNetUsers.Find(uid);
            if (user != null)
            {
                return user.UserName;
            }
            return "anonymous";
        }
#endregion Helpers

    }
}
