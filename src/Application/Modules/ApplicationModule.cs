using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Domain;
using Autofac;
using Infrastructure.Domain.Typicode;
using Infrastructure.Modules;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Application.Modules
{
    public class ApplicationModule : Module
    {
        private readonly IConfiguration _configuration;

        public ApplicationModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new InfrastructureModule(_configuration));

            builder.Register(c => new UserRepository(c.Resolve<IJsonPlaceholderRepository>(), c.Resolve<ILogger>()))
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.Register(c => new UserRepositoryCacheDecorator(c.Resolve<UserRepository>(), c.Resolve<ILogger>()))
                .As<IUserRepository>()
                .InstancePerLifetimeScope();

            builder.Register(c => new AlbumRepository(c.Resolve<IJsonPlaceholderRepository>(), c.Resolve<IUserRepository>(), c.Resolve<ILogger>()))
                .AsSelf()
                .InstancePerLifetimeScope();

            builder.Register(c => new AlbumRepositoryCacheDecorator(c.Resolve<AlbumRepository>(), c.Resolve<ILogger>()))
                .As<IAlbumRepository>()
                .InstancePerLifetimeScope();

            builder.Register(c => new AlbumSearchService(
                    c.Resolve<IJsonPlaceholderRepository>(),
                    c.Resolve<IAlbumRepository>(), 
                    c.Resolve<ILogger>()))
                .As<IAlbumSearchService>()
                .InstancePerLifetimeScope();
        }
    }
}
