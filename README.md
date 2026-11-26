# Practical 8: Inventory Management Module (ADO.NET & CRUD Operations)

**Course:** 01CE1523 - .NET Technologies (CO4)  
**Student:** Basanagouda Sanjay Patil (92400120671), Batch B  
**Institution:** Faculty of Engineering and Technology, Marwadi University  

## Features
- Add new inventory items (`ProductName`, `Category`, `Quantity`, `Price`)
- Display all inventory items with stock value
- Update existing inventory item details
- Delete inventory items with confirmation
- Search inventory by Product ID or Product Name
- View available stock count and total inventory value
- Parameterized ADO.NET SQL queries via `Microsoft.Data.Sqlite`
- Input validation and structured exception handling

## Database Setup
The SQL schema is located in `schema.sql` and is automatically initialized on startup into `inventory.db`.

## Run Instructions
```bash
dotnet restore
dotnet build
dotnet run
```
