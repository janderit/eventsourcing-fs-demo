namespace Eventsourcing_fs_Demo

open System

module Server =
    
    open Model
    open Layout
    open Model
    open Generated
    open Common
    open Projections
    open Eventsourcing_fs_Demo.WebApplication.Common
    open CommandHandling

    module Initial_data =
       let supplier_1 = Domain.new_Supplier()
       let supplier_2 = Domain.new_Supplier()
       let product_1 = Domain.new_Product()
       let product_2 = Domain.new_Product()
       let po_1 = Domain.new_Purchaseorder()
       let po_2 = Domain.new_Purchaseorder()
    
       let history = [
           Domain.event_Supplier_account_opened supplier_1 "Foodworld Inc."
           Domain.event_Supplier_account_opened supplier_2 "Bestcrops Inc."
           Domain.event_Product_listed product_1 "Cornflakes"
           Domain.event_Product_listed product_2 "Crispies"
           Domain.event_Received_product_availability_annoucement supplier_1 product_1 1000 (Currency 85M) 
           Domain.event_Received_product_availability_annoucement supplier_1 product_2 1000 (Currency 120M) 
           Domain.event_Received_product_availability_annoucement supplier_2 product_1 1000 (Currency 82.50M) 
           Domain.event_Purchase_order_confirmed_by_supplier po_1 supplier_1 product_1 250  (Currency 85M)
           Domain.event_Purchase_order_confirmed_by_supplier po_2 supplier_1 product_2 250  (Currency 120M)
           Domain.event_Goods_received_in_depot product_2 250 po_2 supplier_1
           Domain.event_Supplier_fulfilled_purchase_order supplier_1 po_2
       ]
    
    let store = new Eventstore.Naive_Eventstore(Initial_data.history)

    let handle command =
        let uow = new CQRS.UnitOfWork (store.retrieve_all())
        CommandHandling.handle command uow
        uow.commit () |> store.do_store

    let checked_handle on_success command = 
        try 
            handle command
            "Request handled successfully. " + (link on_success "continue...")
        with
            | Failure msg -> ("Error while executing request: "+msg)

    let allevents () = store.retrieve_all()

    let readmodels = new QueryHandling.Readmodels(store)

    let webcontext = new Eventsourcing_fs_Demo.WebApplication.Common.WebContext ("CQRS Inc.",
        [
            { MenuItem.Key=""; Title="Home + log" }
            { MenuItem.Key="supply"; Title="Suppliers + products" }
            { MenuItem.Key="logistics"; Title="Depot management" }
        ], readmodels)
            


    open Eventsourcing_fs_Demo.WebApplication
    open Suave
    open Suave.Web
    open Suave.Http
    open Suave.Http.Applicatives
    open Suave.Http.Writers
    open Suave.Http.Files
    open Suave.Http.Successful
    open Suave.Types
    open Suave.Session
    open Suave.Log

    let unpack (data: (string*string option)list) : (string*string)list=
        data |> List.filter (snd >> Option.isSome) |> List.map (fun (k, Some v) -> (k,v))

    let routes =
        choose 
            [ GET >>= choose
                [ url "/" >>= request(fun ctx -> OK <| Status.status_home webcontext (store.retrieve_all()))
                  url "/supply" >>= OK (Suppliers.suppliers_home webcontext)
                  url_scan "/product/%s" (fun id -> OK (Suppliers.details webcontext (Domain.Product (Guid.Parse id))))
                  url "/logistics" >>= request (fun ctx -> OK (Logistics.logistics_home webcontext))
                  Redirection.FOUND "/"
                ]
              POST >>= choose
                [
                    url "/supply/purchase_order" >>= request(fun x -> OK (Suppliers.purchase_form_handler (unpack (form x)) (checked_handle "/supply")))
                    url "/supply/evaluate_order" >>= request(fun x -> OK (Suppliers.order_form_handler (unpack (form x)) (checked_handle "/supply")))
                    url "/logistics/goods_received" >>= request(fun x -> OK (Logistics.goods_received_form_handler (unpack (form x)) (checked_handle "/logistics")))
                    url "/logistics/dispatch_goods" >>= request(fun x -> OK (Logistics.dispatch_goods_form_handler (unpack (form x)) (checked_handle "/logistics")))
                    RequestErrors.NOT_FOUND "Oops, no POST handler here..."
                ]
            ]

    [<EntryPoint>]
    let main args =
        web_server default_config routes
        0
