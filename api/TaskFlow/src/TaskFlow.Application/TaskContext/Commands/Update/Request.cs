using System.ComponentModel.DataAnnotations;
using TaskFlow.Domain.TaskContext.Enums;

namespace TaskFlow.Application.TaskContext.Commands.Update;

public sealed record Request
{
    [Required(ErrorMessage = "Title is required.")]
    [MinLength(1, ErrorMessage = "Title must not be empty.")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    public string Description { get; init; } = string.Empty;

    public TaskItemStatus Status { get; init; }
}