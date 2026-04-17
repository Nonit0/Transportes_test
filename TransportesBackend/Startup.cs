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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            services.AddScoped<ICargaService, CargaService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IConductorService, ConductorService>();
            services.AddScoped<IDireccionService, DireccionService>();
            services.AddScoped<IFabricaService, FabricaService>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<IPedidoService, PedidoService>();
            services.AddScoped<IAuthService, AuthService>();

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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Transportes API", Version = "v1" });

                /* Con esta configuracion podemos bloquear swagger para evitar peticiones
                // 1. Definir el esquema de seguridad (JWT)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Escribe: 'Bearer [tu_token]'"
                });

                // 2. Hacer que Swagger use ese esquema de seguridad
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] {}
                    }
                });
                */
            });

            services.AddHttpContextAccessor();


            // ==========================================
            // 6. JWT Authentication
            // ==========================================
            var key = Encoding.ASCII.GetBytes(Configuration["Jwt:Key"]);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = Configuration["Jwt:Audience"],
                    ValidateLifetime = true
                };
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
            app.UseAuthentication();
            app.UseAuthorization();

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
