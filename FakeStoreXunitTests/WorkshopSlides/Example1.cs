using AutoBogus;
using Microsoft.VisualBasic;

public class User
{
    public string Name { get; set; }
    public int Age { get; set; }

    public bool IsAdult()
    {
        return Age >= 18;
    }
}

public class UserTests
{
    [Fact]
    public void IsAdult_ShouldReturnTrueForAge18()
    {
        // Arrange
        var user = new User { Name = "Alice Santos", Age = 18 };

        // Act
        var result = user.IsAdult();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAdult_ShouldReturnTrueForAgeGreaterThan18()
    {
        // Arrange
        var user = new User { Name = "Bob Costa", Age = 22 };

        // Act
        var result = user.IsAdult();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(17)]
    [InlineData(16)]
    [InlineData(1)]
    public void IsAdult_ShouldReturnFalseForAgeLessThan18(int age)
    {
        // Arrange
        var user = new User { Name = "Carol Dias", Age = age };

        // Act
        var result = user.IsAdult();

        // Assert
        Assert.False(result);
    }
}

public class MathTests
{
    [Theory]
    [InlineData(5, 5, 10)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, -1, -2)]
    public void Add_ShouldCalculateCorrectSum(int a, int b, int expectedSum)
    {
        // Act
        var result = Add(a, b);

        // Assert
        Assert.Equal(expectedSum, result);
    }

    public int Add(int x, int y)
    {
        return x + y;
    }
}

public class UserTestsWithFaker
{
    [Fact]
    public void IsAdult_ShouldReturnTrueForAdults()
    {
        // Arrange
        var userFaker = new AutoFaker<User>();
        userFaker.RuleFor(u => u.Age, (f) => f.Random.Int(18, 100));
        var user = userFaker.Generate();

        // Act
        var result = user.IsAdult();

        // Assert
        Assert.True(result);
    }
}
