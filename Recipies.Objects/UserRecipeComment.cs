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
    
    public partial class UserRecipeComment
    {
        public long ID { get; set; }
        public Nullable<long> RecipeID { get; set; }
        public string UserID { get; set; }
        public string Comment { get; set; }
        public Nullable<System.DateTime> Timestamp { get; set; }
    }
}