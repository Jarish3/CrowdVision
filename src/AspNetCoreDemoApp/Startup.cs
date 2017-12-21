using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace CrowdVisionCoreApp
{
	public class Utils
	{
		// Sanitizes input
		static public string SanitizeString(string str)
		{
			return Regex.Replace(str, "[^?!(a-zA-Z0-9)]", "");
		}
	}
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore()
                .AddCors()
                .AddJsonFormatters();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseCors(builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            );
            app.UseMvcWithDefaultRoute();
        }
    }
}