using System.IO;
using FluentResults;
using Microsoft.VisualBasic.FileIO;

namespace AudioTagger.Library.Genres;

public static class GenreService
{
    private static readonly string[] Delimiters = ["＼"];

    /// <summary>
    /// Write settings to the specified file.
    /// </summary>
    /// <returns>A bool indicating success or failure.</returns>
    public static Result Write(string genreFileName,
                               IDictionary<string, string> artistsWithGenres)
    {
        try
        {
            var lines = artistsWithGenres
                .Where(ag => !string.IsNullOrWhiteSpace(ag.Key))
                .OrderBy(ag => ag.Key)
                .Select(ag => $"{ag.Key}{Delimiters[0]}{ag.Value}");
            File.WriteAllLines(genreFileName, lines);
            return Result.Ok();
        }
        catch (FileNotFoundException)
        {
            return Result.Fail($"Settings file \"{genreFileName}\" is missing.");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Could not save the genre file: {ex.Message}");
        }
    }

    public static Result<Dictionary<string, string>> Read(string? genreFileName)
    {
        if (genreFileName is null)
        {
            return Result.Fail($"No artist-genre file name was passed.");
        }

        Dictionary<string, string> artistsWithGenres = [];
        try
        {
            using TextFieldParser csvParser = new(genreFileName);
            // csvParser.CommentTokens = ["#"];
            csvParser.SetDelimiters(Delimiters);
            csvParser.HasFieldsEnclosedInQuotes = false;
            // csvParser.ReadLine(); // Skip header row

            while (!csvParser.EndOfData)
            {
                // Read current line and move pointer to the following line.
                string[] fields = csvParser.ReadFields() ?? [];
                artistsWithGenres.Add(fields[0], fields[1]);
            }

            return Result.Ok(artistsWithGenres);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Reading the genre file failed: {ex.Message}");
        }
    }

    private static string EscapeQuotationMarks(string text) => text.Replace("\"", "\\\"");
}
