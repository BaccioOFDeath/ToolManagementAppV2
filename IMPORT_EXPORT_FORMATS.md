# Multiple Import/Export Formats Feature

## Overview

The Inventory Management App now supports multiple file formats for importing and exporting Items and Customers data. This provides flexibility for users to work with different data formats based on their needs and existing workflows.

## Supported Formats

### Items
- **CSV** (Comma-Separated Values) - Traditional format with column mapping
- **JSON** (JavaScript Object Notation) - Structured format, no mapping required
- **XML** (Extensible Markup Language) - Structured format, no mapping required

### Customers
- **CSV** (Comma-Separated Values) - Traditional format with column mapping
- **JSON** (JavaScript Object Notation) - Structured format, no mapping required
- **XML** (Extensible Markup Language) - Structured format, no mapping required

## How to Use

### Importing Data

1. Navigate to the **Import/Export** page in the application
2. Click the appropriate import button (e.g., "Import Items" or "Import Customers")
3. In the file dialog, select your desired file format from the dropdown
4. Choose your file
5. For CSV files: Map the columns from your file to the application fields
6. For JSON/XML files: The import happens automatically with proper structure validation
7. Review the import log for success messages and any skipped rows

### Exporting Data

1. Navigate to the **Import/Export** page
2. Click the appropriate export button (e.g., "Export Items" or "Export Customers")
3. In the file dialog, choose your desired format by changing the file extension:
   - `.csv` for CSV format
   - `.json` for JSON format
   - `.xml` for XML format
4. Save the file
5. Review the export log for confirmation

## File Format Details

### CSV Format
- **Items**: Supports all item fields with user-defined column mapping
- **Customers**: Supports all customer fields with user-defined column mapping
- **Advantage**: Works with existing spreadsheet workflows
- **Note**: Requires manual column mapping during import

### JSON Format
- **Structure**: Array of objects with camelCase property names
- **Advantage**: Easy to read, widely supported, works well with web applications
- **Example (Items)**:
```json
[
  {
    "itemNumber": "ITEM-001",
    "name": "Sample Item",
    "location": "Shelf A",
    "brand": "BrandName",
    "quantityOnHand": 5,
    "isRentalItem": true
  }
]
```

- **Example (Customers)**:
```json
[
  {
    "company": "Acme Corp",
    "contact": "John Doe",
    "email": "john@acme.com",
    "phone": "555-1234"
  }
]
```

### XML Format
- **Structure**: XML document with root element "Items" or "Customers"
- **Advantage**: Highly structured, supports validation, works well with enterprise systems
- **Example (Items)**:
```xml
<Items>
  <ItemModel>
    <ItemNumber>ITEM-001</ItemNumber>
    <Name>Sample Item</Name>
    <Location>Shelf A</Location>
    <Brand>BrandName</Brand>
    <QuantityOnHand>5</QuantityOnHand>
    <IsRentalItem>true</IsRentalItem>
  </ItemModel>
</Items>
```

## Validation and Error Handling

### Import Validation
- **Items**: Must have an `ItemNumber` field
- **Customers**: Must have either `Company` or `Contact` field
- Invalid records are skipped and logged
- Duplicate ItemNumbers are automatically skipped

### Error Messages
- File format not supported
- Missing required fields
- Invalid data format
- Parsing errors

All errors are logged in the Import/Export log panel for review.

## Architecture

### Interfaces
- `IDataImporter<T>` - Generic interface for importing data
- `IDataExporter<T>` - Generic interface for exporting data

### Implementations
Each format has dedicated importer and exporter classes:
- `ItemJsonImporter` / `ItemJsonExporter`
- `ItemXmlImporter` / `ItemXmlExporter`
- `ItemCsvExporter` (CSV import uses existing mapping logic)
- `CustomerJsonImporter` / `CustomerJsonExporter`
- `CustomerXmlImporter` / `CustomerXmlExporter`
- `CustomerCsvExporter` (CSV import uses existing mapping logic)

### Extension Points
New formats can be easily added by:
1. Creating a new importer/exporter class implementing the interfaces
2. Registering it in `ImportExportViewModel` constructor
3. The UI automatically includes it in file dialogs

## Benefits

1. **Flexibility**: Choose the format that best fits your workflow
2. **Data Exchange**: Easy integration with other systems (JSON/XML for APIs, CSV for Excel)
3. **Backup Options**: Multiple formats provide redundancy for data archival
4. **Migration**: Simplify data migration between different systems
5. **Automation**: JSON/XML formats enable scripted imports without manual mapping

## Notes

- CSV format is recommended for spreadsheet users and existing workflows
- JSON format is recommended for web developers and API integration
- XML format is recommended for enterprise systems and data exchange
- All formats maintain data integrity and validation
- Existing CSV import/export functionality is preserved and enhanced
