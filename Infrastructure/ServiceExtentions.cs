using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class ServiceExtentions
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            //
            services.AddTransient<IEmailService, EmailService>();
        }
    }
}
