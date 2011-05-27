// Learn more about F# at http://fsharp.net
open System
open System.Net
open System.IO
open Microsoft.FSharp.Control.WebExtensions
#if INTERACTIVE
#load @"AsyncHelpers.fs"
#endif
open FSharp.Tutorial
open System.Collections.Generic
open System.Linq

let myAppId = "B3CFD35A9EB3995659BAC3E9B6456CE8008AD59B"
let translateUri    = "http://api.microsofttranslator.com/V1/Http.svc/Translate?appId=" + myAppId + "&"
let languageUri     = "http://api.microsofttranslator.com/V1/Http.svc/GetLanguages?appId=" + myAppId
let languageNameUri = "http://api.microsofttranslator.com/V1/Http.svc/GetLanguageNames?appId=" + myAppId
let run x = Async.RunSynchronously x

let httpLines (uri:string) =
  async { 
    let request = WebRequest.Create uri 
    use! response = request.AsyncGetResponse()          
    use stream = response.GetResponseStream()
    use reader = new StreamReader(stream)
    let! lines = reader.AsyncReadAllLines() 
    return new List<String>(lines |> List.toArray)
  }

/// key is just a variable for concatinating outputs
let translateText(key, text, fromLang, toLang) =
    async {
        let uri = sprintf "%sfrom=%s&to=%s" translateUri fromLang toLang
        let req2 = WebRequest.Create(uri, Method = "Post", ContentType = "text/plain")
        do! req2.AsyncWriteContent text
        let! resp = req2.AsyncReadResponse()
        return key + resp
    }

let translateFile(filename : string, fromLang : string, toLang : string, outputfile : string) =
    let languages = httpLines languageUri |> run
    printfn "Languages available"
    languages
    |> Seq.iteri(fun i t -> printfn "%d) %s" i t)

    use sr = new StreamReader(filename)
    let lines = sr.ReadToEnd().Split([|Environment.NewLine|], StringSplitOptions.RemoveEmptyEntries)
    printfn "Language Translation started..."
    let task = 
        Async.Parallel
            [for line in lines ->
                let txt = line.Split([|'='|])
                let key = txt.[0] + "="
                translateText(key, txt.[1], fromLang, toLang)]
    Async.StartWithContinuations(
        task,
        (fun results ->            
            let sb = new System.Text.StringBuilder()
            for r in results do
                printfn "%s" r
                sb.AppendLine(r) |> ignore
            let ofile =
                if outputfile = String.Empty then
                    let f = filename.Substring(0, filename.IndexOf("."))
                    f + "." + toLang + ".txt"
                else
                    outputfile
            use sw = new StreamWriter(ofile)
            sw.Write(sb.ToString())
            printfn "Language Translation done..."
        ),
        (fun _ -> ()),
        (fun _ -> ()))

#if INTERACTIVE
printf "From Lang:"
let fLang = Console.ReadLine()
printf "To Lang:"
let tLang = Console.ReadLine()
printf "Enter Resouces File Path:"
let filename = Console.ReadLine()
    
if fLang <> String.Empty && languages.Contains(fLang) && tLang <> String.Empty && languages.Contains(tLang) && filename <> String.Empty then
    translateFile(filename, fLang, tLang)
else
    printfn "Input details in-correct"           
#else
/// /file:sometext.txt /from:en /to:de /o:output.de
[<EntryPoint>]
let main args =
    let usage = "Usage of ResxGenerator should be as below,

    resxgenerator.exe /f:sometext.txt /from:en /to:de /o:output.de.txt

    /f:    - filename
    /from: - from language
    /to:   - to language
    /o:    - output file"    
    if args.Length = 0 then
        printfn "%s" usage
    else
        try
            let parseArg(cmp) =
                let bitString = args.FirstOrDefault(Func<string, bool>(fun s -> s.Contains(cmp)))            
                if bitString <> String.Empty then
                    let b = bitString.Split([|':'|])
                    Some(b.[0], b.[1])
                else
                    None        
            let parseFileArg(cmp) = 
                let bitString = args.FirstOrDefault(Func<string, bool>(fun s -> s.Contains(cmp)))            
                if bitString <> String.Empty then
                    let s1 = bitString
                    Some(s1.Substring(s1.IndexOf("/f:") + 3, s1.Length - (s1.IndexOf("/f:") + 3)))
                else
                    None

            let fileArg = parseFileArg("/f:")
            let filename = 
                match fileArg with
                | Some(b) -> b
                | _ -> String.Empty
        
            let fromArg = parseArg("/from:")
            let fLang = 
                match fromArg with
                | Some(b, b1) -> b1
                | _ -> String.Empty

            let toArg = parseArg("/to:")
            let tLang = 
                match toArg with
                | Some(b, b1) -> b1
                | _ -> String.Empty

            let outputfileArg = parseArg("/o:")
            let outputfile = 
                match outputfileArg with
                | Some(b, b1) -> b1
                | _ -> "output.txt"

            if fLang <> String.Empty && tLang <> String.Empty && filename <> String.Empty then
                translateFile(filename, fLang, tLang, outputfile)
            else
                printfn "%s" usage        
            Console.ReadKey() |> ignore
        with
            | e -> 
                printfn "%s" e.Message
                printfn "%s" usage
    0
#endif