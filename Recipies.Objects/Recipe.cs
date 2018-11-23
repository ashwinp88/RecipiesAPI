//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Recipies.Objects
{
    using System;
    using System.Collections.Generic;
    
    public partial class Recipe
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Recipe()
        {
            this.RecipeDirections = new HashSet<RecipeDirection>();
            this.RecipeIngredients = new HashSet<RecipeIngredient>();
            this.UserRecipeBookMarks = new HashSet<UserRecipeBookMark>();
            this.UserRecipeRatings = new HashSet<UserRecipeRating>();
        }
    
        public long ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<byte> TotalTimeNeededHours { get; set; }
        public Nullable<byte> TotalTimeNeededMinutes { get; set; }
        public string CreatedByUser { get; set; }
        public Nullable<decimal> AverageRecipieRating { get; set; }
        public Nullable<bool> Complete { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecipeDirection> RecipeDirections { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserRecipeBookMark> UserRecipeBookMarks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserRecipeRating> UserRecipeRatings { get; set; }
    }
}