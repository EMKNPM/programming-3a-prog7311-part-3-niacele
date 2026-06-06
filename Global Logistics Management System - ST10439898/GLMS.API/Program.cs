using GLMS.API.Data;
using GLMS.API.Services.Behavioural;
using GLMS.API.Services.Creational;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container - process controllers returning JSOn data, not HTML views
            builder.Services.AddControllers();
            // Swagger Config for self documenting endpoints - Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //ADDED DATABASE CONTEXT
            builder.Services.AddDbContext<GLMSContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));


            //REGISTERING THE DESIGN PATTERNS INTO THE BACKEND
            builder.Services.AddScoped<IContractFactory, FreightContractFactory>();
            builder.Services.AddScoped<ServiceRequestManager>();

            //HttpClient for Currency Converter
            builder.Services.AddHttpClient();

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
}
