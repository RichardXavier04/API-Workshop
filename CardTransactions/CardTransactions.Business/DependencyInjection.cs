using CardTransactions.Business.Interfaces;
using CardTransactions.Business.Services;
using CardTransactions.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CardTransactions.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddData(configuration);
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();

        return services;
    }
}