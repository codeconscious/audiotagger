using System;
using Xunit;

namespace AudioTagger.Tests;

public class OutputLineTests
{
    [Fact]
    public void OutputLine_StringPassedIntoCtor_Succeeds()
    {
        const string testString = "Test output";

        var outputLine = new OutputLine(testString);

        Assert.Equal(testString, outputLine.Line[0].Text);
    }

    [Fact]
    public void OutputLine_SubArrayPassedIntoCtor_Succeeds()
    {
        const string testString1 = "Test output 1";
        const string testString2 = "Test output 2";
        var lineSubString1 = new LineSubString(testString1);
        var lineSubString2 = new LineSubString(testString2);
        var array = new[] { lineSubString1, lineSubString2 };

        var outputLine = new OutputLine(array);

        Assert.Equal(testString1, outputLine.Line[0].Text);
        Assert.Equal(testString2, outputLine.Line[1].Text);
    }
}
