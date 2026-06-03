using MagazinOnline.Models;
using MagazinOnline.Tests.Helpers;
using Xunit;

namespace MagazinOnline.Tests.Tests
{
    public class CartTests
    {
        private static Product MakeProduct(int id) => new Product
        {
            Id = id,
            Name = "Produs Test",
            Description = "Descriere test",
            Price = 100,
            Stock = 10,
            ImageUrl = "default.png"
        };

        [Fact]
        public async Task AddCartItem_SavesCorrectly()
        {
            var context = DbContextHelper.GetInMemoryContext();
            context.Products.Add(MakeProduct(1));
            await context.SaveChangesAsync();

            var cartItem = new Cart
            {
                UserId = "user-abc",
                ProductId = 1,
                Quantity = 2,
                Price = 100
            };

            context.Carts.Add(cartItem);
            await context.SaveChangesAsync();

            Assert.Equal(1, context.Carts.Count());
            Assert.Equal(2, context.Carts.First().Quantity);
        }

        [Fact]
        public async Task RemoveCartItem_DeletesCorrectly()
        {
            var context = DbContextHelper.GetInMemoryContext();
            context.Products.Add(MakeProduct(1));
            await context.SaveChangesAsync();

            var cartItem = new Cart
            {
                UserId = "user-abc",
                ProductId = 1,
                Quantity = 1,
                Price = 50
            };
            context.Carts.Add(cartItem);
            await context.SaveChangesAsync();

            context.Carts.Remove(cartItem);
            await context.SaveChangesAsync();

            Assert.Equal(0, context.Carts.Count());
        }
    }
}