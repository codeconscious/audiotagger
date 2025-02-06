# AudioTagger

AudioTagger is .NET CLI program that performs various metadata-related operations on supported audio files. It relies on the [TagLibSharp](https://github.com/mono/taglib-sharp) library for tag reading and writing operations.

This is a little labor-of-love project that I work on in my spare time. While I maintain it for my own use, feel free to use it yourself. However, please note that it's geared to my own personal use case and that no warranties or guarantees are provided.

Additionally, your original audio files will be modified during selected operations, so I recommend making backups of your files to be safe.

## Requirements

- [.NET 9 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- `settings.json` (See below)

## Running

Run this app from the `AudioTagger.Console` folder using `dotnet run`. Pass one or more flags followed by at least one directory containing audio files or an audio filename.

Examples:

```sh
# Show a summary of metadata for all audio files within the specific directory, recursively:
dotnet run -- -vs ~/Downloads/Audio

# Do the same for two directories:
dotnet run -- -vs ~/Downloads/Audio ~/Documents/Media

# Do the same for a single file:
dotnet run -- -vs ~/Downloads/Audio/free-audio.m4a

# Update all tags via filename patterns, auto-update the genres, then show a tag summary:
dotnet run -- -u -ug -vs ~/Downloads/Audio/

# Rename all audio files within a folder using the pattern(s) in your rename settings.
dotnet run -- -r ~/Downloads/Audio/
```

## Flags

| Flags | Description
|---|---|
| -v, --view | View full tag data.
| -vs, --view-summary | View a summary of tag data.
| -u, --update | Update tag data using filename patterns from the settings.
| -u1, --update-single | Update a single tag in multiple files to a single, manually-specified value.
| -ug, --update-genres | Update the genres in all files automatically using the CSV specified in the settings.
| -um, --update-multiple | Update a single tag in multiple files with multiple values.
| -uy, --update-year | Update the year using media files' own dates of creation. (Must do before other updates, lest the creation date be modified by those updates.)
| -urt, --reverse-track-numbers | Reverse the track numbers of the given files.
| -uea, --extract-artwork | Extracts artwork from directory files if they have the same artist and album, then deletes the artwork from the files containing it.
| -ura, --remove-artwork | Removes artwork from files. (File size is not reduced, as padding remains.)
| -rt, --rewrite-tags | Rewrites file tags. (Can be helping in reducing padding, such as from removed artwork.)
| -r, --rename | Rename and reorganize files into folders based on tag data.
| -d, --duplicates | List tracks with identical artists and titles. No files are modified or deleted.
| -s, --stats | Display file statistics based on tag data.
| -g, --genres | Save the primary genre for each artist to a genre file.
| -p, --parse | Get a single tag value by parsing the data of another (generally Comments).

Passing no arguments will also display these instructions.

### Settings

The file `settings.json` must exist in the application directory for some features. A sparsely populated file will be automatically created if it does not already exist when the program is started.

A sample settings file, which can you copy and paste if you wish, follows:

```json
{
  "artistGenreCsvFilePath": "/Users/me/Documents/audio/artist-genres.csv",
  "resetSavedArtistGenres": true,
  "tagging": {
    "regexPatterns": [
      "(?:(?<albumArtists>.+) ≡ )?(?<album>.+?)(?: ?\\[(?<year>\\d{4})\\])? = (?<trackNo>\\d+) [–-] (?<artists>.+?) [–-] (?<title>.+)(?=\\.(?:m4a|opus))",
      "(?:(?<albumArtists>.+) ≡ )?(?<album>.+?)(?: ?\\[(?<year>\\d{4})\\])? = (?<trackNo>\\d{1,3}) [–-] (?<title>.+)(?=\\.(?:m4a|opus))",
      "(?:(?<albumArtists>.+) ≡ )(?<album>.+?)(?: ?\\[(?<year>\\d{4})\\])? = (?<artists>.+?) [–-] (?<title>.+)(?=\\.(?:m4a|opus))",
      "(?:(?<albumArtists>.+) ≡ )?(?<album>.+?)(?: ?\\[(?<year>\\d{4})\\])? = (?<title>.+)(?=\\.(?:m4a|opus))",    ]
  },
  "renaming": {
    "useAlbumDirectories": true,
    "ignoredDirectories": [
      "Directory 1",
      "Directory 2"
    ],
    "patterns": [
      "%ALBUMARTISTS% ≡ %ALBUM% [%YEAR%] = %TRACK% - %ARTISTS% - %TITLE%",
      "%ARTISTS% - %ALBUM% [%YEAR%] - %TRACK% - %TITLE%",
      "%ALBUMARTISTS% ≡ %ARTISTS% - %ALBUM% [%YEAR%] - %TITLE%",
      "%ARTISTS% - %ALBUM% [%YEAR%] - %TITLE%",
      "%ARTISTS% - %ALBUM% - %TRACK% - %TITLE%",
      "%ARTISTS% - %ALBUM% - %TITLE%",
      "%ARTISTS% - %TITLE% [%YEAR%]",
      "%ARTISTS% - %TITLE%",
      "%TITLE%"
    ]
  },
  "duplicates": {
    "pathSearchFor": "/Users/me/Documents/Media/",
    "pathReplaceWith": "",
    "savePlaylistDirectory": "/Users/me/Downloads/NewMusic",
    "exclusions": [
      { "artist": "Artist Name" },
      { "title": "Track Title" },
      { "artist": "Artist Name", "title": "Track Title" },
    ],
    "artistReplacements": [
      " ",
      "　",
      "The ",
      "ザ・"
    ],
    "titleReplacements": [
      " ",
      "　",
      "-",
      "~",
      "〜",
      "/",
      "／",
      "?",
      "？",
      "!",
      "！",
      "AlbumVersion",
      "AlbumVer",
      "ShortVersion",
      "ShortVer",
      "()",
      "（）",
      "•",
      "・",
      ".",
      ":",
      "："
    ]
  },
}
```

Partial explanation of options:

- `titleReplacements`: Optional. Substrings in titles that will be ignored when searching for duplicate files. This allows pairs like "My Song" and "My Song (Single Version)" to be considered identical. Otherwise, they would be considered separate titles.
- `renamePatterns`: Mandatory. When renaming files, they will be renamed according to the first such pattern that matches the populated tags in each file. For example, if a file contains only artist and title information, then it will be renamed per the third option above.
- `regexPatterns`: Mandatory. When tagging files, these regexes are used to match against the filenames. When there is a match, then the appropriate ID3 tags are updated in the file.
- `artistGenres`: Auto-populated when the `-g` option is supplied.
