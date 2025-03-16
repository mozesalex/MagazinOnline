using System.ComponentModel.DataAnnotations.Schema;

namespace MagazinOnline.ViewModel
{
    public class ProductViewModel
    {
        public int Id { get; set; }  // Adaugă ID-ul aici
        public string Name { get; set; }

        public string Description { get; set; }
 
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public IFormFile Image { get; set; }  // Imagine încărcată

        public string? ExistingImage { get; set; } // Imaginea existentă (pentru a nu o șterge)
    }

}
