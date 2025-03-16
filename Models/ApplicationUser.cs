using Microsoft.AspNetCore.Identity;

namespace MagazinOnline.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Relații
        public ICollection<Order> Orders { get; set; }
        public ICollection<Cart> CartItems { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
