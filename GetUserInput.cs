using System;
using System.Collections.Generic;

namespace AudioTagger
{
    public enum UserReponse
    {
        None,
        Yes,
        No,
        Cancel
    }

    public static class ResponseHandler
    {
        // TODO: Make generic Yes/No and Yes/No/Cancel variants?
        /// <summary>
        /// Get a response from the user via a single key press.
        /// </summary>
        public static UserReponse GetUserResponse(IList<LineSubString> question,
                                                  IReadOnlyDictionary<char, UserReponse> allowedResponses)
        {
            Printer.Print(question);

            var validInput = false;

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

            return UserReponse.None; // Perhaps throw an exception since this should never be hit.
        }
    }    
}
