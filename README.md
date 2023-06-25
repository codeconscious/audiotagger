# AudioTagger

A .NET 7 CLI program that can perform the following actions on audio files:

- View ID3v2 tags
- Auto-update tags using filename patterns
- Auto-rename and reorganize files using filename patterns
- Reorganize files into folders using their tag data
- Find duplicate files by their tags
- See folder stats
- Apply audio normalization (ReplayGain)

This is a personal, labor-of-love project that I work on in my spare time, and it's not particularly polished, but it works very well for my use case. Feel feel to try it out yourself.

Requirements: .NET 7 runtime.

Currently, you must run this app from the `AudioTagger.Console` folder using `dotnet run`. Passing no arguments will show the instructions.
