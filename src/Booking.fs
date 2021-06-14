module Booking

open System

type RawTypeEvent =
    { variation: string
      bookingLink: string
      beginTime: string
      endTime: string
      saleStartTime: string }

type RawType = { events: RawTypeEvent list }

[<CustomEquality; NoComparison>]
type Type =
    { Variation: string
      BookingLink: string
      BeginTime: DateTime
      EndTime: DateTime
      SalesStartTime: DateTime }
    override this.Equals(other) =
        match other with
        | :? Type as other -> this.BookingLink = other.BookingLink
        | _ -> false

    override this.GetHashCode() = hash this.BookingLink

let Into (raw: RawTypeEvent) =
    { Variation = raw.variation
      BookingLink = raw.bookingLink
      BeginTime = DateTime.Parse(raw.beginTime)
      EndTime = DateTime.Parse(raw.endTime)
      SalesStartTime = DateTime.Parse(raw.saleStartTime)
    }
