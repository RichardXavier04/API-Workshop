using CardTransactions.Data;
using CardTransactions.Data.Clients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.TestHost;

namespace CardTransactions.IntegrationTests.Support;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeTreasuryExchangeRateClient FakeTreasury { get; } = new();

    public CustomWebApplicationFactory()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CardTransactionsDbContext>();
        dbContext.Database.Migrate();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json"),
                optional: false,
                reloadOnChange: false);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITreasuryExchangeRateClient>();
            services.AddSingleton(FakeTreasury);
            services.AddSingleton<ITreasuryExchangeRateClient>(FakeTreasury);
        });
    }
}