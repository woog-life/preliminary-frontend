module Lake

open System

type RawTypeData =
    { time: string
      temperature: int
      preciseTemperature: string }

type RawType =
    { id: string
      name: string
      data: RawTypeData option }

[<CustomEquality; NoComparison>]
type Type =
    { Uuid: string
      Name: string
      Time: DateTime option
      Temperature: string option
      Events: Booking.Type list }
    override this.Equals(other) =
        match other with
        | :? Type as other -> this.Uuid = other.Uuid
        | _ -> false

    override this.GetHashCode() = hash this.Uuid

let Into raw =
    { Uuid = raw.id
      Name = raw.name
      Temperature =
          if raw.data.IsSome then
              Some raw.data.Value.preciseTemperature
          else
              None
      Time =
          if raw.data.IsSome then
              Some(DateTime.Parse(raw.data.Value.time))
          else
              None
      Events = [] }
