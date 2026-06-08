using MagazinOnline.Controllers;
using MagazinOnline.Models;
using MagazinOnline.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace MagazinOnline.Tests.Tests
{
    public class ProductsControllerTests
    {
        private static Product MakeProduct(int id, string name) => new Product
        {
            Id = id,
            Name = name,
            Description = "Descriere test",
            Price = 100,
            Stock = 5,
            ImageUrl = "default.png"
        };

        private static ProductsController CreateControllerWithUser(
            MagazinOnline.Data.ApplicationDbContext context,
            string role = "Client")
        {
            var controller = new ProductsController(context, null!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user-test-id"),
                new Claim(ClaimTypes.Name, "testuser@test.com"),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithProducts()
        {
            var context = DbContextHelper.GetInMemoryContext();
            context.Products.AddRange(MakeProduct(1, "Laptop"), MakeProduct(2, "Mouse"));
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context, "Client");
            var result = await controller.Index();

            Assert.IsType<ViewResult>(result);
        }//cu produse în bază, Index() returnează un view

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            var context = DbContextHelper.GetInMemoryContext();
            var controller = CreateControllerWithUser(context);

            var result = await controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }//id null retruneaza 404

        [Fact]
        public async Task Details_ReturnsNotFound_WhenProductDoesNotExist()
        {
            var context = DbContextHelper.GetInMemoryContext();
            var controller = CreateControllerWithUser(context);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }//id inexistent returneaza 404

        [Fact]
        public async Task Details_ReturnsViewResult_WhenProductExists()
        {
            var context = DbContextHelper.GetInMemoryContext();
            context.Products.Add(MakeProduct(1, "Tastatura"));
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context);
            var result = await controller.Details(1);

            Assert.IsType<ViewResult>(result);
        }// produs existent returneaza view corect

        [Fact]
        public async Task DeleteConfirmed_RemovesProduct_AndRedirects()
        {
            var context = DbContextHelper.GetInMemoryContext();
            context.Products.Add(MakeProduct(1, "Monitor"));
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context, "Admin");
            var result = await controller.DeleteConfirmed(1);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(0, context.Products.Count());
        }//șterge produsul din bază și face redirect
    }
}