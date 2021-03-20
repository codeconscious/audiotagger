using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public enum UserResponse
    {
        None,
        Yes,
        No,
        Cancel
    }

    public static class ResponseHandler
    {
        /// <summary>
        /// Ask the user a question that they can answer with a single keystroke.
        /// </summary>
        public static UserResponse AskUserQuestion(IList<LineSubString> question,
                                                  IReadOnlyDictionary<char, UserResponse> allowedResponses)
        {
            Printer.Print(question);

            var validInput = false;

            // Take no action until a valid key is pressed.
            do
            {
                var keyInfo = Console.ReadKey(true);
                var keyChar = char.ToLowerInvariant(keyInfo.KeyChar);

                if (allowedResponses.ContainsKey(keyChar))
                {
                    return allowedResponses[keyChar];
                }
            }
            while (!validInput);

            return UserResponse.None; // For the compiler only. Should never be hit.
        }

        /// <summary>
        /// Ask the user a question to which they can answer Yes, No, or Cancel with a single keystroke.
        /// </summary>
        /// <returns></returns>
        public static UserResponse AskUserYesNoCancel()
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

            var allowedResponses = new Dictionary<char, UserResponse>
            {
                { 'y', UserResponse.Yes },
                { 'n', UserResponse.No },
                { 'c', UserResponse.Cancel }
            };

            return AskUserQuestion(question, allowedResponses);
        }
    }    
}
