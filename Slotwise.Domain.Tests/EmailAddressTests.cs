using Slotwise.Domain.Sessions.ValueObjects;

namespace Slotwise.Domain.Tests;

public class EmailAddressTests
{
    [Theory]
    [InlineData("sam@example.com", "sam@example.com")]
    [InlineData("SAM@Example.COM", "sam@example.com")]
    public void Constructor_ValidEmail_StoresNormalizedValue(string input, string expected)
    {
        Assert.Equal(expected, new EmailAddress(input).Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("plainstring")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.com")]
    [InlineData("no-dot@domain")]
    [InlineData("has space@example.com")]
    public void Constructor_InvalidEmail_Throws(string? input)
    {
        Assert.Throws<ArgumentException>(() => new EmailAddress(input!));
    }

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        Assert.Equal("sam@example.com", new EmailAddress("Sam@Example.com").ToString());
    }

    [Fact]
    public void Equality_DifferentCaseSameAddress_IsEqual()
    {
        Assert.Equal(new EmailAddress("Sam@Example.com"), new EmailAddress("sam@example.com"));
    }

    [Fact]
    public void Equality_DifferentAddress_IsNotEqual()
    {
        Assert.NotEqual(new EmailAddress("a@example.com"), new EmailAddress("b@example.com"));
    }
}
