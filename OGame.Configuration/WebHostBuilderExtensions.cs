using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace OGame.Configuration
{
    public static class WebHostBuilderExtensions
    {
        public static IConfigurationBuilder AddSharedConfig(this IConfigurationBuilder builder)
        {
            IFileProvider fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
            return builder.AddJsonFile(fileProvider, "appsettings-private.json", false, false);
        }
    }
}
