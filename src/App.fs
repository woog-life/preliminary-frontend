module App

open System
open Browser
open Elmish
open Elmish.React
open Fable.Import
open Fable.React
open Fable.React.Props
open LakeInfo
open Thoth.Fetch

type Model =
    { Lake: Lake.Type option
      LakeLoadInit: bool
      InitialLoad: bool
      Weather: Weather.Type option
      Lakes: LakeInfo.LakeInfo list }

type Msg =
    | InitialLoad
    | GetLake of string
    | AddLake of Lake.RawType
    | AddEvents of Booking.RawType
    | GetWeather of string option
    | UpdateWeather of Weather.RawType option
    | GetLakes
    | AddLakes of RawType

let PRECISION = 1

let getLake uuid dispatch =
    promise {
        let url =
            sprintf "https://api.woog.life/lake/%s?precision=%d" uuid PRECISION

        let! res = Fetch.get url

        AddLake res |> dispatch
    }

let getLakes dispatch =
    promise {
        let url = "https://api.woog.life/lake"

        let! res = Fetch.get url

        AddLakes res |> dispatch
    }

let getEvents uuid dispatch =
    promise {
        let url =
            sprintf "https://api.woog.life/lake/%s/booking" uuid

        let! res = Fetch.get url

        AddEvents res |> dispatch
    }

let API_KEY =
    "{{API_KEY}}"

let UUID_CITY_MAP =
    Map [ ("acf32f07-e702-4e9e-b766-fb8993a71b21", "Bern")
          ("55e5f52a-2de8-458a-828f-3c043ef458d9", "Hamburg")
          ("69c8438b-5aef-442f-a70d-e0d783ea2b38", "Darmstadt")
          ("d074654c-dedd-46c3-8042-af55c93c910e", "Cuxhaven")
          ("bedbdac7-7d61-48d5-b1bd-0de5be25e953", "Potsdam")
          ("ab337e4e-7673-4b5e-9c95-393f06f548c8", "Köln") ]

let findCity uuid =
    try
        Some(UUID_CITY_MAP |> Map.find uuid)
    with
    | _ -> None

let getWeather city dispatch =
    promise {
        let url =
            sprintf "https://api.openweathermap.org/data/2.5/weather?q=%s&units=metric&appid=%s" city API_KEY

        let! res = Fetch.tryGet url

        let weather =
            match res with
            | Ok weather -> Some weather
            | _ -> None

        UpdateWeather weather |> dispatch
    }

let findCookieValue (name: string) : string option =
    let kvArrToPair (kvArr: string []) : string * string =
        match kvArr with
        | [| k; v |] -> (k, v)
        | _ -> ("", "")

    let rawCookies: string = Dom.document.cookie

    rawCookies.Split ';'
    |> Array.map (fun (s: string) -> s.Trim().Split '=' |> kvArrToPair)
    |> Map.ofArray
    |> Map.tryFind name

let init () : Model * Cmd<Msg> =
    { InitialLoad = true
      Lake = None
      LakeLoadInit = false
      Weather = None
      Lakes = [] },
    Cmd.ofSub (fun dispatch ->
        GetLakes |> dispatch
        dispatch (GetWeather None))

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
            Cmd.Empty
    | GetLake uuid ->
        JsCookie.set "initial-lake-uuid" uuid |> ignore

        { model with
            LakeLoadInit = true
            InitialLoad = false },
        Cmd.ofSub (fun dispatch -> getLake uuid dispatch |> Promise.start)
    | GetWeather uuid ->
        if uuid.IsSome then
            match (findCity uuid.Value) with
            | Some(city) -> model, Cmd.ofSub (fun dispatch -> getWeather city dispatch |> Promise.start)
            | None -> { model with Weather = None }, Cmd.Empty
        else
            { model with Weather = None }, Cmd.Empty
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
                { lake with Events = (List.map Booking.Into events.events) }

            { model with Lake = Some(lake) }, Cmd.ofSub (fun dispatch -> GetWeather(Some(lake.Uuid)) |> dispatch)
    | UpdateWeather weather ->
        { model with
            Weather =
                if weather.IsSome then
                    Some(Weather.Into weather.Value)
                else
                    None },
        Cmd.Empty
    | AddLakes lakes ->
        { model with Lakes = (List.map Into lakes.lakes) },
        if lakes.lakes.Length > 0 then
            let defaultUuid =
                match findCookieValue "initial-lake-uuid" with
                | Some uuid -> uuid
                | None -> "69c8438b-5aef-442f-a70d-e0d783ea2b38"

            let lake =
                match List.filter (fun lake -> lake.id = defaultUuid) lakes.lakes with
                | x :: _ -> x.id
                | [] -> lakes.lakes.Head.id

            (Cmd.ofSub (fun dispatch -> GetLake lake |> dispatch))
        else
            Cmd.Empty
    | GetLakes -> model, Cmd.ofSub (fun dispatch -> getLakes dispatch |> Promise.start)

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
        span [ Id "sunset"; Title "sunset" ] [
            str (formatDateTime weather.Sunset.UtcDateTime sunTimeFormat)
        ]
    ]

let displayEventHeader =
    thead [] [
        tr [] [
            th [] [ str "Badestelle" ]
            th [] [ str "Buchungs-Link" ]
            th [] [ str "Startzeit Slot" ]
            th [] [ str "Endzeit Slot" ]
            th [] [ str "Verkaufsstart" ]
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
            str (formatDateTime event.BeginTime timeFormat)
        ]
        td [] [
            str (formatDateTime event.EndTime timeFormat)
        ]
        td [] [
            str (formatDateTime event.SalesStartTime timeFormat)
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
    div [ ClassName "collapse show"
          Id "events" ] [
        table [ ClassName "table table-dark" ] [
            displayEventHeader
            tbody [] (List.map displayEvent events)
        ]
    ]

let displayTemp model =
    str (
        match model.Lake with
        | Some lake ->
            match lake.Temperature with
            | Some temperature ->
                if temperature.StartsWith("0.") then
                    ""
                else
                    sprintf "%s°" temperature
            | None -> ""
        | None -> ""
    )

let displayLakeChooser (lake: Lake.Type option) (lakes: LakeInfo.LakeInfo list) dispatch =
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
        div
            [ Class "dropdown-menu"
              AriaLabelledby "dropdownLakeChooseButton" ]
            (List.map
                (fun _lake ->
                    button [ ClassName "dropdown-item"
                             Type "button"
                             OnClick(fun _ -> GetLake _lake.Id |> dispatch) ] [
                        str _lake.Name
                    ])
                lakes)
    ]

let displayBookings model (lake: Lake.Type option) =
    match lake with
    | Some lake ->
        let lakeInfo =
            List.find (fun x -> x.Id = lake.Uuid) model.Lakes

        let hideEvents =
            lake.Events.IsEmpty
            || not (List.contains (Booking) lakeInfo.Features)

        if hideEvents then
            span [] []
        else
            div [] [
                displayEventCollapseButton
                displayEvents lake.Events
            ]
    | None -> span [] []

let displayLake model dispatch =
    div [ Id "data"
          ClassName "text-center h-75 text-white"
          Style [ MarginTop "4%"
                  Padding "10px"
                  FontFamily "Chawp" ] ] [
        div [] [
            (displayLakeChooser model.Lake model.Lakes dispatch)
            (match model.Weather with
             | Some weather ->
                 if model.Lake.IsSome then
                     displaySun weather
                 else
                     span [] []
             | None -> span [] [])

            (if model.Lake.IsSome then
                 let lakeInfo =
                     List.find (fun x -> x.Id = model.Lake.Value.Uuid) model.Lakes

                 if List.contains Temperature lakeInfo.Features then
                     span [] [
                         span [ Id "water-temperature-header"
                                Style [ FontSize "2em" ] ] [
                             str "Wasser (°C)"
                         ]
                         br []
                         a [ Href(sprintf "https://sos-de-fra-1.exo.io/wooglife/%s.svg" model.Lake.Value.Uuid)
                             Id "water-history"
                             Style [ FontSize "1.3em" ] ] [
                             str "Historie"
                         ]
                         p [ Id "data-updated-time"
                             Style [ FontSize "2em" ] ] [
                             str (
                                 match model.Lake.Value.Time with
                                 | Some time -> (formatDateTime time timeFormat)
                                 | None -> "No data available"
                             )
                         ]
                         p [ Id "lake-water-temperature"
                             ClassName "ml-4"
                             Style [ FontSize "12em" ] ] [
                             displayTemp model
                         ]
                     ]
                 else
                     span [ Style [ FontSize "2em" ] ] [
                         str "No data available"
                     ]
             else
                 span [ Style [ FontSize "2em" ] ] [
                     str "No data available"
                 ])

            displayBookings model model.Lake
        ]
    ]

let view (model: Model) dispatch =
    div [ ClassName "row d-flex justify-content-center"
          Style [ Width "99%" ] ] [
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
