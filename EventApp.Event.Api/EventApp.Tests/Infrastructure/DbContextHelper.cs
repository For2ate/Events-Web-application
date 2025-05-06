using EventApp.Data.DbContexts; 
using Microsoft.EntityFrameworkCore;

namespace EventApp.Tests.Infrastructure {

    public static class DbContextHelper {

       
        public static ApplicationContext CreateInMemoryDbContext() {

            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationContext(options);

            return context;

        }

    }

}