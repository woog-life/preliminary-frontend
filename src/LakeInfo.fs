module LakeInfo

open System

type RawTypeInfo = { id: string; name: string }

type RawType = { lakes: RawTypeInfo list }

[<CustomEquality; NoComparison>]
type LakeInfo =
    { Id: string
      Name: string }
    override this.Equals(other) =
        match other with
        | :? LakeInfo as other -> this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = hash this.Id

type Type = { lakes: LakeInfo list }

let Into raw = { Id = raw.id; Name = raw.name }
