module Api

open System
open Giraffe
open Microsoft.AspNetCore.Http

type Colour = {
    Id   : string
    Name : string
    Hex  : string
    RGB  : RGB
    HSL  : HSL
}
and RGB = { Red : int; Green : int; Blue : int }
and HSL = { Hue : int; Saturation : int; Lightness : int }

type MyColoursDto = { Colours : Colour list }

type MiniColour = {
    Id   : string
    Name : string
    Hex  : string
}
type MiniMyColoursDto = { Colours : MiniColour list }

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

let toMiniColour (c : Colour) = { MiniColour.Id = c.Id; Name = c.Name; Hex = c.Hex }

let getMyColours: HttpHandler =
    fun next ctx ->
        match ctx.TryGetRequestHeader "Accept" with
        | Some x when x.Contains "application/vnd.chamook.mini-colours+json" ->
            Successful.OK
                { MiniMyColoursDto.Colours = myColours |> List.map toMiniColour }
                next
                ctx
        | _ ->
            Successful.OK
                { MyColoursDto.Colours = myColours }
                next
                ctx

