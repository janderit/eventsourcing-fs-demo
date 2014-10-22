namespace Eventsourcing_fs_Demo


module QueryHandling =

    open Eventstore
    open Eventsourcing
    open Model.Projections

    type Readmodels (store:Naive_Eventstore) =

        let allevents = store.retrieve_all

        member this.entity_list_readmodel () = Eventsourcing.evaluate_ondemand Entity_list (allevents())

        member this.supplier_list_readmodel () = Eventsourcing.evaluate_ondemand Supplier_list (allevents())
        member this.product_list_readmodel () = Eventsourcing.evaluate_ondemand Product_list (allevents())
        member this.product_availability_readmodel () = Eventsourcing.evaluate_ondemand Product_availability_with_suppliers (allevents())

        member this.open_orders_readmodel () = Eventsourcing.evaluate_ondemand Open_orders (allevents())
        member this.remaining_quantity_in_order id = Eventsourcing.evaluate_ondemand_filtered Remaining_quantity_in_order id (allevents())
        member this.open_purchase_orders_readmodel () = Eventsourcing.evaluate_ondemand Open_purchase_orders (allevents())
        member this.purchase_orders_states_readmodel id = Eventsourcing.evaluate_ondemand_filtered Purchase_order_states id (allevents())
        
        member this.available_items id = Eventsourcing.evaluate_ondemand_filtered Product_items_available id (allevents())
        member this.items_in_stock id = Eventsourcing.evaluate_ondemand_filtered Product_items_in_stock id (allevents())
        member this.items_allocated_to_orders id = Eventsourcing.evaluate_ondemand_filtered Product_items_allocated_to_orders id (allevents())
        member this.items_in_purchase id = Eventsourcing.evaluate_ondemand_filtered Product_items_in_purchase id (allevents())

