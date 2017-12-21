using System.IO;
using Microsoft.AspNetCore.Hosting;
using Npgsql;

namespace CrowdVisionCoreApp
{
    public class Program
    {
		public static NpgsqlConnection sqlQueryConn;
        public static void Main(string[] args)
        {
			CrowdVision.App_Start.PodMonitorConfig.Start();

			sqlQueryConn = new NpgsqlConnection(CrowdVision.Pooler.Pooler.connstring);
			sqlQueryConn.OpenAsync();

			new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build()
                .Run();

			CrowdVision.App_Start.PodMonitorConfig.Shutdown();

		}
	}
}