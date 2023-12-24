using Xunit;
using AudioTagger.Library;

namespace AudioTagger.Tests;

public sealed class UtilityMethodTests
{
    [Fact]
    public void Milliseconds_OneDigit_FormatsCorrectly()
    {
        double milliseconds = 1;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "1ms";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Milliseconds_TwoDigits_FormatsCorrectly()
    {
        double milliseconds = 99;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "99ms";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Milliseconds_ThreeDigits_FormatsCorrectly()
    {
        double milliseconds = 999;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "999ms";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Seconds_NoDecimals_FormatsCorrectly()
    {
        double milliseconds = 3000;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "3s";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Seconds_OneDecimal_FormatsCorrectly()
    {
        double milliseconds = 3500;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "3.5s";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Seconds_TwoDecimals_FormatsCorrectly()
    {
        double milliseconds = 3520;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "3.52s";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Minutes_NoDecimals_FormatsCorrectly()
    {
        double milliseconds = 4_500_000;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "75min";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Minutes_OneDecimal_FormatsCorrectly()
    {
        double milliseconds = 4_530_000;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "75.5min";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Minutes_TwoDecimals_FormatsCorrectly()
    {
        double milliseconds = 4_533_000;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "75.55min";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Hours_NoDecimals_FormatsCorrectly()
    {
        double milliseconds = 25_200_000;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "7hr";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Hours_OneDecimal_FormatsCorrectly()
    {
        double milliseconds = 25_920_000;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "7.2hr";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Hours_TwoDecimals_FormatsCorrectly()
    {
        double milliseconds = 26_172_000;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "7.27hr";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Custom_FormatsCorrectly()
    {
        double milliseconds = 5142.783;
        string actual = Utilities.FormatMsAsTime(milliseconds);
        string expected = "5.06s";
        Assert.Equal(expected, actual);
    }
}
