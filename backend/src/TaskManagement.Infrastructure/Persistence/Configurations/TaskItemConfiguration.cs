using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(task => task.Description)
            .HasMaxLength(2000);

        builder.Property(task => task.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Stored as int so tasks can be ordered Low < Medium < High. No database-level
        // default: the application always sets Priority explicitly (defaulting to Medium),
        // so a DB default would incorrectly override an explicit "Low" (CLR default 0).
        builder.Property(task => task.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(task => task.CreatedAtUtc).IsRequired();
        builder.Property(task => task.UpdatedAtUtc).IsRequired();

        // Helpful indexes for the common filter/sort operations.
        builder.HasIndex(task => task.IsCompleted);
        builder.HasIndex(task => task.Priority);
        builder.HasIndex(task => task.DueDateUtc);
    }
}
