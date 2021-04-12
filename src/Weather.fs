module Weather

open System

type Sys = {
    sunset: int
    sunrise: int
}

type RawType =
    { sys: Sys }

type Type = {
    Sunrise: DateTimeOffset
    Sunset: DateTimeOffset
}

let Into (raw: RawType) =
    {
        Sunrise = DateTimeOffset.FromUnixTimeSeconds(int64 raw.sys.sunrise).AddHours(2.0)
        Sunset = DateTimeOffset.FromUnixTimeSeconds(int64 raw.sys.sunset).AddHours(2.0)
    }
