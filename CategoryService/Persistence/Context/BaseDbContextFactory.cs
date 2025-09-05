using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Persistence.Context; // DbContext sınıfınızın namespace'i

public class BaseDbContextFactory : IDesignTimeDbContextFactory<BaseDbContext>
{
    public BaseDbContext CreateDbContext(string[] args)
    {
        // appsettings.json dosyasının yolunu belirtiyoruz
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Bağlantı dizesini okuyarak DbContextOptions'ı oluşturuyoruz
        var connectionString = configuration.GetConnectionString("DatabaseContext");

        var builder = new DbContextOptionsBuilder<BaseDbContext>();
        builder.UseSqlServer(connectionString);

        // Hem options'ı hem de configuration'ı constructor'a iletiyoruz
        return new BaseDbContext(builder.Options, configuration);
    }
}