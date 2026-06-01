namespace TaskManagement.Domain.Enums;

/// <summary>
/// Relative importance of a task. Stored as an <see cref="int"/> in the database
/// so that tasks can be ordered Low &lt; Medium &lt; High.
/// </summary>
public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}
