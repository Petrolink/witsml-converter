namespace Petrolink.WitsmlConverter.Tests;

public class Witsml20UnitConverterTests
{
    [Theory]
    [InlineData(true, "CV", "hp[metric]")]
    [InlineData(true, "MY", "Ma[t]")]
    [InlineData(true, "1000ft3", "1000 ft3")]
    [InlineData(false, "unknown/unit", null)]
    public void TryConverter14To20_14Units_20Units(bool success, string before, string? after)
    {
        var result = Witsml20UnitConverter.TryConvert14To20(before, out var resultUnit);

        Assert.Equal(success, result);
        Assert.Equal(after, resultUnit);
    }

    [Theory]
    [InlineData(true, "hp[metric]", "CV")]
    [InlineData(true, "Ma[t]", "MY")]
    [InlineData(true, "1000 ft3", "1000ft3")]
    [InlineData(false, "unknown/unit", null)]
    public void TryConverter20To14_20Units_14Units(bool success, string before, string? after)
    {
        var result = Witsml20UnitConverter.TryConvert20To14(before, out var resultUnit);

        Assert.Equal(success, result);
        Assert.Equal(after, resultUnit);
    }
}
