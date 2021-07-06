using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace WebApplication1
{
    public static class ServicesExtensions
    {
        public static void AddApiVersioning(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiVersioning(options =>
            {
                options.ApiVersionReader = new QueryStringApiVersionReader("version");
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.UseApiBehavior = true; // It means include only api controller not mvc controller.
                //options.Conventions.Controller<AppController>().HasApiVersion(options.DefaultApiVersion);
                //options.Conventions.Controller<UserController>().HasApiVersion(options.DefaultApiVersion);
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
            });

            services.AddVersionedApiExplorer(); // It will be used to explorer api versioning and add custom text box in swagger to take version number.
        }

        public static void AddSwaggerGenerationUI(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
            
            services.AddSwaggerGen(options =>
            {
                options.OrderActionsBy(orderBy => orderBy.HttpMethod);
                //options.UseReferencedDefinitionsForEnums();
                foreach (var item in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(item.GroupName, new OpenApiInfo
                    {
                        Title = "Version-" + item.GroupName,
                        Version = item.ApiVersion.MajorVersion.ToString() + "." + item.ApiVersion.MinorVersion
                    });
                }
            });
        }

        public static void UseSwaggerGenerationUI(this IApplicationBuilder applicationBuilder)
        {
            var provider = applicationBuilder.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

            applicationBuilder.UseSwagger();
            applicationBuilder.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "Api Help";
                foreach (var item in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{item.GroupName}/swagger.json", item.GroupName);
                }
            });
        }
    }
}
