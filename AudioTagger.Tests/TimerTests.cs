using Xunit;
using AudioTagger.Library;
using System;

namespace AudioTagger.Tests;

public sealed class TimerTests
{
    [Fact]
    public void Milliseconds_OneDigit_FormatsCorrectly()
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(1);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "1ms";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Milliseconds_TwoDigits_FormatsCorrectly()
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(99);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "99ms";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Milliseconds_ThreeDigits_FormatsCorrectly()
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(999);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "999ms";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Seconds_NoDecimals_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(0, 0, 3);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "3.00s";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Seconds_OneDecimal_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(0, 0, 0, 3, 500);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "3.50s";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Seconds_TwoDecimals_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(0, 0, 0, 3, 520);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "3.52s";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Minutes_NoSeconds_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(0, 1, 0);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "1m exactly";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Minutes_OneDecimal_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(0, 1, 30);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "1m30s";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Minutes_OverTen_NoSeconds_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(0, 59, 0);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "59m exactly";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Minutes_OverTen_WithSeconds_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(0, 59, 30);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "59m30s";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SingleDigitHour_NoMinutes_NoSeconds_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(7, 20, 0);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "7h20m exactly";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DoubleDigitHour_NoMinutes_NoSeconds_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(13, 0, 0);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "13h exactly";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SingleDigitHour_SingleDigitMinutes_NoSeconds_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(1, 5, 0);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "1h05m exactly";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SingleDigitHour_DoubleDigitMinutes_NoSeconds_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(1, 55, 0);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "1h55m exactly";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SingleDigitHour_DoubleDigitMinutes_SingleDigitSeconds_FormatsCorrectly()
    {
        TimeSpan timeSpan = new(1, 55, 8);
        string actual = timeSpan.ElapsedFriendly();
        string expected = "1h55m08s";
        Assert.Equal(expected, actual);
    }
}
