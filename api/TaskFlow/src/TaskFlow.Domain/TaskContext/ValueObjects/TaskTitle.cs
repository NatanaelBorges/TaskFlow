using TaskFlow.Domain.SharedContext.UseCases;

namespace TaskFlow.Domain.TaskContext.ValueObjects;

public sealed record TaskTitle
{
    public string Value { get; }

    private TaskTitle(string value) => Value = value;

    public static Result<TaskTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<TaskTitle>.ValidationError("Title must not be empty.");

        var trimmed = value.Trim();

        if (trimmed.Length > 200)
            return Result<TaskTitle>.ValidationError("Title must not exceed 200 characters.");

        return Result<TaskTitle>.Success(new TaskTitle(trimmed));
    }

    public static TaskTitle FromTrustedSource(string value) => new(value);
    
    public static implicit operator string(TaskTitle title) => title.Value;
    public override string ToString() => Value;
}