# AudioTagger

A .NET CLI program that can perform the following actions on audio files:

- View IDE2 tags
- Auto-update tags using filename patterns
- Auto-rename and reorganize files using filename patterns
- Reorganize files into folders using their tag data
- Find duplicate files by their tags
- See folder stats
- Apply audio normalization (ReplayGain)

This is a little labor-of-love project that I work on in my spare time. It's not particularly polished, but it's sufficient for my use case.

Requirements: .NET 7 runtime.

Currently, you must run this app from the `AudioTagger.Console` folder using `dotnet run`. Passing no arguments will show the instructions.

Additionally, you need to create `settings.json` in the application directory for some features—namely, updating tags (mandatory) and checking for duplicates (optional). Below is a sample that illustrates the necessary nodes:

```json
{
    "duplicates": {
        "titleReplacements": [
            "Short Version",
            "Short Ver.",
            "Short Ver",
            "()",
            "（）",
            "•",
            "・"
        ]
    },
    "tagging": {
        "regexPatterns": [
            "(?<artists>.+) - (?<album>.+) - (?<title>.+?(?:\\.{3})?) ?(?:\\[(?<year>\\d{4})\\])? ?(?:\\{(?<genres>.+?)\\})?(?=\\..+)",
            "(?<artists>.+) - (?<title>.+?(?:\\.{3})?)(?: \\[(?<year>\\d{4})\\])?(?: \\{(?<genres>.+?)\\})?(?=\\.\\S{3,4}$)",
            "(?<title>.+?) ?(?:\\[(?<year>\\d{4})\\])? ?(?:\\{(?<genres>.+?)\\})?(?=\\.[^.]+$)"
        ]
    }
}
```
