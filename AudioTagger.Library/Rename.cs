using System.IO;
using System.Text;
using System.Globalization;
using AudioTagger.Library.MediaFiles;
using AudioTagger.Library.Settings;

namespace AudioTagger;

public sealed class FileRenamer : IPathOperation
{
    public void Start(IReadOnlyCollection<MediaFile> filesData,
                      DirectoryInfo workingDirectory,
                      IPrinter printer,
                      Settings settings)
    {
        foreach (var fileData in filesData)
        {
            if (fileData == null)
            {
                printer.Error("Skipped invalid file.");
            }
            else
            {
                try
                {
                    var (wasDone, message) = RenameFile(fileData, printer);
                    printer.Print(wasDone ? "◯ " : "× " + message); // TODO: Refactor
                }
                catch (TagLib.CorruptFileException e)
                {
                    printer.Error("The file's tag metadata was corrupt or missing: " + e.Message);
                    continue;
                }
                catch (Exception e)
                {
                    printer.Error("An error occurred: " + e.Message);
                    continue;
                }
            }
        }
    }

    public static (bool wasDone, string message) RenameFile(MediaFile fileData, IPrinter printer)
    {
        printer.Print("Entered rename method...");
        var fileName = fileData.Path;

        // Check mandatory fields
        if (string.IsNullOrWhiteSpace(fileData.Title))
            return (false, "Rename cancelled due to missing TITLE tag: " + Path.GetFileName(fileName));

        var newFileName = new StringBuilder();

        var artist = fileData.Artists.Join();
        var title = fileData.Title;
        var year = fileData.Year.ToString(CultureInfo.InvariantCulture);
        var genre = fileData.Genres.Join();

        if (!string.IsNullOrWhiteSpace(artist))
            newFileName.Append(artist).Append(" - ");

        newFileName.Append(title);

        if (!string.IsNullOrWhiteSpace(year))
            newFileName.Append(" [").Append(year).Append(']');

        if (!string.IsNullOrWhiteSpace(genre))
            newFileName.Append(" {").Append(genre).Append('}');

        printer.Print("Rename file:");
        printer.Print("OLD: " + fileName);
        printer.Print("NEW: " + newFileName.ToString());
        if (fileName.Equals(newFileName.ToString()))
            printer.Print("(No changes)");

        Console.Read();

        return (false, "Testing only"); // TODO: Handle this...
    }
}
