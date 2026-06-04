using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskFlow.Domain.TaskContext.Entities;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.Infrastructure.TaskContext.Mappings;

public sealed class TaskItemMapping: IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {        
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id);

        var titleConverter = new ValueConverter<TaskTitle, string>(
            title => title.Value,
            str   => TaskTitle.FromTrustedSource(str));

        builder.Property(t => t.Title)
            .HasConversion(titleConverter)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(t => t.Description)
            .HasMaxLength(2000);
        
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(t => t.IsDeleted);
        builder.Property(t => t.CreatedAtUtc);
        builder.Property(t => t.UpdatedAtUtc);
        builder.Property(t => t.DeletedAtUtc);
        
        builder.Ignore(t => t.IsDeleted);
    }
}