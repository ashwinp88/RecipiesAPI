using Recipies.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
           var db = new RecipiesDbEntities();
            //var ingredients = (IQueryable<Ingredient>)db.Ingredients;
            var ingredients = db.Ingredients;
            var items = ingredients.OrderBy(r => r.Description).Take(10);
            foreach (var x in items)
            {
                Console.WriteLine($"{x.ID}\t{x.Description}");
            }
            Console.ReadLine();
        }
    }
}
