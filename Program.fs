#if BOOTSTRAP
System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
if not (System.IO.File.Exists "paket.exe") then let url = "https://github.com/fsprojects/Paket/releases/download/3.13.3/paket.exe" in use wc = new System.Net.WebClient() in let tmp = System.IO.Path.GetTempFileName() in wc.DownloadFile(url, tmp); System.IO.File.Move(tmp,System.IO.Path.GetFileName url);;
#r "paket.exe"
Paket.Dependencies.Install (System.IO.File.ReadAllText "paket.dependencies")
#endif

#if INTERACTIVE
#I "packages/Suave/lib/net40"
#r "packages/Suave/lib/net40/Suave.dll"
#endif

open System
open Suave              // always open suave
open Suave.Http
open Suave.Filters
open Suave.Successful   // for OK-result
open Suave.Web          // for config
open System.Net
open Suave.Operators 

let random_object = System.Random()

let filelines filename = System.IO.File.ReadLines(__SOURCE_DIRECTORY__ + "/" + filename)

let get_random_line file = random_object.Next(0, Seq.length(file))
let get_random_word word_file = Seq.item (get_random_line word_file) (word_file)

type words =
    | Verb
    | Noun
    | Adjective
    | Word of string

let noun_file = filelines "nounlist.txt"
let verb_file = filelines "31k verbs.txt"
let adjective_file = filelines "commonadjectives.txt"

let replacer = function
    | Verb -> get_random_word verb_file
    | Noun -> get_random_word noun_file
    | Adjective -> get_random_word adjective_file
    | Word(word) -> word

let output_list input = List.map replacer input
let folded_list list = list |> List.fold (+) ""
let get_output_string = output_list >> folded_list

let config = 
    let port = System.Environment.GetEnvironmentVariable("PORT")
    let ip127  = IPAddress.Parse("127.0.0.1")
    let ipZero = IPAddress.Parse("0.0.0.0")

    { defaultConfig with 
        logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Verbose
        bindings=[ (if port = null then HttpBinding.mk HTTP ip127 (uint16 8080)
                    else HttpBinding.mk HTTP ipZero (uint16 port)) ] }

[<EntryPoint>]
let main argv = 

    
    let input_list = [Word("In what sort of world is "); Adjective; Word(" "); Noun; Word(" allowed to be a thing")]

    printfn "%A" (output_list [Noun])
    printfn "%A" (output_list [Noun])
    printfn "%A" (output_list [Noun])
    printfn "%A" (output_list input_list)
    printfn "%A" (output_list input_list)

    let app = GET >=> path "/hello" >=> warbler (fun ctx -> OK (sprintf "%A" (output_list input_list)))

    startWebServer config app

    0