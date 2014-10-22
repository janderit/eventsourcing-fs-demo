namespace Eventsourcing_fs_Demo.WebApplication

module Orders =

        open Eventsourcing_fs_Demo
        open Common
        open Layout
        
        let orders_home (context:WebContext) = "N/I" |> page (context.Company+ " - Orderbook") context.Menu "orders"