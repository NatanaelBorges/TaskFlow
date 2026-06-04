using TaskFlow.Api.Helpers;

namespace TaskFlow.UnitTests.Api.Helpers;

public sealed class PaginationLinksHelperTests
{
    private const string Base = "http://localhost/api/v1/tasks";

    [Fact]
    public void Generate_AlwaysIncludesSelfFirstAndLast()
    {
        var links = PaginationLinksHelper.Generate(Base, 2, 10, 5);

        Assert.True(links.ContainsKey("self"));
        Assert.True(links.ContainsKey("first"));
        Assert.True(links.ContainsKey("last"));
    }

    [Fact]
    public void Generate_SelfLink_PointsToCurrentPage()
    {
        var links = PaginationLinksHelper.Generate(Base, 3, 10, 5);

        Assert.Contains("page=3", links["self"]!.Href);
    }

    [Fact]
    public void Generate_FirstLink_PointsToPageOne()
    {
        var links = PaginationLinksHelper.Generate(Base, 3, 10, 5);

        Assert.Contains("page=1", links["first"]!.Href);
    }

    [Fact]
    public void Generate_LastLink_PointsToTotalPages()
    {
        var links = PaginationLinksHelper.Generate(Base, 1, 10, 7);

        Assert.Contains("page=7", links["last"]!.Href);
    }

    [Fact]
    public void Generate_NotOnLastPage_IncludesNextLink()
    {
        var links = PaginationLinksHelper.Generate(Base, 2, 10, 5);

        Assert.True(links.ContainsKey("next"));
        Assert.NotNull(links["next"]);
        Assert.Contains("page=3", links["next"]!.Href);
    }

    [Fact]
    public void Generate_OnLastPage_NextLinkIsNull()
    {
        var links = PaginationLinksHelper.Generate(Base, 5, 10, 5);

        Assert.Null(links["next"]);
    }

    [Fact]
    public void Generate_NotOnFirstPage_IncludesPrevLink()
    {
        var links = PaginationLinksHelper.Generate(Base, 3, 10, 5);

        Assert.True(links.ContainsKey("prev"));
        Assert.NotNull(links["prev"]);
        Assert.Contains("page=2", links["prev"]!.Href);
    }

    [Fact]
    public void Generate_OnFirstPage_PrevLinkIsNull()
    {
        var links = PaginationLinksHelper.Generate(Base, 1, 10, 5);

        Assert.Null(links["prev"]);
    }

    [Fact]
    public void Generate_WithStatusFilter_AppendsStatusToAllLinks()
    {
        var links = PaginationLinksHelper.Generate(Base, 1, 10, 3, "active");

        Assert.All(links.Values, l => Assert.True(l is null || l.Href.Contains("status=active")));
    }

    [Fact]
    public void Generate_WithoutStatusFilter_DoesNotAppendStatusParam()
    {
        var links = PaginationLinksHelper.Generate(Base, 1, 10, 3);

        Assert.DoesNotContain(links.Values, l => l is not null && l.Href.Contains("status="));
    }

    [Fact]
    public void Generate_AllLinksUseGetMethod()
    {
        var links = PaginationLinksHelper.Generate(Base, 2, 10, 5);

        Assert.All(links.Values.Where(l => l is not null), l => Assert.Equal("GET", l!.Method));
    }
}
