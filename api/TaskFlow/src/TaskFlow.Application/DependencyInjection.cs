using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.SharedContext.Abstractions;
using TaskFlow.Application.SharedContext.Interfaces;
using TaskFlow.Application.SharedContext.Responses;
using Delete = TaskFlow.Application.TaskContext.Commands.Delete;
using Patch = TaskFlow.Application.TaskContext.Commands.Patch;
using Update = TaskFlow.Application.TaskContext.Commands.Update;
using Create = TaskFlow.Application.TaskContext.Commands.Create;
using GetById = TaskFlow.Application.TaskContext.Queries.GetById;
using GetAll = TaskFlow.Application.TaskContext.Queries.GetAll;
using TaskFlow.Application.TaskContext.Responses;
using TaskFlow.Domain.SharedContext.UseCases;

namespace TaskFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ── CQRS handlers
        
        services.AddScoped<IQueryHandler<GetAll.Query, PagedResult<TaskResponse>>, GetAll.Handler>();
        services.AddScoped<IQueryHandler<GetById.Query, TaskResponse?>, GetById.Handler>();
        services.AddScoped<ICommandHandler<Create.Command, Result<TaskResponse>>, Create.Handler>();
        services.AddScoped<ICommandHandler<Update.Command, Result<TaskResponse>>, Update.Handler>();
        services.AddScoped<ICommandHandler<Patch.Command, Result<TaskResponse>>, Patch.Handler>();
        services.AddScoped<ICommandHandler<Delete.Command, Result<bool>>, Delete.Handler>();
            
        return services;
    }
}