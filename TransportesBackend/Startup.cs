using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TransportesBackend.Models;
using TransportesBackend.Services;

namespace TransportesBackend
{

    /*
    el orden:
    configure service:
    service dbcontext
    declarar servicios con inyeccion dependencias

    cors
    controller
    mvc
    addcontroller

    void Configure{
        routing
        usecors
        useauth
        useendpoint
            {
            mapcontroller
            }
        }

    */
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ==========================================
            // 1. Service DbContext
            // ==========================================
            string mySqlConnectionStr = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<TransportesDbContext>(options =>
                options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr)));

            // ==========================================
            // 2. Declarar servicios con inyección de dependencias
            // ==========================================
            // AddScoped = una instancia por petición HTTP (lo más común en web)
            services.AddScoped<IAlmacenService, AlmacenService>();
            services.AddScoped<ICamionService, CamionService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IConductorService, ConductorService>();
            services.AddScoped<IDireccionService, DireccionService>();
            services.AddScoped<IFabricaService, FabricaService>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<IPedidoService, PedidoService>();

            // ==========================================
            // 3. Configuración CORS
            // ==========================================
            services.AddCors(options =>
            {
                options.AddPolicy("PermitirAngular", builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // ==========================================
            // 4. AddController / MVC
            // ==========================================
            services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                // OBLIGATORIO: Para que Angular reciba las propiedades en minúsculas (camelCase)
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                // Opcional pero recomendado: Para que acepte mayúsculas o minúsculas al recibir datos
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            // ==========================================
            // 5. Swagger
            // ==========================================
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TransportesBackend", Version = "v1" });
            });            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TransportesBackend v1"));
            }

            app.UseHttpsRedirection();

            // ==========================================
            // 1. Routing
            // ==========================================
            app.UseRouting();

            // ==========================================
            // 2. UseCors
            // ==========================================
            app.UseCors("PermitirAngular");
    
            // ==========================================
            // 3. UseAuth (Authentication / Authorization)
            // ==========================================
            app.UseAuthorization();
            // app.UseAuthentication(); // Descomentar cuando tengamos un esquema authn

            // ==========================================
            // 4. UseEndpoints -> MapControllers
            // ==========================================
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
