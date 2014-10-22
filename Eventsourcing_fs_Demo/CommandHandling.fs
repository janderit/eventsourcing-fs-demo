namespace Eventsourcing_fs_Demo

module CommandHandling =
 
    open System
    open Common
    open Model
    open Generated
    open Domain
    open Projections
    open CQRS


    let ensure (condition:bool) (reason:string) =
        if (not condition) then failwith reason else ()

    let single_or_failwith error list =
        if (List.length list = 1) then List.head list else failwith error 
    

    

    type Order_number_series (uow:UnitOfWork) =
        
        let get_by_id concept selector = Eventsourcing.evaluate_ondemand_filtered concept selector (uow.events())
        let get concept = Eventsourcing.evaluate_ondemand concept (uow.events())            
        
        let next_order_number () = get Projections.Next_order_number

        member this.allocate_order_number (order:Order) =
            let allocated_number = next_order_number()
            uow.stage <| event_Order_number_allocated (allocated_number) order
            allocated_number
    


    type SupplierAccount (uow:UnitOfWork, supplier:Domain.Supplier)=
        
        let get_by_id concept selector = Eventsourcing.evaluate_ondemand_filtered concept selector (uow.events())
        let get concept = Eventsourcing.evaluate_ondemand concept (uow.events())            

        let isActive () = get Supplier_list |> List.exists (fun (sup,_) -> sup=supplier)

        member this.Id = supplier

        member this.ensure_accepts_purchase_orders () = 
            ensure (isActive()) (sprintf "Unknown or closed supplier account %s" (supplier.Id.ToString()))

        

    type Supplier_purchase_order (uow:UnitOfWork, purchaseorder:Purchaseorder, supplier:SupplierAccount) =
        
        let get_by_id concept selector = Eventsourcing.evaluate_ondemand_filtered concept selector (uow.events())
        let get concept = Eventsourcing.evaluate_ondemand concept (uow.events())            
        
        let state() = get_by_id Purchase_order_states purchaseorder

        member this.submit product quantity =
            ensure (state() = Purchase_order_state.Unknown) "purchase order is not new"
            supplier.ensure_accepts_purchase_orders()

            let available = 
                get Product_availability_with_suppliers
                |> List.filter (fun x -> Product_availability_info_product x = product && Product_availability_info_supplier x = supplier.Id) 
                |> single_or_failwith "Product not available with supplier (or insufficient quantity)"

            uow.stage <| Domain.event_Purchase_order_submitted_to_supplier purchaseorder supplier.Id product quantity
            ensure (get_by_id Purchase_order_states purchaseorder = Purchase_order_state.Submitted) "purchase order failed to be submitted"

            // assume automagical supplier
            uow.stage <| Domain.event_Purchase_order_confirmed_by_supplier purchaseorder supplier.Id product quantity (Product_availability_info_price available)
            ensure (get_by_id Purchase_order_states purchaseorder = Purchase_order_state.Confirmed) "supplier did not confirm purchase order"

        member this.confirm_delivery () =
            ensure (state() = Purchase_order_state.Confirmed) "cannot register delivery for a rejected or closed purchase order"
            uow.stage <| Domain.event_Supplier_fulfilled_purchase_order supplier.Id purchaseorder



    type Customer_order (uow:UnitOfWork, id:Order, allocate_order_number:Order->int) =
        let get_by_id concept selector = Eventsourcing.evaluate_ondemand_filtered concept selector (uow.events())
        let get concept = Eventsourcing.evaluate_ondemand concept (uow.events())            

        let info () = get Open_orders |> Seq.find (fun i -> i.Id=id)
        let state () = get_by_id Order_states id
        let remaining_quantity () = get_by_id Remaining_quantity_in_order id
        let ordered_product () = info().Product
        
        member this.evaluate  order product quantity price shipping shipping_address =
            
            ensure (get_by_id Product_items_available product >= quantity) "cannot commit larger quantity than available to a customer order"
                
            let ordernumber = allocate_order_number order
            uow.stage <| event_Customer_order_accepted order ordernumber shipping_address product quantity price shipping

        member this.register_dispatch product quantity =
            
            ensure (state() = Order_state.Accepted) "cannot dispatch for a rejected or closed order"
            ensure (ordered_product() = product) "cannot dispatch product not included in order"
            let r = remaining_quantity ()
            ensure (r >= quantity) "cannot dispatch larger quantity than ordered"
        
            if (remaining_quantity ()= 0)
                    then uow.stage <| Domain.event_Customer_order_fulfilled id

        member this.shippingAddress () = info().ShippingAddress



    
    let decorate_with_debug_output backend =
        fun command uow ->
            System.Diagnostics.Debug.WriteLine ("handling " + command.ToString())
            backend command uow


    let handle (command:obj) (uow:UnitOfWork) =                
    
        let get_by_id concept selector = Eventsourcing.evaluate_ondemand_filtered concept selector (uow.events())
                    
    
        match command with
        
        | :? API.``submit purchase order`` as cmd -> 
            
            let purchaseorder = cmd.``purchaseorder id``
            let supplier = cmd.``supplier id``
            let product = cmd.``product id``
            let quantity = cmd.quantity
            
            let order = new Supplier_purchase_order(uow, purchaseorder, new SupplierAccount(uow, supplier))
            order.submit product quantity


        | :? API.``register arrival of goods`` as cmd -> 
            
            let purchaseorder = cmd.``purchaseorder id``
            let supplier = cmd.``supplier id``
            let product = cmd.``product id``
            let quantity = cmd.quantity

            uow.stage <| Domain.event_Goods_received_in_depot product quantity purchaseorder supplier
            let order = new Supplier_purchase_order(uow, purchaseorder, new SupplierAccount(uow, supplier))
            order.confirm_delivery ()


        | :? API.``evaluate order`` as cmd ->
            let id = cmd.``order id``
            let product = cmd.``product id``
            let shipping_address = cmd.``shipping address``
            let quantity = cmd.quantity
            let price = cmd.``net price``
            let shipping = cmd.shipping

            let number_series = new Order_number_series (uow)
            let order = new Customer_order(uow, id, number_series.allocate_order_number)
            order.evaluate id product quantity price shipping shipping_address
            ()


        | :? API.``dispatch goods`` as cmd -> 
            
            let order = cmd.``order id``
            let product = cmd.``product id``
            let quantity = cmd.quantity

            let customer_order = new Customer_order(uow, order, (fun _->failwith "Not allowed"))

            customer_order.register_dispatch product quantity

            ensure (get_by_id Product_items_in_stock product >= quantity) "cannot dispatch larger quantity than in stock"
            let expenses = Currency 5.0M
            uow.stage <| Domain.event_Goods_dispatched_to_customer order product quantity (customer_order.shippingAddress()) expenses


        | _ -> failwith ("No command handler defined for "+command.ToString())
        


    module CommandHandlingTests =
        
        open API
        open NUnit.Framework
        open FsUnit
    
    
        [<TestFixture>]
        type Purchase_order_process_demonstration () =
            
            let handle command history =
                let uow = new UnitOfWork (history)
                let handle = decorate_with_debug_output <| handle
                handle command uow
                uow.commit ()
    
            [<Test>]
            member this.``Submit Purchase Order command issues purchase order`` () =
                
                let supplier1 = new_Supplier()
    
                let product = new_Product()
    
                let history = (
                    [
                        Domain.event_Supplier_account_opened supplier1 "Supp 1 designation 1"
                        Domain.event_Received_product_availability_annoucement supplier1 product 1000 (Common.Currency 1.00M) 
                    ])
    
                let po = new_Purchaseorder()
                let quantity = 42
    
                let command = {``submit purchase order``.``purchaseorder id``=po; ``supplier id``=supplier1; ``product id``=product; quantity=quantity}
    
                let emitted_events = handle command history
    
                emitted_events |> List.head |> should equal (Domain.event_Purchase_order_submitted_to_supplier po supplier1 product quantity)
    
            [<Test>]
            member this.``Submit Purchase Order fails for unknown supplier`` () =
                
                let supplier1 = new_Supplier()
                let supplier2 = new_Supplier()
    
                let history = (
                    [
                        Domain.event_Supplier_account_opened supplier1 "Supp 1 designation 1"
                    ])
    
                let po = new_Purchaseorder()
                let product = new_Product()
                let quantity = 42
    
                let command = {API.``submit purchase order``.``purchaseorder id``=po; API.``submit purchase order``.``supplier id``=supplier2; API.``submit purchase order``.``product id``=product; API.``submit purchase order``.quantity=quantity}
    
                let test = new TestDelegate(fun () -> handle command history |> ignore)
                test |> should throw typeof<Exception>
                               
