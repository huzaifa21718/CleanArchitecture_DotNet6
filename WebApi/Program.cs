using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistance;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Text;
using WebApi;
using WebApi.Middlewares;
using WebApi.SharedServices;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Information()
          .WriteTo.Console()
            .WriteTo.MSSqlServer(
                connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
                sinkOptions: new MSSqlServerSinkOptions { TableName = "SerilogEvents", AutoCreateSqlTable = true })
          //.WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
          .CreateLogger();

Log.Information("Hello, world!");

try
{
    builder.Host.UseSerilog();
    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerExtention();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure();
    builder.Services.AddPersistance(builder.Configuration);
    builder.Services.AddScoped<IAuthenticatedUser, AuthenticatedUser>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.SaveToken = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["JWTSettings:Issuer"],
            ValidAudience = builder.Configuration["JWTSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSettings:Key"]))
        };
        o.Events = new JwtBearerEvents()
        {
            OnAuthenticationFailed = c =>
            {
                c.NoResult();
                c.Response.StatusCode = 500;
                c.Response.ContentType = "text/plain";
                return c.Response.WriteAsync(c.Exception.ToString());
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("User unauthorized");
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "text/plain";
                return context.Response.WriteAsync("Access is denied due to insufficient permissions. ");
            },
        };
    });



    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<ErrorHandlerMiddleware>();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Error(ex, "Something went wrong");
}
finally
{
    await Log.CloseAndFlushAsync();
}
