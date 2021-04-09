module Lake

type RawTypeData =
    { time: string
      temperature: int }

type RawType =
    { id: string
      name: string
      data: RawTypeData }

[<CustomEquality; NoComparison>]
type Type =
    { Uuid: string
      Name: string
      Time: string
      Temperature: int }
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
            Temperature = raw.data.temperature
            Time = raw.data.time
        }
