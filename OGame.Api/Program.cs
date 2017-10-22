using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace OGame.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings-security.json", false, true);
                    config.AddJsonFile("appsettings-connections.json", false, true);
                    config.AddJsonFile("appsettings-emails.json", false, true);
                })
                .UseStartup<Startup>()
                .Build();
    }
}
