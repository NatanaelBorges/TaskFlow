using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using TaskFlow.Api.Extensions;
using TaskFlow.Application;
using TaskFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddControllers();
builder.AddDatabase();
builder.AddHateoas();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddControllers();

builder.AddCors();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

app.UseExceptionHandler(options => options.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var env = context.RequestServices.GetRequiredService<IHostEnvironment>();

    var problem = new ProblemDetails
    {
        Status   = StatusCodes.Status500InternalServerError,
        Title    = "An internal error occurred.",
        Instance = $"{context.Request.Method} {context.Request.Path}",
        Extensions = { ["traceId"] = context.TraceIdentifier }
    };

    if (env.IsDevelopment() && exception is not null)
    {
        problem.Detail = exception.Message;
        problem.Extensions["stackTrace"] = exception.StackTrace;
    }

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(problem);
}));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseStatusCodePages();
app.UseCors();
app.MapControllers();

app.Run();
