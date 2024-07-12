using Xunit;

namespace AudioTagger.Tests;

public sealed class OutputLineTests
{
    [Fact]
    public void OutputLine_StringPassedIntoCtor_Succeeds()
    {
        const string testString = "Test output";
        OutputLine outputLine = new(testString);
        Assert.Equal(testString, outputLine.Line[0].Text);
    }

    [Fact]
    public void OutputLine_SubArrayPassedIntoCtor_Succeeds()
    {
        const string testString1 = "Test output 1";
        const string testString2 = "Test output 2";
        LineSubString lineSubString1 = new(testString1);
        LineSubString lineSubString2 = new(testString2);
        LineSubString[] array = [lineSubString1, lineSubString2];

        OutputLine outputLine = new(array);

        Assert.Equal(testString1, outputLine.Line[0].Text);
        Assert.Equal(testString2, outputLine.Line[1].Text);
    }
}
