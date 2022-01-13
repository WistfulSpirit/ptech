using EasyNetQ;
using EasyNetQ.AutoSubscribe;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleFibonacciAPI
{
    public static class AppBuilderExtension
    {
        public static IApplicationBuilder UsePublish(this IApplicationBuilder appBuilder)
        {
            var services = appBuilder.ApplicationServices.CreateScope().ServiceProvider;

            var lifeTime = services.GetService<IHostApplicationLifetime>();
            var bus = services.GetService<IBus>();
            lifeTime.ApplicationStopped.Register(() => bus.Dispose());
            return appBuilder;
        }
    }
}
