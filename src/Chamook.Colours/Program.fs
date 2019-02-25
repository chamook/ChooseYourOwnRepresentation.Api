module Program

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Giraffe
open Giraffe.Serialization

let customJsonContentType mimeType data next (ctx : HttpContext) =
    sprintf "%s; charset=utf-8" mimeType |> ctx.SetContentType
    let serializer = ctx.GetJsonSerializer()
    serializer.SerializeToBytes data
    |> ctx.WriteBytesAsync

type CustomNegotiationConfig (baseConfig : INegotiationConfig) =
    interface INegotiationConfig with
        member __.UnacceptableHandler =
            baseConfig.UnacceptableHandler
        member __.Rules =
            dict [
                "*/*" , json
                "application/json" , json
                "application/vnd.chamook.mini-colours+json",
                customJsonContentType "application/vnd.chamook.mini-colours+json"
                ]

let webApp =
    choose [
        route "/health"   >=> Successful.OK "Everything's fine here, how are you?"
        route "/my-colours" >=> Api.getMyColours ]

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

    CustomNegotiationConfig(DefaultNegotiationConfig())
    |> services.AddSingleton<INegotiationConfig>
    |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0
