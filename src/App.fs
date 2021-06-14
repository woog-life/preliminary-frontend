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
    | GetLake of string
    | AddLake of Lake.RawType
    | AddEvents of Booking.RawType
    | GetWeather
    | UpdateWeather of Weather.RawType

let PRECISION = 1

let getLake uuid model dispatch =
    promise {
        let url =
            sprintf "https://api.woog.life/lake/%s?precision=%d" uuid PRECISION

        let! res = Fetch.get url

        AddLake res |> dispatch
    }

let UUID = "69c8438b-5aef-442f-a70d-e0d783ea2b38"
let MUEHLCHEN_UUID = "25aa2968-e34e-4f86-87cc-56b16b5aff36"

let getEvents uuid dispatch =
    promise {
        let url =
            sprintf "https://api.woog.life/lake/%s/booking" uuid

        let! res = Fetch.get url

        AddEvents res |> dispatch
    }

let API_KEY = "{{API_KEY}}"

let getWeather dispatch =
    promise {
        let url =
            sprintf "https://api.openweathermap.org/data/2.5/weather?q=Darmstadt&units=metric&appid=%s" API_KEY

        let! res = Fetch.get url
        UpdateWeather res |> dispatch
    }

let init () : Model * Cmd<Msg> =
    { InitialLoad = true
      Lake = None
      LakeLoadInit = false
      Weather = None },
    Cmd.ofSub
        (fun dispatch ->
            GetLake UUID |> dispatch
            dispatch GetWeather)

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
                Cmd.ofSub
                    (fun dispatch ->
                        GetLake UUID |> dispatch
                        dispatch GetWeather)
    | GetLake uuid ->
        { model with
              LakeLoadInit = true
              InitialLoad = false },
        Cmd.ofSub (fun dispatch -> getLake uuid model dispatch |> Promise.start)
    | GetWeather -> model, Cmd.ofSub (fun dispatch -> getWeather dispatch |> Promise.start)
    | AddLake lake ->
        { model with
              Lake = Some(Lake.Into lake)
              InitialLoad = false
              LakeLoadInit = true },
        Cmd.ofSub (fun dispatch -> getEvents lake.id dispatch |> Promise.start)
    | AddEvents events ->
        if model.Lake.IsNone then
            model, Cmd.Empty
        else
            let lake = model.Lake.Value

            let lake =
                { lake with
                      Events = (List.map Booking.Into events.events) }

            { model with Lake = Some(lake) }, Cmd.Empty
    | UpdateWeather weather ->
        { model with
              Weather = Some(Weather.Into weather) },
        Cmd.Empty

let timeFormat = "HH:mm dd.MM.yyyy"
let sunTimeFormat = "HH:mm"
let formatDateTime (time: DateTime) (format: string) = time.ToString(format)

let displaySun (weather: Weather.Type) =
    div [ ClassName "mb-4"
          Style [ FontSize "2em" ] ] [
        p [ Id "sun-information-header" ] [
            str "Sonne"
        ]
        span [ Id "sunrise"; Title "sunrise" ] [
            str (formatDateTime weather.Sunrise.UtcDateTime sunTimeFormat)
        ]
        str " - "
        span [ Id "sunrise"; Title "sunset" ] [
            str (formatDateTime weather.Sunset.UtcDateTime sunTimeFormat)
        ]
    ]

let displayEventHeader =
    thead [] [
        tr [] [
            th [] [ str "Badestelle" ]
            th [] [ str "Buchungs-Link" ]
            th [] [ str "Verkaufsstart" ]
            th [] [ str "Startzeit Slot" ]
            th [] [ str "Endzeit Slot" ]
        ]
    ]

let displayEvent (event: Booking.Type) =
    tr [] [
        td [] [ str event.Variation ]
        td [] [
            a [ Href event.BookingLink
                Target "_blank"
                ClassName "text-white" ] [
                str "Hier buchen"
            ]
        ]
        td [] [
            str (formatDateTime event.SalesStartTime timeFormat)
        ]
        td [] [
            str (formatDateTime event.BeginTime timeFormat)
        ]
        td [] [
            str (formatDateTime event.EndTime timeFormat)
        ]
    ]

type HtmlAttr =
    | [<CompiledName("aria-valuenow")>] AriaValueNow of string
    | [<CompiledName("aria-valuemin")>] AriaValueMin of string
    | [<CompiledName("aria-valuemax")>] AriaValueMax of string
    | [<CompiledName("data-toggle")>] DataToggle of string
    | [<CompiledName("data-target")>] DataTarget of string
    | [<CompiledName("data-dismiss")>] DataDismiss of string
    | [<CompiledName("type")>] InputType of string
    | [<CompiledName("for")>] For of string
    | [<CompiledName("aria-labelledby")>] AriaLabelledby of string
    interface IHTMLProp

let displayEventCollapseButton =
    button [ ClassName "btn btn-primary mb-2"
             Type "button"
             DataToggle "collapse"
             DataTarget "#events"
             AriaExpanded true
             AriaControls "events" ] [
        str "Verfügbare Buchungsslots"
    ]

let displayEvents (events: Booking.Type list) =
    div [ ClassName "collapse show"; Id "events" ] [
        table [ ClassName "table table-dark" ] [
            displayEventHeader
            tbody [] (List.map displayEvent events)
        ]
    ]

let displayTemp model =
    str (
        match model.Lake with
        | Some lake -> if lake.Temperature.StartsWith("0.") then "/" else sprintf "%s°" lake.Temperature
        | None -> "/"
    )

let displayLakeChooser (lake: Lake.Type option) dispatch =
    div [ ClassName "dropdown mb-4" ] [
        button [ ClassName "btn btn-secondary dropdown-toggle"
                 Type "button"
                 Id "dropdownLakeChooseButton"
                 DataToggle "dropdown"
                 AriaHasPopup true
                 AriaExpanded false ] [
            str (
                if lake.IsSome then
                    lake.Value.Name
                else
                    "Choose"
            )
        ]
        div [ Class "dropdown-menu"
              AriaLabelledby "dropdownLakeChooseButton" ] [
            button [ ClassName "dropdown-item"
                     Type "button"
                     OnClick(fun _ -> GetLake UUID |> dispatch) ] [
                str "Großer Woog"
            ]
            button [ ClassName "dropdown-item"
                     Type "button"
                     OnClick(fun _ -> GetLake MUEHLCHEN_UUID |> dispatch) ] [
                str "Arheilger Mühlchen"
            ]
        ]
    ]


let displayLake model dispatch =
    div [ Id "data"
          ClassName "text-center h-75 text-white"
          Style [ MarginTop "4%"
                  Padding "10px"
                  FontFamily "Chawp" ] ] [
        div [] [
            (displayLakeChooser model.Lake dispatch)
            (match model.Weather with
             | Some weather -> displaySun weather
             | None -> span [] [])
            p [ Id "water-temperature-header"
                Style [ FontSize "2em" ] ] [
                str "Wasser (°C)"
            ]
            (match model.Lake with
             | Some lake ->
                 p [ Id "data-updated-time"
                     Style [ FontSize "2em" ] ] [
                     str (formatDateTime lake.Time timeFormat)
                 ]
             | None -> span [] [])
            p [ Id "lake-water-temperature"
                ClassName "ml-4"
                Style [ FontSize "12em" ] ] [
                displayTemp model
            ]
            (match model.Lake with
             | Some lake ->
                 div [] [
                     displayEventCollapseButton
                     displayEvents lake.Events
                 ]
             | None -> span [] [])
        ]
    ]

let view (model: Model) dispatch =
    div [ ClassName "row d-flex justify-content-center" ] [
        span [ Id "commit-sha"; ClassName "d-none" ] [
            str "{{TAG}}"
        ]
        (displayLake model dispatch)
    ]

// App
Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
|> Program.withConsoleTrace
|> Program.run
