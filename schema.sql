-- Inventory Management Database Schema (SQLite / ADO.NET)
CREATE TABLE IF NOT EXISTS Products (
    ProductId INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductName TEXT NOT NULL,
    Category TEXT NOT NULL,
    Quantity INTEGER NOT NULL CHECK (Quantity >= 0),
    Price REAL NOT NULL CHECK (Price > 0),
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);
