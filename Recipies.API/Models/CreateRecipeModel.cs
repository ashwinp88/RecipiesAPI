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
        public Recipies.Objects.Recipe Recipe_ { get; set; }

        public Recipies.Objects.Image RecipeImage_ { get; set; }

        public List<Recipies.Objects.RecipeIngredient> RecipeIngredients_ { get; set; }

        [Required]
        public List<Recipies.Objects.RecipeDirection> RecipeSteps_ { get; set; }
        

    }
}