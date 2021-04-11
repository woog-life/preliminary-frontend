module Lake

open System

type RawTypeData =
    { time: string
      temperature: int
      preciseTemperature: string }

type RawType =
    { id: string
      name: string
      data: RawTypeData }

[<CustomEquality; NoComparison>]
type Type =
    { Uuid: string
      Name: string
      Time: DateTime
      Temperature: double }
    override this.Equals(other) =
        match other with
        | :? Type as other ->
            this.Uuid = other.Uuid
        | _ -> false

    override this.GetHashCode() = hash this.Uuid

let Into raw =
        {
            Uuid = raw.id
            Name = raw.name
            Temperature = double (raw.data.preciseTemperature)
            Time = DateTime.Parse(raw.data.time)
        }
