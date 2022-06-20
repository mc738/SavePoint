open System

[<AutoOpen>]
module Common =

    /// Types of save points
    type SavePointType = File of BasePath: string

module Dedupe =

    open System.IO
    open System.Text.Json
    
    [<RequireQualifiedAccess>]
    module File =

        module private Internal =

            /// A helper function to save a string to a file.
            /// Saves the file as [savePoint]-[key].json in the base path.
            let saveToFile (path: string) (savePoint: string) (key: string) (value: string) =
                File.WriteAllText(Path.Combine(path, $"{savePoint}-{key}.json"), value)


            /// A helper function to load a string from a file.
            /// Attempts to load the file called [savePoint]-[key].json from the base path.
            /// If the file doesn't exist None is return.
            let loadFromFile (path: string) (savePoint: string) (key: string) =
                let path =
                    Path.Combine(Path.Combine(path, $"{savePoint}-{key}.json"))

                match File.Exists path with
                | true -> File.ReadAllText path |> Some
                | false -> None

        let saveResult<'T> (path: string) (savePoint: string) (key: string) (value: 'T) =
            JsonSerializer.Serialize value
            |> Internal.saveToFile path savePoint key

        let loadResult<'T> (path: string) (savePoint: string) (key: string) =
            Internal.loadFromFile path savePoint key
            |> Option.map JsonSerializer.Deserialize<'T>
    
    /// Run a function within a save point.
    let runInSavePoint<'T> (savePointType: SavePointType) (savePoint: string) (key: string) (fn: unit -> 'T) =
        // Only one type at the moment, but this could expanded.
        match savePointType with
        | File basePath ->
            // Attempt to love the result. If successful, return it.
            // If not run fn, save the result and return it.
            match File.loadResult<'T> basePath savePoint key with
            | Some result -> result
            | None ->
                let result = fn ()
                File.saveResult basePath savePoint key result
                result

type Foo =
    { Bar: string
      Baz: int
      Timestamp: DateTime }

let testFn _ =
    let timestamp = DateTime.UtcNow
    printfn "Running a expensive computation"
    Async.Sleep 1000 |> Async.RunSynchronously

    { Bar = "Hello, World!"
      Baz = 42
      Timestamp = timestamp }

// Change this for a base path. If not it will be [Project]\bin\Debug\net6.0.
let spt = SavePointType.File ""

let test1 =
    Dedupe.runInSavePoint<Foo> spt "test_sp" "test_key" testFn

let test2 =
    Dedupe.runInSavePoint<Foo> spt "test_sp" "test_key" testFn

printfn $"Are equal: {test1 = test2}"

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
