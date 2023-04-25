using ClinicService.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net;

namespace ClinicServiceV2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure Kestrel

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Any, 5100, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
                options.Listen(IPAddress.Any, 5101, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1;
                });
            });

            #endregion

            #region Configuration Grpc

            builder.Services
                .AddGrpc()
                .AddJsonTranscoding();



            #endregion

            #region Configure EF DBContext Service (CardStorageService Database)

            builder.Services.AddDbContext<ClinicServiceDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration["Settings:DatabaseOptions:ConnectionString"]);
            });

            #endregion


            #region ConfigureSwagger

            //https://learn.microsoft.com/ru-ru/ASPNET/Core/grpc/json-transcoding-openapi?view=aspnetcore-8.0

            builder.Services.AddGrpcSwagger();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo { Title = "ClinicServiceV2", Version = "v1" });

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "ClinicServiceV2.xml");
                c.IncludeXmlComments(filePath);
                c.IncludeGrpcXmlComments(filePath, includeControllerXmlComments: true);
            });


            #endregion
            // Add services to the container.
            // builder.Services.AddAuthorization();


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });
            }
            app.UseRouting();
            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true } );

            app.MapGrpcService<ClinicServiceV2.Services.ClinicService>().EnableGrpcWeb();
            app.MapGet("/",
                () =>
                "Communication with gRPC endpoint must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


            app.Run();
        }
    }
}