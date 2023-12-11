using AudioTagger.Library.MediaFiles;

namespace AudioTagger;

public interface IPrinter
{
    void Print(string message, byte prependLines = 0, byte appendLines = 0, string prependText = "",
               ConsoleColor? fgColor = null, ConsoleColor? bgColor = null, bool addLinebreak = true);

    void Print(IEnumerable<LineSubString> lineParts, byte prependLines = 0, byte appendLines = 1);

    void Print(IEnumerable<OutputLine> lines, byte prependLines = 0, byte appendLines = 1);

    void Print(string message, ResultType type, byte prependLines = 0, byte appendLines = 0);

    void Error(string message);

    void Warning(string message);

    void Success(string message);

    char GetResultSymbol(ResultType type);

    void PrintTagDataToTable(MediaFile mediaFile, IDictionary<string, string> proposedUpdates, bool includeComments);

    void PrintException(Exception ex);
}
