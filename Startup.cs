using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;

namespace cms_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public IConfiguration Configuration { get; }

        // Add services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    builder.WithOrigins(
                        "http://example.com",
                        "http://www.contoso.com",
                        "http://localhost:4200",
                        "http://www.we-builds.com",
                        "http://vet.we-builds.com")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
                });
            });

            services.AddControllers();

            // ✅ เพิ่ม MemoryCache สำหรับเก็บ request count
            services.AddMemoryCache();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger Movies Demo", Version = "v1" });
            });
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // Configure HTTP pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(MyAllowSpecificOrigins);

            // ✅ Rate Limiter Middleware (วางไว้ก่อน Routing)
            app.UseMiddleware<RateLimitingMiddleware>();

            app.UseRouting();

            app.UseAuthorization();

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Local");
                c.SwaggerEndpoint("/td-ddpm-api/swagger/v1/swagger.json", "DDPM Connect");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    // ✅ Custom Rate Limiting Middleware
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        // ค่า limit
        private static readonly int LIMIT = 10; // อนุญาต 10 requests
        private static readonly TimeSpan WINDOW = TimeSpan.FromSeconds(30); // ต่อ 30 วินาที

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var cacheKey = $"RateLimit_{ipAddress}";

            var entry = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = WINDOW;
                return new RequestCounter
                {
                    Count = 0,
                    ExpireAt = DateTime.UtcNow.Add(WINDOW)
                };
            });

            entry.Count++;

            if (entry.Count > LIMIT)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. กรุณาลองใหม่ภายหลัง");
                return;
            }

            await _next(context);
        }

        private class RequestCounter
        {
            public int Count { get; set; }
            public DateTime ExpireAt { get; set; }
        }
    }
}
