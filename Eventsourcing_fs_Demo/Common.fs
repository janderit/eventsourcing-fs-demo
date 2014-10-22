namespace Eventsourcing_fs_Demo

module Common =

    type Currency = Currency of System.Decimal with
        override this.ToString () = match this with | (Currency amount) -> sprintf "%.2f EUR" amount
        member this.EUR = this.ToString()

