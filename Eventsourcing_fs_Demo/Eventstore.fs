namespace Eventsourcing_fs_Demo

open NUnit.Framework
open FsUnit


module Eventstore =

    type Naive_Eventstore() =

        let mutable storage = List.Empty

        /// store a list of objects as events
        member this.do_store (events: obj list) : unit =
            storage <- storage @ events

        member this.retrieve_all () : obj list =
            storage

        /// initialize event store with a list of events
        new (history:obj list) as this =
            Naive_Eventstore()
            then
                this.do_store history






    
    [<TestFixture>]
    type Eventstore_Specs() =

        let create () = new Naive_Eventstore()
        
        let retrieve_all_events (eventstore:Naive_Eventstore) = eventstore.retrieve_all() 

        let store_an_event (eventstore:Naive_Eventstore) (event:obj) = eventstore.do_store [event]

        [<Test>]
        member test.``Initialized eventstore returns no events``() =
            let sut = create()
            retrieve_all_events sut |> should be Empty

        [<Test>]
        member test.``Eventstore returns an event stored previously``() =
            let sut = create()
            store_an_event sut "TestEvent"
            let result = retrieve_all_events sut 
            result.IsEmpty |> should be False
            result |> List.head |> should equal "TestEvent"

