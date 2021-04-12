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
      InitialLoad: bool
      Weather: Weather.Type option }

type Msg =
    | InitialLoad
    | GetLake
    | AddLake of Lake.RawType
    | GetWeather
    | UpdateWeather of Weather.RawType

let getLake uuid model dispatch =
    promise {
        let url =
            sprintf "https://api.woog.life/lake/%s" uuid

        let! res = Fetch.get url

        if (not model.LakeLoadInit) then
            AddLake res |> dispatch
    }

let API_KEY = "{{API_KEY}}"

let getWeather dispatch =
    promise {
        let url =
            sprintf "https://api.openweathermap.org/data/2.5/weather?q=Darmstadt&appid=%s" API_KEY

        let! res = Fetch.get url
        UpdateWeather res |> dispatch
    }

let init () : Model * Cmd<Msg> =
    { InitialLoad = true
      Lake = None
      LakeLoadInit = false
      Weather = None },
    Cmd.ofSub (fun dispatch -> dispatch GetLake
                               dispatch GetWeather)

let UUID = "69c8438b-5aef-442f-a70d-e0d783ea2b38"

let unwrapMapOrDefault (opt: 'b option) (m: 'b -> 't) (def: 't) = if opt.IsSome then m opt.Value else def

let update (msg: Msg) (model: Model) =
    match msg with
    | InitialLoad ->
        if (model.Lake.IsSome
            || model.LakeLoadInit
            || not model.InitialLoad) then
            model, Cmd.Empty
        else
            { model with
                  InitialLoad = false
                  LakeLoadInit = true },
            if model.LakeLoadInit then
                Cmd.Empty
            else
                Cmd.ofSub (fun dispatch -> dispatch GetLake
                                           dispatch GetWeather)
    | GetLake ->
        if (model.Lake.IsSome || model.LakeLoadInit) then
            model, Cmd.Empty
        else
            { model with
                  LakeLoadInit = true
                  InitialLoad = false },
            Cmd.ofSub (fun dispatch -> getLake UUID model dispatch |> Promise.start)
    | GetWeather ->
        model, Cmd.ofSub (fun dispatch -> getWeather dispatch |> Promise.start)
    | AddLake lake ->
        if model.Lake.IsSome then
            model, Cmd.Empty
        else
            { model with
                  Lake = Some(Lake.Into lake)
                  InitialLoad = false
                  LakeLoadInit = true },
            Cmd.Empty
    | UpdateWeather weather ->
        { model with Weather = Some (Weather.Into weather) }, Cmd.Empty

let timeFormat = "HH:mm dd.MM.yyyy"
let sunTimeFormat = "HH:mm"
let formatDateTime (time: DateTime) (format: string) = time.ToString(format)

let displaySun (weather: Weather.Type) =
    div [ ClassName "mb-4"
          Style [ FontSize "2em" ]
    ] [
        p [  ] [ str "Sonne" ]
        str (formatDateTime weather.Sunrise.UtcDateTime sunTimeFormat)
        str " - "
        str (formatDateTime weather.Sunset.UtcDateTime sunTimeFormat)
    ]

let displayTemp model =
    str (
        match model.Lake with
        | Some lake -> sprintf "%.1f°" lake.Temperature
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
                    | Some lake -> lake.Name
                    | None -> "Kein See verfügbar"
                )
            ]
            (match model.Weather with
             | Some weather -> displaySun weather
             | None -> span [] [])
            p [ Style [ FontSize "2em" ] ] [
                str "Wasser (°C)"
            ]
            p [ Style [ FontSize "2em" ] ] [
                str (
                    match model.Lake with
                    | Some lake -> formatDateTime lake.Time timeFormat
                    | None -> ""
                )
            ]
            p [ Id "temperature"
                Style [ FontSize "12em" ] ] [
                displayTemp model
            ]
        ]
    ]

let view (model: Model) _ =
    div [ ClassName "row d-flex justify-content-center" ] [
        span [ ClassName "d-none" ] [
            str "{{TAG}}"
        ]
        (displayLake model)
    ]

// App
Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
//|> Program.withConsoleTrace
|> Program.run
