using Autofac;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Domain.Typicode;
using Serilog;

namespace Infrastructure.Modules
{
    public class InfrastructureModule : Module
    {
        private readonly IConfiguration _configuration;

        public InfrastructureModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new JsonPlaceholderRepository(c.Resolve<HttpClient>(), c.Resolve<ILogger>()))
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.Register(c => new JsonPlaceholderRepositoryCacheDecorator(c.Resolve<JsonPlaceholderRepository>(), c.Resolve<ILogger>()))
                .As<IJsonPlaceholderRepository>()
                .InstancePerLifetimeScope();

            builder.Register(c =>
                {
                    var client = new HttpClient
                    {
                        BaseAddress = new Uri(_configuration["TypicodeBaseUrl"])
                    };
                    return client;
                })
                .AsSelf()
                .SingleInstance();
        }
    }
}
