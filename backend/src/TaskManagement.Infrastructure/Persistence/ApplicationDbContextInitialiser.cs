using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Infrastructure.Persistence;

/// <summary>
/// Applies pending migrations and seeds initial data (a default user and a few
/// sample tasks) so the application is usable immediately after first run.
/// </summary>
public class ApplicationDbContextInitialiser
{
    /// <summary>Default seeded login. Documented in the README.</summary>
    public const string DefaultUsername = "admin";
    public const string DefaultPassword = "Passw0rd!";

    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _context = context;
        _passwordHasher = passwordHasher;
    }

    /// <summary>Creates the database and applies any pending migrations.</summary>
    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    /// <summary>Seeds the default user and sample tasks when the tables are empty.</summary>
    public async Task SeedAsync()
    {
        try
        {
            await SeedDefaultUserAsync();
            await SeedSampleTasksAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedDefaultUserAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            return;
        }

        _context.Users.Add(new AppUser
        {
            Username = DefaultUsername,
            DisplayName = "Administrator",
            PasswordHash = _passwordHasher.Hash(DefaultPassword),
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(CancellationToken.None);
        _logger.LogInformation("Seeded default user '{Username}'.", DefaultUsername);
    }

    private async Task SeedSampleTasksAsync()
    {
        if (await _context.Tasks.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var samples = new List<TaskItem>
        {
            new()
            {
                Title = "Set up project repository",
                Description = "Initialise the Git repository and push the initial commit.",
                Priority = TaskPriority.High,
                IsCompleted = true,
                DueDateUtc = now.AddDays(-3),
                CreatedAtUtc = now.AddDays(-5),
                UpdatedAtUtc = now.AddDays(-3)
            },
            new()
            {
                Title = "Design the database schema",
                Description = "Model tasks and users, then create the EF Core migrations.",
                Priority = TaskPriority.Medium,
                IsCompleted = true,
                DueDateUtc = now.AddDays(-1),
                CreatedAtUtc = now.AddDays(-4),
                UpdatedAtUtc = now.AddDays(-1)
            },
            new()
            {
                Title = "Implement the Tasks CRUD API",
                Description = "Expose create, read, update, delete and complete endpoints.",
                Priority = TaskPriority.High,
                IsCompleted = false,
                DueDateUtc = now.AddDays(2),
                CreatedAtUtc = now.AddDays(-2),
                UpdatedAtUtc = now.AddDays(-2)
            },
            new()
            {
                Title = "Build the Angular UI",
                Description = "List and edit tasks side by side with sorting and filtering.",
                Priority = TaskPriority.Medium,
                IsCompleted = false,
                DueDateUtc = now.AddDays(5),
                CreatedAtUtc = now.AddDays(-1),
                UpdatedAtUtc = now.AddDays(-1)
            },
            new()
            {
                Title = "Write the project documentation",
                Description = "Document setup, configuration and how to run the application.",
                Priority = TaskPriority.Low,
                IsCompleted = false,
                DueDateUtc = now.AddDays(7),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            }
        };

        _context.Tasks.AddRange(samples);
        await _context.SaveChangesAsync(CancellationToken.None);
        _logger.LogInformation("Seeded {Count} sample tasks.", samples.Count);
    }
}
