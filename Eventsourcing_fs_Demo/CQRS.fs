namespace Eventsourcing_fs_Demo

module CQRS =

    type UnitOfWork (fixed_history:obj list) =
            
        let mutable staged = List.empty

        member this.stage (event:obj) =
            staged <- staged @ [event]

        member this.events () = 
            fixed_history @ staged

        member this.commit () : obj list =
            let to_publish = staged
            staged <- List.empty
            to_publish



    type CommandHandler = obj -> UnitOfWork -> unit
