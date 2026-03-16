using Microsoft.EntityFrameworkCore;
using Library.Domain;
using Library.MVC.Data;

namespace Library.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}