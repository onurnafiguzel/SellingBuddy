using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace OrderService.Application
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationRegistration(this IServiceCollection services, Type startup)
        {
            var assm = Assembly.GetExecutingAssembly();

            services.AddAutoMapper(assm);
            services.AddMediatR(assm);

            return services;
        }
    }
}
