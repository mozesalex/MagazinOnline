using MagazinOnline.Data;
using Microsoft.EntityFrameworkCore;

namespace MagazinOnline.Tests.Helpers
{
    public static class DbContextHelper
    {
        public static ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}