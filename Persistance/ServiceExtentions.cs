using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistance.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistance
{
    public static class ServiceExtentions
    {
        public static void AddPersistance(this IServiceCollection services, IConfiguration configuration)
        {
            //register services
            services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")
                ));

            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        }
    }
}
