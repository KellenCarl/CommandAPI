using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using CommandAPI.Models;
using Npgsql;
using Microsoft.IdentityModel.Logging;

namespace CommandAPI
{
    public class Startup
    {
       
        public IConfiguration Configuration {get;}
        public Startup(IConfiguration configuration) => 
            Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true; //Add this line
            //Assemble Connection String from App settings and User Secrets 
                      
            var builder = new NpgsqlConnectionStringBuilder();
            builder.ConnectionString = Configuration.GetConnectionString("PostgreSqlConnection");
            builder.Username = Configuration["UserID"];
            builder.Password = Configuration["Password"];  

            services.AddDbContext<CommandContext>(opt => opt.UseNpgsql(builder.ConnectionString));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt => {
                opt.Audience = Configuration["ResourceId"];
                opt.Authority = $"{Configuration["Instance"]}{Configuration["TenantId"]}";
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CommandContext context)
        {
            context.Database.Migrate();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //Section 2
                endpoints.MapControllers();
            });
        }
    }
}
