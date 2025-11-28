using Xunit;

namespace ProjArqsi.Tests;

public class BasicTest
{
    [Fact]
    public void BasicMath_AlwaysPasses()
    {
        // Arrange
        int expected = 2;
        int actual = 1 + 1;

        // Act & Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void StringComparison_AlwaysPasses()
    {
        // Arrange
        string expected = "Hello";
        string actual = "Hello";

        // Act & Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BooleanCheck_AlwaysPasses()
    {
        // Act & Assert
        Assert.True(true);
        Assert.False(false);
    }
}