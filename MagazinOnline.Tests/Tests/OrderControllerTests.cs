using MagazinOnline.Controllers;
using MagazinOnline.Models;
using MagazinOnline.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MagazinOnline.Tests.Tests
{
    public class OrdersControllerTests
    {
        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            var context = DbContextHelper.GetInMemoryContext();
            var controller = new OrdersController(context);

            var result = await controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        } //dacă id-ul e null, returnează 404

        [Fact]
        public async Task Details_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            var context = DbContextHelper.GetInMemoryContext();
            var controller = new OrdersController(context);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }//dacă comanda nu există (id=999), returnează 404

        [Fact]
        public async Task Details_ReturnsViewResult_WhenOrderExists()
        {
            var context = DbContextHelper.GetInMemoryContext();
            context.Orders.Add(new Order
            {
                Id = 1,
                UserId = "user-123",
                OrderDate = DateTime.Now,
                TotalAmount = 500,
                Status = "Pending",
                Address = "Str. Exemplu nr. 1"
            });
            await context.SaveChangesAsync();

            var controller = new OrdersController(context);
            var result = await controller.Details(1);

            Assert.IsType<ViewResult>(result);
        }// dacă comanda există, returnează un ViewResult
    }
}