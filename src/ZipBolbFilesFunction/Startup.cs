using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZipBlobFilesFunction;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ZipBlobFilesFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = BuildConfiguration(builder.GetContext().ApplicationRootPath);
            builder.Services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));
            builder.Services.AddScoped<ZipService>();
        }

        private IConfiguration BuildConfiguration(string applicationRootPath)
        {
            var config = new ConfigurationBuilder()
                    .SetBasePath(applicationRootPath)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

            return config;
        }
    }
}
