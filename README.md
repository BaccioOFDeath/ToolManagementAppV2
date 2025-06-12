# ToolManagementAppV2

This application manages tools, rentals and users. The SQLite database uses an auto-increment `INTEGER PRIMARY KEY` for each tool record. Throughout the codebase, `ToolID` is represented as a `string` to simplify binding in the UI and service interfaces. When creating a new tool, no `ToolID` value should be provided; it is generated automatically by the database.
