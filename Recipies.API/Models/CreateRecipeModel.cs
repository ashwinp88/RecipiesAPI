using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Recipies.Objects;

namespace Recipies.API.Models
{
    public class CreateRecipeModel
    {
        public CreateRecipeModel()
        { }

        [Required]
        public Recipe Recipe_ { get; set; }

        public Image RecipeImage_ { get; set; }

        public List<RecipeIngredient> RecipeIngredients_ { get; set; }

        [Required]
        public List<RecipeDirection> RecipeSteps_ { get; set; }
        

    }
}