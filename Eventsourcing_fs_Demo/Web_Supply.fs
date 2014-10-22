namespace Eventsourcing_fs_Demo.WebApplication

module Suppliers =

        open Eventsourcing_fs_Demo
        open Layout
        open Common
        open Generated
        open Model
        open Domain
        open Projections
        open API
        open System
    
        
        let supplier_details (id:Domain.Supplier) =
            id.ToString()
     
        
        let supplier_list_entry (id:Domain.Supplier, designation) = 
            [ link (details_url "supplier" id) designation ] |> datarow
     
        let supplier_list (context:WebContext) =        
            let suppliers = context.Readmodels.supplier_list_readmodel()
            [
                section_title "List of active suppliers"
                suppliers |> List.map supplier_list_entry |> datatable
            ] |> collect
        

        let purchase_form_handler (fielddata:(string*string) list) (checked_handle:obj -> string) =
            let supplier = Domain.Supplier <| new Guid(form_data fielddata "supplier_id")
            let product = Domain.Product <| new Guid(form_data fielddata "product_id")
            let quantity = Int32.Parse(form_data fielddata "quantity")
            let purchaseorder = Domain.new_Purchaseorder ()
            let command = 
                {
                    ``submit purchase order``.``purchaseorder id`` = purchaseorder
                    ``supplier id`` =supplier
                    ``product id`` = product
                    quantity = quantity
                }
            checked_handle command

        let purchaseform (supplier:Domain.Supplier) (product:Domain.Product) =
            let fields = 
                [
                    id_field "supplier_id" supplier.Id
                    id_field "product_id" product.Id
                    number_field "quantity" 0 "Quantity"
                ] |> collect
            
            let url = "/supply/purchase_order"

            [
                "Purchase order: "
                form url "submit purchase order !" fields
            ] |> collect



        let order_form_handler (fielddata:(string*string) list) (checked_handle:obj -> string) =
            let product = Domain.Product <| new Guid(form_data fielddata "product_id")
            let quantity = Int32.Parse(form_data fielddata "quantity")
            let shipping_address = form_data fielddata "shipping_address"
            let price = Currency (decimal(quantity)*Decimal.Parse(form_data fielddata "price"))
            let shipping = Currency (Decimal.Parse(form_data fielddata "shipping"))
            let command = 
                {
                    ``evaluate order``.``order id`` = new_Order()
                    ``product id`` = product
                    quantity = quantity
                    ``shipping address``= shipping_address
                    ``net price``= price
                    shipping = shipping
                }
            checked_handle command

        let order_form (context:WebContext) (product:Product) = 
            let fields = 
                [
                    id_field "product_id" product.Id
                    number_field "quantity" 0 "Quantity"
                    text_field "shipping_address" "" "Shipping address"
                    number_field "price" "?" "Price per item"
                    number_field "shipping" 5.00M "Price"
                ] |> collect
            
            let url = "/supply/evaluate_order"

            [
                form url "submit customer order !" fields
            ] |> collect

        let details_instock (context:WebContext) id info suppliers =
    
            let total_available_from_suppliers = suppliers |> List.map Product_availability_info_amount |> Seq.sum
            let min_price = suppliers |> List.map Product_availability_info_price |> Seq.min
            let max_price = suppliers |> List.map Product_availability_info_price |> Seq.max
            let total_suppliers = suppliers.Length
            
            let texts = context.Readmodels.entity_list_readmodel()
            [
                section_title info+" Details"
                [
                    [ "In stock "; sprintf "%d" (context.Readmodels.items_in_stock id) ] |> datarow
                    [ "Available "; sprintf "%d" (context.Readmodels.available_items id) ] |> datarow
                    [ "Supplier availability"; (sprintf "%d items from %d suppliers" total_available_from_suppliers total_suppliers) ] |> datarow
                    [ "Price range"; (sprintf "%s to %s" min_price.EUR max_price.EUR) ] |> datarow
                ] |> datatable
    
                section_title "Register customer order"
                order_form context id

                section_title "Available from the following suppliers"
                
                suppliers 
                    |> Seq.map(fun (product,supplier,amount,price) -> 
                        [texts.[supplier.Id]; amount.ToString(); price.EUR; purchaseform supplier id ] |> datarow) 
                    |> Seq.toList 
                    |> datatable2 (headerrow ["Supplier";"Items available";"Price per item"])

            ] |> collect

    
        let details_outofstock id info =
            [
                section_title info+" Details"
                [
                    [ "Availability"; "Not available from any suppliers" ] |> datarow
                ] |> datatable
            ] |> collect
    
    
        let details (context:WebContext) (id:Domain.Product) =
            let info = context.Readmodels.product_list_readmodel () |> List.filter (fun (x,_) -> x=id ) |> List.head |> snd
            let suppliers = context.Readmodels.product_availability_readmodel () |> List.filter (fun x -> (Product_availability_info_product x)=id)
            if (suppliers.Length>0) then details_instock context id info suppliers else details_outofstock id info
    
        let product_list_entry (context:WebContext) (id:Domain.Product, designation) = 
            [ link (details_url "product" id) designation ] |> datarow
    
        let product_list (context:WebContext) =        
            let suppliers =  context.Readmodels.product_list_readmodel() 
            [
                section_title "Product list"
                suppliers |> List.map (product_list_entry context) |> datatable
            ] |> collect       
            
    
        let suppliers_home (context:WebContext) = [product_list context; supplier_list context] |> collect |> page (context.Company+ " - Products + Suppliers") context.Menu "suppliers"
