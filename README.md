# AudioTagger

A .NET CLI program that can perform the following actions on audio files:

- View ID3v2 tags
- Auto-update tags using filename patterns
- Update a single tag for multiple files at once
- Auto-rename and reorganize files using filename patterns
- Reorganize files into folders using their tag data
- Find duplicate files by their tags
- Auto-write genres for registered artists
- See folder stats
- Apply audio normalization (ReplayGain)

This is a little labor-of-love project that I work on in my spare time. It's not particularly polished, but it's sufficient for my use case. It relies on the [TagLibSharp](https://github.com/mono/taglib-sharp) library for tag reading and writing.

## Requirements

- [.NET 8 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Running

Currently, you must run this app from the `AudioTagger.Console` folder using `dotnet run`. Passing no arguments will show the instructions.

Additionally, `settings.json` should exist in the application directory for some features. A sparsely populated file will be automatically created if it does not already exist when the program is started.

Below is a snippet that illustrates the supported nodes with some examples:

```json
{
    "duplicates": {
        "titleReplacements": [
            "Single Version",
            "Single Ver.",
            "Single Ver",
            "()",
            "（）",
            "•",
            "・"
        ]
    },
    "renamePatterns": [
        "%ARTISTS% - %ALBUM% - %TRACK% - %TITLE%",
        "%ARTISTS% - %TITLE% [%YEAR%]",
        "%ARTISTS% - %TITLE%",
        "%TITLE%"
    ],
    "tagging": {
        "regexPatterns": [
            "(?<artists>.+) - (?<album>.+) - (?<title>.+?(?:\\.{3})?) ?(?:\\[(?<year>\\d{4})\\])? ?(?:\\{(?<genres>.+?)\\})?(?=\\..+)",
            "(?<artists>.+) - (?<title>.+?(?:\\.{3})?)(?: \\[(?<year>\\d{4})\\])?(?: \\{(?<genres>.+?)\\})?(?=\\.\\S{3,4}$)",
            "(?<title>.+?) ?(?:\\[(?<year>\\d{4})\\])? ?(?:\\{(?<genres>.+?)\\})?(?=\\.[^.]+$)"
        ]
    },
    "artistGenres": {}
}
```

Explanation of options:
- `titleReplacements`: Optional. Substrings in titles that will be ignored when searching for duplicate files. This allows pairs like "My Song" and "My Song (Single Version)" to be considered identical. Otherwise, they would be considered separate titles.
- `renamePatterns`: Mandatory. When renaming files, they will be renamed according to the first such pattern that matches the populated tags in each file. For example, if a file contains only artist and title information, then it will be renamed per the third option above.
- `regexPatterns`: Mandatory. When tagging files, these regexes are used to match against the filenames. When there is a match, then the appropriate ID3 tags are updated in the file.
- `artistGenres`: Auto-populated when the `-g` option is supplied.
