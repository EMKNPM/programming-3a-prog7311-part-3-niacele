using GLMS.API.Data;
using GLMS.API.Services.Behavioural;
using GLMS.API.Services.Creational;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GLMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container - process controllers returning JSOn data, not HTML views
            builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
            // Swagger Config for self documenting endpoints - Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                //status enum make api crash
                options.CustomSchemaIds(type => type.FullName?.Replace("+", "_"));
            });

            //ADDED DATABASE CONTEXT
            builder.Services.AddDbContext<GLMSContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));


            //REGISTERING THE DESIGN PATTERNS INTO THE BACKEND
            builder.Services.AddScoped<IContractFactory, FreightContractFactory>();
            builder.Services.AddScoped<ServiceRequestManager>();

            //HttpClient for Currency Converter
            builder.Services.AddHttpClient();


            //AUTHENTICATION!!!
            // cryptokey 
            var jwtKey = "GLMS_Super_Secret_Security_Key_2026_Do_Not_Share";
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            // 2. Register Authentication and JWT Bearer Services
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // for when theres an unauthorized user
            })
                // token validation rules criteria
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,  
                    ValidateAudience = false,
                    ValidateLifetime = true, // token will expire after a certain amount of time
                    ValidateIssuerSigningKey = true, // verify incoming token
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };
            });



            var app = builder.Build();

            //DEBUGGING PURPOSES
            app.UseDeveloperExceptionPage();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // ensures that the tokens get used
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
