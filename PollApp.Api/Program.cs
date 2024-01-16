using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PollApp.Api.Repositories;
using Swashbuckle.AspNetCore.Filters;

namespace PollApp.Api;

public class Program {
    public static void Main(string[] args){
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add database configuration
        builder.Services.Configure<StoreDatabaseSettings>(
            builder.Configuration.GetSection("Database")
        );

        // Add Redis configuration
        builder.Services.AddStackExchangeRedisCache(options => {
            var conn = builder.Configuration.GetConnectionString("Redis");
            options.Configuration = conn;
        });


        // Add services
        builder.Services.AddSingleton<ICachedPollRepository, CachedPollRespository>();
        builder.Services.AddSingleton<IPollService, PollService>();
        builder.Services.AddSingleton<IUserService, UserService>();

        // Add authentication
        // Add auth to app
        builder.Services.AddSwaggerGen(options => {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });
        // Add auth to swagger
        builder.Services.AddAuthentication().AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters{
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!))
            };
        });


        // Add auto mappers
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

}