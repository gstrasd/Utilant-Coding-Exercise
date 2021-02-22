using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Web.Modules;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Start();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder =
                Host.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureContainer((HostBuilderContext context, ContainerBuilder container) => container.RegisterModule(new WebModule(context.Configuration)))
                    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());

            return builder;
        }
    }
}
