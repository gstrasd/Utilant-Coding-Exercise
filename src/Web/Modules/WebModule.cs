using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Domain;
using Application.Modules;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;
using Web.Controllers;

namespace Web.Modules
{
    public class WebModule : Module
    {
        private readonly IConfiguration _configuration;

        public WebModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new ApplicationModule(_configuration));

            builder.Register(c =>
                {
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(_configuration)
                        .CreateLogger();
                    return logger;
                })
                .As<ILogger>()
                .SingleInstance();

            builder.Register(c => new HomeController(
                    c.Resolve<IAlbumSearchService>(), 
                    c.Resolve<IUserRepository>(), 
                    c.Resolve<ILogger>(), Int32.Parse(_configuration["PageSize"])))
                .AsSelf()
                .InstancePerLifetimeScope();
        }
    }
}
