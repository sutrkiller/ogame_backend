using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using OGame.Configuration;

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
                    config.AddSharedConfig();
                })
                .UseStartup<Startup>()
                .Build();
    }
}
