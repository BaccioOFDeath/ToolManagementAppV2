# Sample Export Files

These examples show the item and customer formats supported by InventoryManagementApp import/export.

## Items Export Samples

### items_sample.json

```json
[
  {
    "itemID": 1,
    "itemNumber": "ITEM-001",
    "name": "Cordless Drill",
    "location": "Workshop - Shelf A",
    "brand": "DeWalt",
    "partNumber": "DCD771C2",
    "supplier": "Home Depot",
    "purchasedDate": "2024-01-15T00:00:00",
    "notes": "18V compact drill/driver kit",
    "keywords": "power drill cordless battery",
    "quantityOnHand": 3,
    "rentedQuantity": 0,
    "isPowered": true,
    "isRentalItem": true
  },
  {
    "itemID": 2,
    "itemNumber": "ITEM-002",
    "name": "Digital Multimeter",
    "location": "Testing Station - Drawer 2",
    "brand": "Fluke",
    "partNumber": "87V",
    "supplier": "Grainger",
    "purchasedDate": "2024-02-20T00:00:00",
    "notes": "Industrial multimeter with temperature probe",
    "keywords": "test measure electrical voltage current",
    "quantityOnHand": 2,
    "rentedQuantity": 1,
    "isPowered": true,
    "isRentalItem": true
  }
]
```

### items_sample.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Items>
  <ItemModel>
    <ItemID>1</ItemID>
    <ItemNumber>ITEM-001</ItemNumber>
    <Name>Cordless Drill</Name>
    <Location>Workshop - Shelf A</Location>
    <Brand>DeWalt</Brand>
    <PartNumber>DCD771C2</PartNumber>
    <Supplier>Home Depot</Supplier>
    <PurchasedDate>2024-01-15T00:00:00</PurchasedDate>
    <Notes>18V compact drill/driver kit</Notes>
    <Keywords>power drill cordless battery</Keywords>
    <QuantityOnHand>3</QuantityOnHand>
    <RentedQuantity>0</RentedQuantity>
    <IsPowered>true</IsPowered>
    <IsRentalItem>true</IsRentalItem>
  </ItemModel>
  <ItemModel>
    <ItemID>2</ItemID>
    <ItemNumber>ITEM-002</ItemNumber>
    <Name>Digital Multimeter</Name>
    <Location>Testing Station - Drawer 2</Location>
    <Brand>Fluke</Brand>
    <PartNumber>87V</PartNumber>
    <Supplier>Grainger</Supplier>
    <PurchasedDate>2024-02-20T00:00:00</PurchasedDate>
    <Notes>Industrial multimeter with temperature probe</Notes>
    <Keywords>test measure electrical voltage current</Keywords>
    <QuantityOnHand>2</QuantityOnHand>
    <RentedQuantity>1</RentedQuantity>
    <IsPowered>true</IsPowered>
    <IsRentalItem>true</IsRentalItem>
  </ItemModel>
</Items>
```

### items_sample.csv

```csv
ItemID,ItemNumber,Name,Location,Brand,PartNumber,Supplier,PurchasedDate,Notes,Keywords,QuantityOnHand,RentedQuantity,IsPowered,IsRentalItem
1,ITEM-001,Cordless Drill,Workshop - Shelf A,DeWalt,DCD771C2,Home Depot,2024-01-15,18V compact drill/driver kit,power drill cordless battery,3,0,True,True
2,ITEM-002,Digital Multimeter,Testing Station - Drawer 2,Fluke,87V,Grainger,2024-02-20,Industrial multimeter with temperature probe,test measure electrical voltage current,2,1,True,True
```

## Customers Export Samples

### customers_sample.json

```json
[
  {
    "customerID": 1,
    "company": "Acme Construction",
    "contact": "John Smith",
    "email": "jsmith@acmeconstruction.com",
    "phone": "555-0100",
    "mobile": "555-0101",
    "address": "123 Builder Ave, Construction City, CC 12345"
  },
  {
    "customerID": 2,
    "company": "Bright Electric",
    "contact": "Sarah Johnson",
    "email": "sjohnson@brightelectric.com",
    "phone": "555-0200",
    "mobile": "555-0201",
    "address": "456 Power St, Electric Town, ET 67890"
  }
]
```

### customers_sample.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<Customers>
  <Customer>
    <CustomerID>1</CustomerID>
    <Company>Acme Construction</Company>
    <Contact>John Smith</Contact>
    <Email>jsmith@acmeconstruction.com</Email>
    <Phone>555-0100</Phone>
    <Mobile>555-0101</Mobile>
    <Address>123 Builder Ave, Construction City, CC 12345</Address>
  </Customer>
  <Customer>
    <CustomerID>2</CustomerID>
    <Company>Bright Electric</Company>
    <Contact>Sarah Johnson</Contact>
    <Email>sjohnson@brightelectric.com</Email>
    <Phone>555-0200</Phone>
    <Mobile>555-0201</Mobile>
    <Address>456 Power St, Electric Town, ET 67890</Address>
  </Customer>
</Customers>
```

### customers_sample.csv

```csv
CustomerID,Company,Contact,Email,Phone,Mobile,Address
1,Acme Construction,John Smith,jsmith@acmeconstruction.com,555-0100,555-0101,"123 Builder Ave, Construction City, CC 12345"
2,Bright Electric,Sarah Johnson,sjohnson@brightelectric.com,555-0200,555-0201,"456 Power St, Electric Town, ET 67890"
```

## Usage Notes

- JSON uses camelCase property names.
- XML uses PascalCase element names.
- CSV headers match the property names.
- Dates are in ISO 8601 format.
- Boolean values are represented as true/false in JSON and XML, and True/False in CSV.
