using CardTransactions.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CardTransactions.Data.Clients;

namespace CardTransactions.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddData(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CardTransactionsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddHttpClient<ITreasuryExchangeRateClient, TreasuryExchangeRateClient>();
        return services;
    }
}