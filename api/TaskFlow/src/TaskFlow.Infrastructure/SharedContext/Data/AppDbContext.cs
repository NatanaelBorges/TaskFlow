using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.TaskContext.Entities;

namespace TaskFlow.Infrastructure.SharedContext.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}