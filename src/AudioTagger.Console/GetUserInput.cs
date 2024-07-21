using System;
using System.Linq;
using System.Collections.Generic;
using AudioTagger.Library;

namespace AudioTagger.Console;

public enum UserResponse
{
    None,
    Yes,
    No,
    Cancel
}

public sealed record KeyResponse
{
    public char Key { get; init; }
    public UserResponse Response { get; init; }

    public KeyResponse(char key, UserResponse response)
    {
        Key = char.ToLowerInvariant(key);
        Response = response;
    }
}

public static class ResponseHandler
{
    /// <summary>
    /// Ask the user a question that they can answer with a single keystroke.
    /// </summary>
    public static UserResponse AskUserQuestion(IReadOnlyList<LineSubString> question,
                                               IReadOnlyList<KeyResponse> allowedResponses,
                                               IPrinter printer)
    {
        if (question.None())
        {
            throw new ArgumentException(
                "Question data must be provided",
                nameof(question));
        }

        if (allowedResponses.None())
        {
            throw new ArgumentException(
                "At least one allowed response must be provided",
                nameof(allowedResponses));
        }

        printer.Print(question);

        // Take no action until a valid key is pressed.
        while (true)
        {
            ConsoleKeyInfo keyInfo = System.Console.ReadKey(true);
            char keyChar = char.ToLowerInvariant(keyInfo.KeyChar);

            KeyResponse? relevantKeyResponse =
                allowedResponses
                    .Where(r => r.Key == keyChar)?
                    .FirstOrDefault();

            if (relevantKeyResponse == null)
                continue;

            return relevantKeyResponse.Response;
        }
    }

    /// <summary>
    /// Ask the user a question to which they can answer Yes, No, or Cancel with a single keystroke.
    /// </summary>
    /// <returns></returns>
    public static UserResponse AskUserYesNoCancel(IPrinter printer)
    {
        var question = new LineSubString[]
        {
            new ("Press "),
            new ("Y", ConsoleColor.Magenta),
            new (" or "),
            new ("N", ConsoleColor.Magenta),
            new (" (or "),
            new ("C", ConsoleColor.Magenta),
            new (" to cancel):  "),
        };

        var allowedResponses = new List<KeyResponse>
        {
            new('y', UserResponse.Yes),
            new('n', UserResponse.No),
            new('c', UserResponse.Cancel)
        };

        return AskUserQuestion(question, allowedResponses, printer);
    }
}
