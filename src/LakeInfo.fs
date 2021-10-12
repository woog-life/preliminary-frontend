module LakeInfo

type Feature =
    | Temperature
    | Booking

let stof (l: string list) =
    let r =
        if (List.contains "temperature" l) then
            [ Temperature ]
        else
            []

    let r =
        r
        @ (if (List.contains "booking" l) then
               [ Booking ]
           else
               [])

    r

type RawTypeInfo =
    { id: string
      name: string
      features: string list }

type RawType = { lakes: RawTypeInfo list }

[<CustomEquality; NoComparison>]
type LakeInfo =
    { Id: string
      Name: string
      Features: Feature list }
    override this.Equals(other) =
        match other with
        | :? LakeInfo as other -> this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = hash this.Id

type Type = { lakes: LakeInfo list }

let Into raw =
    { Id = raw.id
      Name = raw.name
      Features = stof raw.features }
