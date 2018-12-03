using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recipies.API.Models
{
    public class FetchRecipeModel
    {
        public Recipe Recipe { get; set; }
        public Image RecipeImage { get; set; }
        public IEnumerable<RecipeIngredient> RecipeIngredients { get; set; }
        public IEnumerable<RecipeDirection> RecipeDirections { get; set; }
    }
}