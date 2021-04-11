module App

open System
open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Thoth.Fetch

type Model =
    { Lake: Lake.Type option
      LakeLoadInit: bool
      InitialLoad: bool }

type Msg =
    | InitialLoad
    | GetLake
    | AddLake of Lake.RawType

let getLake uuid model dispatch =
    promise {
        let url =
            sprintf "https://api.woog.life/lake/%s" uuid

        let! res = Fetch.get (url)
        if (not model.LakeLoadInit) then
            AddLake res |> dispatch
    }

let x = true
let init () : Model * Cmd<Msg> =
    { InitialLoad = true; Lake = None; LakeLoadInit = false }, Cmd.ofSub (fun dispatch -> dispatch GetLake)

let UUID = "69c8438b-5aef-442f-a70d-e0d783ea2b38"

let unwrapMapOrDefault (opt: 'b option) (m: 'b -> 't) (def: 't) = if opt.IsSome then m opt.Value else def

let update (msg: Msg) (model: Model) =
    match msg with
    | InitialLoad ->
        if (model.Lake.IsSome || model.LakeLoadInit || not model.InitialLoad) then
            model, Cmd.Empty
        else
            { model with InitialLoad = false; LakeLoadInit = true }, if model.LakeLoadInit then Cmd.Empty else Cmd.ofSub (fun dispatch -> dispatch GetLake)
    | GetLake ->
        if (model.Lake.IsSome || model.LakeLoadInit) then
            model, Cmd.Empty
        else
            { model with LakeLoadInit = true; InitialLoad = false }, Cmd.ofSub (fun dispatch -> getLake UUID model dispatch |> Promise.start)
    | AddLake lake ->
        if (model.Lake.IsSome) then
            model, Cmd.Empty
        else
            { model with Lake = Some(Lake.Into lake); InitialLoad = false; LakeLoadInit = true }, Cmd.Empty

let displayTemp model =
    str (
        match model.Lake with
        | Some (lake) -> sprintf "%.2f°" lake.Temperature
        | None -> "0"
    )

let displayLake model =
    div [ ClassName "text-center h-75 text-white"
          Style [ MarginTop "10%"
                  Padding "10px"
                  FontFamily "Chawp" ] ] [
        div [] [
            p [ Style [ FontSize "2em" ] ] [
                str (
                    match model.Lake with
                    | Some (lake) -> lake.Name
                    | None -> "Kein See verfügbar"
                )
            ]
            p [ Style [ FontSize "2em" ] ] [
                str "Wassertemperatur (°C)"
            ]
            p [ Style [ FontSize "2em" ] ] [
                str (
                    match model.Lake with
                    | Some (lake) -> (lake.Time.Split 'T') |> String.concat " "
                    | None -> ""
                )
            ]
            p [ Id "temperature"
                Style [ FontSize "12em" ] ] [
                displayTemp model
            ]
        ]
    ]

let view (model: Model) dispatch =
    div [ (*Ref
              (fun element ->
                  if not (isNull element) then
                      if model.InitialLoad && model.Lake.IsNone && not model.LakeLoadInit then
                          dispatch InitialLoad)*)
          ClassName "row d-flex justify-content-center" ] [
        (displayLake model)
    ]

// App
Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
//|> Program.withConsoleTrace
|> Program.run
