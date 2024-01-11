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

        // Add services
        // builder.Services.AddScoped<IPollService, PollService>();
        builder.Services.AddSingleton<IPollService, PollService>();
        builder.Services.AddSingleton<IUserService, UserService>();

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