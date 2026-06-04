using TaskFlow.Domain.SharedContext.UseCases;
using TaskFlow.Domain.TaskContext.ValueObjects;

namespace TaskFlow.UnitTests.Domain.TaskContext.ValueObjects;

public sealed class TaskTitleTests
{
    [Fact]
    public void Create_ValidTitle_ReturnsSuccess()
    {
        var result = TaskTitle.Create("Buy groceries");

        Assert.True(result.IsSuccess);
        Assert.Equal("Buy groceries", result.Value!.Value);
    }

    [Fact]
    public void Create_TrimsLeadingAndTrailingWhitespace()
    {
        var result = TaskTitle.Create("  My Task  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("My Task", result.Value!.Value);
    }

    [Fact]
    public void Create_ExactlyAtMaxLength_ReturnsSuccess()
    {
        var result = TaskTitle.Create(new string('a', 200));

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ReturnsValidationError(string value)
    {
        var result = TaskTitle.Create(value);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Create_ExceedsMaxLength_ReturnsValidationError()
    {
        var result = TaskTitle.Create(new string('a', 201));

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public void ImplicitStringOperator_ReturnsUnderlyingValue()
    {
        var title = TaskTitle.FromTrustedSource("My Task");
        string asString = title;

        Assert.Equal("My Task", asString);
    }

    [Fact]
    public void ToString_ReturnsUnderlyingValue()
    {
        var title = TaskTitle.FromTrustedSource("My Task");

        Assert.Equal("My Task", title.ToString());
    }

    [Fact]
    public void FromTrustedSource_BypassesValidation_AndReturnsTitle()
    {
        var title = TaskTitle.FromTrustedSource("Loaded from DB");

        Assert.Equal("Loaded from DB", title.Value);
    }
}
