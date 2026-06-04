using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.SharedContext.Interfaces;
using TaskFlow.Application.TaskContext.Interfaces;
using TaskFlow.Infrastructure.SharedContext.Repositories;
using TaskFlow.Infrastructure.TaskContext.Repositories;

namespace TaskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}