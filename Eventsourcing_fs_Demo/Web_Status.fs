namespace Eventsourcing_fs_Demo.WebApplication

module Status =

        open Eventsourcing_fs_Demo
        open Layout
        open Common


        let event_view (event:obj) =
            [ event.ToString() ] |> datarow

        let history_view (history) =
            let events = history |> List.rev
            events |> Seq.map event_view |> datatable

        let status_home (context:WebContext) history = history_view history |> page (context.Company+ " - Status") context.Menu ""
