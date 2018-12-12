using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Recipies.API.Models
{
    public class UserRecipeRating
    {
        public long ID { get; set; }
        public string UserID { get; set; }
    }

    public partial class UserRecipeBookMark
    {
        public long ID { get; set; }
        public string UserID { get; set; }
    }

    public class UnitsOfMeasurement
    {
        public long ID { get; set; }
        public string Description { get; set; }
        public string Abbreviation { get; set; }
    }

    public class Ingredient
    {
        public long ID { get; set; }
        public string Description { get; set; }
    }

    public class Image
    {
        public long ID { get; set; }
        public byte? ImageType { get; set; }
        public byte? seq { get; set; }
        public long? ImageApplyID { get; set; }
        public string ImageLocation { get; set; }
        public byte[] ImageBlob { get; set; }
    }

    public class Recipe
    {
        public long ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public byte? TotalTimeNeededHours { get; set; }
        public byte? TotalTimeNeededMinutes { get; set; }
        public string CreatedByUser { get; set; }
        public decimal? AverageRecipeRating { get; set; }
        public bool? Complete { get; set; }

        public DateTime? CreatedOn { get; set; }
    }

    public class RecipeDirection
    {
        public long ID { get; set; }
        public short? Step { get; set; }
        public string StepTitle { get; set; }
        public string StepInstructions { get; set; }
        public byte? TimeSpanHours { get; set; }
        public byte? TimeSpanMinutes { get; set; }
    }

    public class RecipeIngredient
    {
        public long ID { get; set; }
        public Ingredient Ingredient { get; set; }
        public UnitsOfMeasurement UnitOfMeasurement { get; set; }
        public double? Quantity { get; set; }
    }

    public class RecipeComment
    {
        [Required]
        public long? recipeID { get; set; }
        [Required]
        public string userName { get; set; }
        public DateTime? commentedOn { get; set; }
        [Required]
        public string comment { get; set; }
    }

}