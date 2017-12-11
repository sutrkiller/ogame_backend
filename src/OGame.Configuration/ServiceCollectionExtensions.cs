using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OGame.Configuration.Settings;

namespace OGame.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<ClientSettings>(configuration.GetSection("Client"));
            services.Configure<TokenSettings>(configuration.GetSection("Tokens"));
            services.Configure<AccountSettings>(configuration.GetSection("Account"));
            return services;
        }
    }
}
