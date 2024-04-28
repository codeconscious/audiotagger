namespace AudioTagger.Library.FSharp

module IO =
    open System
    open System.IO

    type PathItem =
        | File of string
        | Directory of string * string list
        | Invalid of string

    let private directoryFiles (directoryPath: string) (recursively: bool) =
        let supportedExtensions =
            [".mp3"; ".ogg"; ".mkv"; ".mp4"; ".m4a"]
        let hasSupportedExtension (fileName:string) =
            supportedExtensions
            |> List.exists (fun f ->
                fileName.EndsWith(f, StringComparison.InvariantCultureIgnoreCase))

        try
            Ok(
                Directory
                    .EnumerateFiles(
                        directoryPath,
                        "*.*", // TODO: Check if multiple can be specified
                        match recursively with
                            | true  -> SearchOption.AllDirectories
                            | false -> SearchOption.TopDirectoryOnly)
                    |> Seq.cast<string>
                    |> Seq.toList
                    |> List.filter hasSupportedExtension
            )
        with
            | e -> Error $"Files unexpectedly could not be read from \"{directoryPath}\": {e.Message}"

    let private readPathFileNames path =
        if Directory.Exists path
        then match directoryFiles path true with
             | Ok i -> Directory (path, i)
             | Error e -> Invalid path
        else if File.Exists path
        then File path
        else Invalid path

    [<CompiledName("ReadPathFilenames")>]
    let readPathFileNames2 (paths:string[]) =
        paths
        |> Array.map readPathFileNames
