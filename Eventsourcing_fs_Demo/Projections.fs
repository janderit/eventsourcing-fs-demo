namespace Eventsourcing_fs_Demo

open System
open NUnit.Framework
open FsUnit


module Model =

    module Projections =
        
        open Common
        open Eventsourcing
        open Generated
        open Domain

        // helper functions for modifying internal state of projections

        let inline modify_by_id id modifier set =
            let old = Map.find id set
            Map.add id (modifier old) <| Map.remove id set

        let inline exchange_by_id id new_value set =
            Map.add id new_value <| Map.remove id set

        let inline remove_where predicate = 
            Map.filter (fun k v -> not (predicate k v))
            
        let inline select_all_events event = Some ()

        let inline replace_with newvalue _ = newvalue

        let inline modify_with modifier state = modifier state

        let inline add_or_modify id modifier default_initialize_with map =
            let old = defaultArg (Map.tryFind id map) default_initialize_with
            (Map.add id (modifier old) << Map.remove id) map




        // helper functions to define composite projections

        let combine_or (a:'Id option) (b:'Id option) = 
            match (a,b) with
            | (None,None) -> None
            | (Some x,Some y) when x=y -> Some x
            | (Some x,None) -> Some x
            | (None,Some y) -> Some y
            | _ -> failwith "Incompatible key selectors"


        let inline combine2 (p1,p2) combinator = 
                {
                    Projection.Init = (p1.Init,p2.Init)
                    Projection.Selector = (fun event -> combine_or (p1.Selector event) (p2.Selector event))
                    Projection.Step = (fun event (s1,s2) -> (p1.Step event s1, p2.Step event s2))
                    Projection.Final = (fun (s1,s2) -> combinator (p1.Final s1, p2.Final s2))
                }
        
        let inline combine3 (p1,p2,p3) combinator = 
                {
                    Projection.Init = (p1.Init,p2.Init,p3.Init)
                    Projection.Selector = (fun event -> combine_or (combine_or (p1.Selector event) (p2.Selector event)) (p3.Selector event))
                    Projection.Step = (fun event (s1,s2,s3) -> (p1.Step event s1, p2.Step event s2, p3.Step event s3))
                    Projection.Final = (fun (s1,s2,s3) -> combinator (p1.Final s1, p2.Final s2, p3.Final s3))
                }




        /// Projection: list of named business entities
        let Entity_list =
            {
                Projection.Init = Map.empty
                Projection.Selector = select_all_events
                Projection.Step = (fun event -> 
                    match event with
                    | :? ``Supplier account opened`` as e ->           Map.add e.``supplier id``.Id e.designation
                    | :? ``Supplier account closed`` as e ->           modify_by_id e.``supplier id``.Id ((+) " (closed)")
                    | :? ``Supplier designation changed`` as e ->      exchange_by_id e.``supplier id``.Id e.designation

                    | :? ``Product listed`` as e ->                    Map.add e.``product id``.Id e.designation
                    | :? ``Product deprecated`` as e ->                modify_by_id e.``product id``.Id ((+) " (delisted)")
                    | :? ``Product designation changed`` as e ->       exchange_by_id e.``product id``.Id e.designation
                    | _ -> no_change) 
                Projection.Final = no_change 
            }




        let Supplier_list = 
            {
                Projection.Init = Map.empty

                Projection.Selector = select_all_events

                Projection.Step = (fun event -> 
                    match event with
                    | :? ``Supplier account opened`` as e ->           Map.add e.``supplier id`` e.designation
                    | :? ``Supplier designation changed`` as e ->      exchange_by_id e.``supplier id`` e.designation
                    | :? ``Supplier account closed`` as e ->           Map.remove e.``supplier id``
                    | _ -> no_change) 
                Projection.Final = Map.toList
            }

        let Product_list = 
            {
                Projection.Init = Map.empty

                Projection.Selector = select_all_events

                Projection.Step = (fun event -> 
                    match event with
                    | :? ``Product listed`` as e ->                    Map.add e.``product id`` e.designation
                    | :? ``Product designation changed`` as e ->       exchange_by_id e.``product id`` e.designation
                    | :? ``Product deprecated`` as e ->                Map.remove e.``product id``
                    | _ -> no_change) 

                Projection.Final = Map.toList
            }

        
        let Product_items_in_stock = 
            {
                Projection.Init = 0

                Projection.Selector = (fun event ->
                    match event with
                    | :? ``Goods received in depot`` as e -> Some e.``product id``
                    | :? ``Goods dispatched to customer`` as e -> Some e.``product id``
                    | _ -> None)
                    

                Projection.Step = (fun event -> 
                    
                    match event with

                    | :? ``Goods received in depot`` as e -> (+) e.quantity
                    | :? ``Goods dispatched to customer`` as e -> (+) (-e.quantity)
                    | _ -> no_change) 

                Projection.Final = no_change
            }



        type Purchase_order_state = |Unknown=0|Submitted=1|Confirmed=2|Rejected=3|Closed=4

        let Purchase_order_states = 
            {
                Projection.Init = Purchase_order_state.Unknown

                Projection.Selector = (fun event ->
                    match event with
                    | :? ``Purchase order submitted to supplier`` as e -> Some e.``purchaseorder id``
                    | :? ``Purchase order rejected by supplier`` as e -> Some e.``purchaseorder id``
                    | :? ``Purchase order confirmed by supplier`` as e -> Some e.``purchaseorder id``
                    | :? ``Supplier fulfilled purchase order`` as e -> Some e.``purchaseorder id``
                    | _ -> None)

                Projection.Step = (fun event -> 
                    match event with

                    | :? ``Purchase order submitted to supplier`` as e -> replace_with Purchase_order_state.Submitted
                    | :? ``Purchase order rejected by supplier`` as e -> replace_with Purchase_order_state.Rejected
                    | :? ``Purchase order confirmed by supplier`` as e -> replace_with Purchase_order_state.Confirmed
                    | :? ``Supplier fulfilled purchase order`` as e -> replace_with Purchase_order_state.Closed
                    | _ -> no_change) 

                Projection.Final = no_change
            }


        type Purchase_order_info (id,supplier,product,quantity) =
            member this.Id = id
            member this.Supplier = supplier
            member this.Product = product
            member this.Quantity = quantity

            new (e:``Purchase order confirmed by supplier``) =
                Purchase_order_info (e.``purchaseorder id``,e.``supplier id``,e.``product id``,e.``quantity shipped``)


        type Order_info (id,order_number,product,quantity,shipping_address) =
            member this.Id:Order = id
            member this.Ordernumber = order_number
            member this.Product:Product = product
            member this.Quantity:int = quantity
            member this.ShippingAddress:string = shipping_address

            new (e: ``Customer order accepted``) =
                Order_info(e.``order id``, e.``order number``, e.``product id``, e.quantity, e.``shipping address``)
                
                
        let Open_purchase_orders = 
            {
                Projection.Init = Map.empty

                Projection.Selector = select_all_events

                Projection.Step = (fun event -> 
                    match event with
                    | :? ``Purchase order confirmed by supplier`` as e -> Map.add e.``purchaseorder id`` (new Purchase_order_info(e))
                    | :? ``Supplier fulfilled purchase order`` as e -> Map.remove e.``purchaseorder id``
                    | _ -> no_change) 

                Projection.Final = Seq.map (fun v -> v.Value)
            }


        let Open_orders = 
            {
                Projection.Init = Map.empty

                Projection.Selector = select_all_events

                Projection.Step = (fun event -> 
                    match event with
                    | :? ``Customer order accepted`` as e -> Map.add e.``order id`` (new Order_info(e))
                    | :? ``Customer order fulfilled`` as e -> Map.remove e.``order id``
                    | _ -> no_change) 

                Projection.Final = Seq.map (fun v -> v.Value)
            }



        type Order_state = |Unknown=0|Accepted=1|Closed=2

        let Order_states = 
            {
                Projection.Init = Order_state.Unknown

                Projection.Selector = (fun event ->
                    match event with
                    | :? ``Customer order accepted`` as e -> Some e.``order id``
                    | :? ``Customer order fulfilled`` as e -> Some e.``order id``
                    | _ -> None)

                Projection.Step = (fun event -> 
                    match event with

                    | :? ``Customer order accepted`` as e -> replace_with Order_state.Accepted
                    | :? ``Customer order fulfilled`` as e -> replace_with Order_state.Closed
                    | _ -> no_change) 

                Projection.Final = no_change
            }



        let Remaining_quantity_in_order = 
            {
                Projection.Init = 0

                Projection.Selector = (fun event ->
                    match event with
                    | :? ``Customer order accepted`` as e -> Some e.``order id``
                    | :? ``Goods dispatched to customer`` as e -> Some e.``order id``
                    | _ -> None)

                Projection.Step = (fun event -> 
                    match event with

                    | :? ``Customer order accepted`` as e -> replace_with e.quantity
                    | :? ``Goods dispatched to customer`` as e -> modify_with (+) -e.quantity
                    | _ -> no_change) 

                Projection.Final = no_change
            }




        let Product_items_in_purchase = 
            {
                Projection.Init = Map.empty

                Projection.Selector = (fun event ->
                    match event with
                    | :? ``Purchase order confirmed by supplier`` as e -> Some e.``product id``
                    | :? ``Goods received in depot`` as e -> Some e.``product id``
                    | _ -> None)

                Projection.Step = (fun event -> 
                    match event with
                    | :? ``Purchase order confirmed by supplier`` as e -> add_or_modify e.``purchaseorder id`` ((+) e.``quantity shipped``) 0
                    | :? ``Goods received in depot`` as e -> add_or_modify e.``purchaseorder id`` ((+) -e.quantity) 0
                    | _ -> no_change) 

                Projection.Final = Seq.sumBy (fun v -> v.Value)
            }



        let Product_items_allocated_to_orders = 
            {
                Projection.Init = Map.empty

                Projection.Selector = (fun event ->
                    match event with
                    | :? ``Customer order accepted`` as e -> Some e.``product id``
                    | :? ``Goods dispatched to customer`` as e -> Some e.``product id``
                    | _ -> None)

                Projection.Step = (fun event -> 
                    match event with
                    | :? ``Customer order accepted`` as e -> add_or_modify e.``order id`` ((+) e.quantity) 0
                    | :? ``Goods dispatched to customer`` as e -> add_or_modify e.``order id`` ((+) -e.quantity) 0
                    | _ -> no_change) 

                Projection.Final = Seq.sumBy (fun v -> v.Value)
            }



        let Next_order_number = 
            {
                Projection.Init = 10000
                
                Projection.Selector = (fun event ->
                    match event with
                    | :? ``Order number allocated`` -> Some ()
                    | _ -> None)

                Projection.Step = (fun event ->
                    match event with
                    | :? ``Order number allocated`` as e -> replace_with e.``order number``
                    | _ -> no_change)

                Projection.Final = (+) 1
            }
            
        
        // example: combined projection
        let Product_items_available = 
            combine3 (Product_items_in_stock, 
                      Product_items_in_purchase, 
                      Product_items_allocated_to_orders) 
                      
                      (fun (instock,inpurchase,allocated) -> instock+inpurchase-allocated)



        // experiment: use a tuple and accessor functions as a projection result
        type Product_availability_info = Domain.Product * Domain.Supplier * int * Common.Currency 
        let Product_availability_info_product ((x,_,_,_):Product_availability_info) = x
        let Product_availability_info_supplier ((_,x,_,_):Product_availability_info) = x
        let Product_availability_info_amount ((_,_,x,_):Product_availability_info) = x
        let Product_availability_info_price ((_,_,_,x):Product_availability_info) = x
        

        let Product_availability_with_suppliers = 
            {
                Projection.Init = Map.empty
                Projection.Selector = (fun event -> Some ())
                Projection.Step = (fun event -> 
                    match event with

                    | :? ``Received product availability annoucement`` as e ->    
                        exchange_by_id (e.``supplier id``,e.``product id``) (e.``product id``,e.``supplier id``,e.``quantity available``,e.``price per quantity``)

                    | :? ``Received product delisting annoucement`` as e ->       
                        remove_where (fun (supplier, product) _ -> product = e.``product id`` && supplier = e.``supplier id``)

                    | :? ``Product deprecated`` as e ->                
                        remove_where (fun (_, product) _ -> product = e.``product id``)

                    | :? ``Supplier account closed`` as e ->                
                        remove_where (fun (supplier, _) _ -> supplier = e.``supplier id``)

                    | _ -> no_change) 

                Projection.Final = Map.toList >> List.map snd
            }
            



            
        [<TestFixture>]
        type Concepts_and_projections()=

            [<Test>]
            member test.``The entity list is initially empty``() =
                let projection = Entity_list

                let history = List.empty
                let result = Eventsourcing.evaluate_ondemand projection history
                result |> should equal Set.empty

            [<Test>]
            member test.``The entity list remembers the designations of entities``() =
                let projection = Entity_list
                
                let supplier1 = new_Supplier()
                let supplier2 = new_Supplier()
                
                let history = 
                    [
                        Domain.event_Supplier_account_opened supplier1 "Supp 1 designation 1"
                        Domain.event_Supplier_account_opened supplier2 "Supp 2 designation 1"
                    ]

                let result = Eventsourcing.evaluate_ondemand projection history
                result |> should not' (equal Map.empty)
                result.[supplier1.Id] |> should equal "Supp 1 designation 1"
                result.[supplier2.Id] |> should equal "Supp 2 designation 1"              
              
            [<Test>]
            member test.``The entity list remembers changed designations of entities``() =
                let projection = Entity_list
                
                let supplier1 = new_Supplier()
                let supplier2 = new_Supplier()
                let history = 
                    [
                        Domain.event_Supplier_account_opened supplier1 "Supp 1 designation 1"
                        Domain.event_Supplier_account_opened supplier2 "Supp 2 designation 1"                    
                        Domain.event_Supplier_designation_changed supplier1 "Supp 1 designation 2"
                    ]
                let result = Eventsourcing.evaluate_ondemand projection history
                result |> should not' (equal Map.empty)
                result.[supplier1.Id] |> should equal "Supp 1 designation 2"
                result.[supplier2.Id] |> should equal "Supp 2 designation 1"


            [<Test>]
            member test.StockTest() =
                
                let product_1 = new_Product ()
                let product_2 = new_Product ()
                
                let history = 
                    [
                        Domain.event_Goods_received_in_depot product_1 10 (new_Purchaseorder()) (new_Supplier()) 
                        Domain.event_Goods_received_in_depot product_1 4 (new_Purchaseorder()) (new_Supplier()) 
                        Domain.event_Goods_dispatched_to_customer (new_Order()) product_1 5 "" (Currency 0M)
                        Domain.event_Goods_received_in_depot product_2 13 (new_Purchaseorder()) (new_Supplier()) 
                    ]
                Eventsourcing.evaluate_ondemand_filtered Product_items_in_stock product_2 history |> should equal 13
                Eventsourcing.evaluate_ondemand_filtered Product_items_in_stock product_1 history |> should equal 9





