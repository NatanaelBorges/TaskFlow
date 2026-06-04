using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Controllers.v1;
using TaskFlow.Application.SharedContext.Interfaces;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Infrastructure.SharedContext.Data;

namespace TaskFlow.Api.Extensions;

public static class BuilderExtension
{
    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TaskDb"));
    }
    
    public static void AddCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
            options.AddDefaultPolicy(p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod());
        });
    }
    
    public static void AddControllers(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }
    
    public static void AddHateoas(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IHateoasLinker<TaskResponse>, TaskLinker>();
    }
}