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
        [Route("Comments")]
        public async Task<IHttpActionResult> GetRecipeComments(long recipeID)
        {
            if (recipeID == 0)
                return StatusCode(HttpStatusCode.NotAcceptable);
            try
            {
                var ret = await FetchCommentsByRecipeID(recipeID);
                return Ok(ret);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [Authorize]
        [ValidateModelStateFilter]
        [Route("Comments")]
        [HttpPost]
        public async Task<IHttpActionResult> PostRecipeComment(RecipeComment comment)
        {
            try
            {
                await SaveRecipeComment(comment);
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return InternalServerError(e);
            }

        }

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

        [HttpGet]
        public async Task<IHttpActionResult> Get(long id)
        {
            if (id == 0)
                return StatusCode(HttpStatusCode.NotAcceptable);
            try
            {
                var ret = await SearchByID(id);
                if (ret == null)
                    return NotFound();
                return Ok(ret);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpGet]
        [Route("Recent")]
        public async Task<IHttpActionResult> GetRecentRecipes()
        {
            try
            {
                var recentRecipes = await db.Recipes.OrderByDescending(r => r.CreatedOn).Take(10).ToListAsync();
                var ret = new List<FetchRecipeModel>();
                foreach (var recipe in recentRecipes)
                {
                    ret.Add(constructRecipeFetchResponse(recipe));
                }
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

        [Authorize]
        [Route("Bookmark")]
        [HttpGet]
        public async Task<IHttpActionResult> IsRecipeBookMarked(long recipeID, string uid)
        {
            var record = await db.UserRecipeBookMarks.Where(r => r.Recipe.ID == recipeID && r.UserID == uid).ToListAsync();
            if (record != null && record.Count > 0)
                return Ok(true);
            return Ok(false);
        }

        [Authorize]
        [Route("Bookmark")]
        [HttpPost]
        public async Task<IHttpActionResult> BookmarkRecipe(long recipeID, string uid)
        {
            try
            {
                var record = await db.UserRecipeBookMarks.Where(r => r.Recipe.ID == recipeID && r.UserID == uid).ToListAsync();
                var res = false;
                if (record == null || record.Count == 0)
                {
                    var recipe = await db.Recipes.FindAsync(recipeID);
                    var newBookMark = new Objects.UserRecipeBookMark { UserID = uid, Recipe = recipe };
                    db.UserRecipeBookMarks.Add(newBookMark);                    
                    res = true;
                }
                else
                {
                    db.UserRecipeBookMarks.RemoveRange(record);
                }
                await db.SaveChangesAsync();
                return Ok(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return InternalServerError(e);
            }


        }

#region Helpers
        private async Task<int> SaveRecipe(CreateRecipeModel recipe)
        {
            try
            {
                var newRecipe = new Objects.Recipe
                {
                    Title = recipe.Recipe_.Title,
                    Description = recipe.Recipe_.Description,
                    TotalTimeNeededHours = recipe.Recipe_.TotalTimeNeededHours,
                    TotalTimeNeededMinutes = recipe.Recipe_.TotalTimeNeededMinutes,
                    CreatedByUser = recipe.Recipe_.CreatedByUser,
                    Complete = true,
                    CreatedOn = DateTime.Now
                };

                db.Recipes.Add(newRecipe);

                foreach (var recipeIngredient in recipe.RecipeIngredients_)
                {
                    var ingredient =
                        db.Ingredients.FirstOrDefault(i => i.Description == recipeIngredient.Ingredient.Description);
                    if (ingredient == null)
                    {
                        ingredient = new Objects.Ingredient { Description = recipeIngredient.Ingredient.Description };
                        db.Ingredients.Add(ingredient);
                        db.SaveChanges();
                    }

                    var unitOfMeasure = db.UnitsOfMeasurements.FirstOrDefault(u =>
                        u.Description == recipeIngredient.UnitOfMeasurement.Description);
                    if (unitOfMeasure == null)
                    {
                        unitOfMeasure = new Objects.UnitsOfMeasurement { Description = recipeIngredient.UnitOfMeasurement.Description };
                        db.UnitsOfMeasurements.Add(unitOfMeasure);
                        db.SaveChanges();
                    }
                        
                    var newRecipeIngredient = new Objects.RecipeIngredient
                    {
                        Recipe = newRecipe,
                        Ingredient = ingredient,
                        UnitsOfMeasurement = unitOfMeasure,
                        Quantity = recipeIngredient.Quantity
                    };

                    db.RecipeIngredients.Add(newRecipeIngredient);
                }

                foreach (var step in recipe.RecipeSteps_)
                {
                    var newStep = new Objects.RecipeDirection
                    {
                        Recipe = newRecipe,
                        Step = step.Step,
                        StepTitle = step.StepTitle,
                        StepInstructions = step.StepInstructions,
                        TimeSpanHours = step.TimeSpanHours,
                        TimeSpanMinutes = step.TimeSpanMinutes
                    };

                    db.RecipeDirections.Add(newStep);

                }

                await db.SaveChangesAsync();

                if (recipe.RecipeImage_ != null &&
                    (!string.IsNullOrWhiteSpace(recipe.RecipeImage_.ImageLocation) || recipe.RecipeImage_.ImageBlob != null))
                {
                    var newRecipeImage = new Objects.Image
                    {
                        ImageType = 0,
                        seq = recipe.RecipeImage_.seq,
                        ImageApplyID = newRecipe.ID,
                        ImageLocation = recipe.RecipeImage_.ImageLocation,
                        ImageBlob = recipe.RecipeImage_.ImageBlob
                    };

                    db.Images.Add(newRecipeImage);
                }

                return await db.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<FetchRecipeModel> SearchByID(long ID)
        {
            var ret = new FetchRecipeModel();
            try
            {
                var recipe = await db.Recipes.FindAsync(ID);
                if (recipe == null)
                {
                    return null;
                }
                return constructRecipeFetchResponse(recipe);
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
                    ret.Add(constructRecipeFetchResponse(recipe));
                }

                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<List<RecipeComment>> FetchCommentsByRecipeID(long ID)
        {
            var ret = new List<RecipeComment>();
            var recipeComments = await db.UserRecipeComments.Where(r => r.RecipeID == ID).ToListAsync();
            foreach (var comment in recipeComments)
            {
                var newComment = new RecipeComment
                {
                    userName = GetUserName(comment.UserID),
                    commentedOn = comment.Timestamp,
                    comment = comment.Comment
                };
                ret.Add(newComment);
            }
            return ret;
        }

        private FetchRecipeModel constructRecipeFetchResponse(Objects.Recipe recipe)
        {
            var newRecipe = new Models.Recipe
            {
                ID = recipe.ID,
                Title = recipe.Title,
                Description = recipe.Description,
                TotalTimeNeededHours = recipe.TotalTimeNeededHours,
                TotalTimeNeededMinutes = recipe.TotalTimeNeededMinutes,
                CreatedByUser = GetUserName(recipe.CreatedByUser),
                AverageRecipeRating = recipe.AverageRecipieRating,
                Complete = recipe.Complete,
                CreatedOn = recipe.CreatedOn
            };
            //Recipe Image
            var newRecipeImage = new Models.Image();
            var img = db.Images.FirstOrDefault(i => i.ImageApplyID == recipe.ID && i.ImageType == 0);
            if (img != null)
            {
                newRecipeImage.ID = img.ID;
                newRecipeImage.ImageApplyID = img.ImageApplyID;
                newRecipeImage.ImageLocation = img.ImageLocation;
                newRecipeImage.ImageBlob = img.ImageBlob;
                newRecipeImage.ImageType = img.ImageType;
                newRecipeImage.seq = img.seq;
            }
            //Recipe Ingredients
            var newRecipeIngredients = recipe.RecipeIngredients.Select(ing => new Models.RecipeIngredient
            {
                ID = ing.ID,
                Ingredient = new Models.Ingredient
                { ID = ing.Ingredient.ID, Description = ing.Ingredient.Description },
                UnitOfMeasurement = new Models.UnitsOfMeasurement
                {
                    ID = ing.UnitsOfMeasurement.ID,
                    Description = ing.UnitsOfMeasurement.Description,
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
            var ret = new FetchRecipeModel
            {
                Recipe = newRecipe,
                RecipeImage = newRecipeImage,
                RecipeIngredients = newRecipeIngredients,
                RecipeDirections = newRecipeDirections,
                UserRecipeBookMarks = GetRecipeLikesCount(recipe.ID),
                UserRecipeComments = GetRecipeCommentsCount(recipe.ID)
            };

            return ret;
        }

        private async Task<int> SaveRecipeComment(RecipeComment comment)
        {
            try
            {
                var newComment = new UserRecipeComment
                {
                    RecipeID = comment.recipeID,
                    Comment = comment.comment,
                    UserID = comment.userName,
                    Timestamp = DateTime.Now
                };
                db.UserRecipeComments.Add(newComment);
                return await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        private string GetUserName(string uid)
        {
            var user = db.AspNetUsers.Find(uid);
            if (user != null)
            {
                return user.UserName;
            }
            return "anonymous";
        }

        private int GetRecipeCommentsCount(long recipeID)
        {
            return db.UserRecipeComments.Count(r => r.RecipeID == recipeID);
        }

        private int GetRecipeLikesCount(long recipeID)
        {
            return db.UserRecipeBookMarks.Count(r => r.Recipe.ID == recipeID);
        }
#endregion Helpers

    }
}
