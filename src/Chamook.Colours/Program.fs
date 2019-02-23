module Program

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Giraffe

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

let webApp =
    choose [
        route "/health"   >=> Successful.OK "Everything's fine here, how are you?"
        route "/my-colours" >=> Successful.OK { Colours = myColours } ]

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0
