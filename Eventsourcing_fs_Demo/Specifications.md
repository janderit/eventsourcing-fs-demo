
# Specifications, automated fulfillment system


## Problem domain


### Outline and use cases

* CQRS Inc. fulfills customer orders
* CQRS Inc. holds quantities of different products in stock
* a customer can order any available quantity of a single product
* customer orders are fulfilled from CQRS's depot (manually by an agent)

* (future version) products are restocked automatically up to a predetermined quantity per product


### Definitions and annotations

* a product is deliverable if it is in stock or begin restocked
* a product is available if it is deliverable or can be restocked
* shipping cost are calculated on a per-shipment basis


### non-functional requirements

* demonstrable during a single DDD.BE meetup
* minimal use of frameworks and libraries
  - web server library (suave)
  - testing framework (nunit+fsunit)
  - html templating (prearranged)


## Solution domain


### Number series

Consecutive order numbers with 5+ digits are provided by a order number series starting at 10000

### Business entities

* supplier
* product
* purchaseorder
* order

### Business events

#### standing data

* Supplier account opened
  - supplier id
  - designation

* Supplier designation changed
  - supplier id
  - designation

* Supplier account closed
  - supplier id

* Product listed
  - product id
  - designation 

* Product designation changed
  - product id
  - designation

* Product deprecated
  - product id

#### product supply

* Received product availability annoucement
  - supplier id
  - product id
  - quantity available
  - price per quantity

* Received product delisting annoucement
  - supplier id
  - product id

* Purchase order submitted to supplier
  - purchaseorder id
  - supplier id
  - product id
  - quantity

* Purchase order confirmed by supplier
  - purchaseorder id
  - supplier id
  - product id
  - quantity shipped
  - price per quantity

* Purchase order rejected by supplier
  - purchaseorder id
  - supplier id

* Supplier fulfilled purchase order
  - supplier id
  - purchaseorder id

#### product stocking

* Goods received in depot
  - product id
  - quantity
  - purchaseorder id
  - supplier id

* Goods dispatched to customer
  - order id
  - product id
  - quantity
  - shipping address
  - shipping expenses


#### ordering

* Customer order accepted
  - order id
  - order number
  - shipping address
  - product id
  - quantity
  - net price
  - shipping cost

* Customer order fulfilled
  - order id


#### order number series

* Order number allocated
  - order number
  - order id





### Definitions

* % id : domain entity <$>

* quantity % : number
* % number : number

* shipping cost : currency
* net value : currency
* % expenses : currency
* % price % : currency

* designation : text
* % address : text
* reason : text
