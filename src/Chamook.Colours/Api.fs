module Api

open System
open Giraffe
open Microsoft.AspNetCore.Http
open System.Text.RegularExpressions
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Microsoft.FSharp.Reflection

type Colour = {
    Id   : string
    Name : string
    Hex  : string
    RGB  : RGB
    HSL  : HSL
}
and RGB = { Red : int; Green : int; Blue : int }
and HSL = { Hue : int; Saturation : int; Lightness : int }

let tryGetPropertyName<'a> propertyName =
    FSharpType.GetRecordFields typeof<'a>
    |> Array.tryFind
        (fun x -> String.Equals(x.Name, propertyName, StringComparison.OrdinalIgnoreCase))
    |> Option.map (fun x -> x.Name)

type MyColoursDto = { Colours : Colour list }

let myColours = [
    { Id   = "abc123"
      Name = "Red"
      Hex  = "#FF0000"
      RGB  = { Red = 255; Green = 0; Blue = 0 }
      HSL  = { Hue = 0; Saturation = 100; Lightness = 50 } }
    { Id   = "def456"
      Name = "Yellow"
      Hex  = "#FFFF00"
      RGB  = { Red = 255; Green = 255; Blue = 0 }
      HSL  = { Hue = 60; Saturation = 100; Lightness = 50 } }
    ]

let includingRegex = Regex("including=(.*?)(\Z|\0)")

let (|FilteredRepresentation|_|) (accept : string option) =
    match accept with
    | Some a
        when a.StartsWith "application/vnd.chamook.api+json"
            && a.Contains "including=" ->
                includingRegex.Match(a).Groups.[1].Captures.[0].Value.Split ','
                |> Array.map tryGetPropertyName<Colour>
                |> Array.choose id
                |> Array.toList
                |> Some
    | _ -> None

let filterJson (includeFields : string list) (original : JObject) =
    let json = JObject()
    includeFields
    |> List.iter (fun name -> json.[name] <- original.[name])
    json

let getMyColours: HttpHandler =
    fun next ctx ->
        match ctx.TryGetRequestHeader "Accept" with
        | FilteredRepresentation filter ->
            let response = JObject()
            let colourArray =
                myColours
                |> List.map (JObject.FromObject >> filterJson filter)
            response.["colours"] <- JArray(colourArray)
            Successful.OK
                response
                next
                ctx
        | _ ->
            Successful.OK
                { Colours = myColours }
                next
                ctx
