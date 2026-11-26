using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace InventoryManagementModule
{
    class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }

    class Program
    {
        static string connectionString = "Data Source=inventory.db";

        static void Main(string[] args)
        {
            Console.Title = "Inventory Management Module";
            InitializeDatabase();
            SeedSampleData();

            bool running = true;
            while (running)
            {
                ShowMenu();
                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            AddItem();
                            break;
                        case "2":
                            DisplayAllItems();
                            break;
                        case "3":
                            UpdateItem();
                            break;
                        case "4":
                            DeleteItem();
                            break;
                        case "5":
                            SearchItem();
                            break;
                        case "6":
                            ViewStockAndValue();
                            break;
                        case "7":
                            running = false;
                            Console.WriteLine("\nGoodbye! Exiting Inventory Management Module.\n");
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please select 1-7.");
                            break;
                    }
                }
                catch (SqliteException ex)
                {
                    Console.WriteLine("Database Error: " + ex.Message);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Input Error: Please enter a valid numeric value.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                if (running)
                {
                    Console.WriteLine("\nPress Enter to continue...");
                    Console.ReadLine();
                }
            }
        }

        // ---------------- DATABASE SETUP ----------------
        static void InitializeDatabase()
        {
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        ProductId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductName TEXT NOT NULL,
                        Category TEXT NOT NULL,
                        Quantity INTEGER NOT NULL CHECK (Quantity >= 0),
                        Price REAL NOT NULL CHECK (Price > 0),
                        CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                    );";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void SeedSampleData()
        {
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (SqliteCommand countCmd = new SqliteCommand("SELECT COUNT(*) FROM Products", conn))
                {
                    long count = (long)countCmd.ExecuteScalar();
                    if (count > 0) return;
                }

                var sampleItems = new List<Product>
                {
                    new Product { ProductName = "Dell Wireless Mouse", Category = "Electronics", Quantity = 25, Price = 850.00 },
                    new Product { ProductName = "HP Mechanical Keyboard", Category = "Electronics", Quantity = 15, Price = 2400.00 },
                    new Product { ProductName = "A4 Printing Paper", Category = "Stationery", Quantity = 60, Price = 320.00 },
                    new Product { ProductName = "USB-C Hub 6-in-1", Category = "Accessories", Quantity = 12, Price = 1650.00 }
                };

                string insertSql = "INSERT INTO Products (ProductName, Category, Quantity, Price) VALUES (@Name, @Category, @Quantity, @Price)";
                foreach (var item in sampleItems)
                {
                    using (SqliteCommand cmd = new SqliteCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", item.ProductName);
                        cmd.Parameters.AddWithValue("@Category", item.Category);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@Price", item.Price);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        // ---------------- UI HELPERS ----------------
        static void ShowMenu()
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine("             INVENTORY MANAGEMENT MODULE");
            Console.WriteLine("=======================================================");
            Console.WriteLine(" 1. Add New Inventory Item");
            Console.WriteLine(" 2. Display All Inventory Items");
            Console.WriteLine(" 3. Update Existing Inventory Item");
            Console.WriteLine(" 4. Delete Inventory Item");
            Console.WriteLine(" 5. Search Inventory (by ID or Product Name)");
            Console.WriteLine(" 6. View Available Stock & Total Inventory Value");
            Console.WriteLine(" 7. Exit");
            Console.Write("Choose an option: ");
        }

        // ---------------- CRUD OPERATIONS ----------------
        static void AddItem()
        {
            Console.Write("\nEnter Product Name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Validation Error: Product name cannot be empty.");
                return;
            }

            Console.Write("Enter Category: ");
            string category = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(category))
            {
                Console.WriteLine("Validation Error: Category cannot be empty.");
                return;
            }

            Console.Write("Enter Quantity: ");
            int quantity = int.Parse(Console.ReadLine());
            if (quantity < 0)
            {
                Console.WriteLine("Validation Error: Quantity cannot be negative.");
                return;
            }

            Console.Write("Enter Unit Price (Rs.): ");
            double price = double.Parse(Console.ReadLine());
            if (price <= 0)
            {
                Console.WriteLine("Validation Error: Price must be greater than zero.");
                return;
            }

            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Products (ProductName, Category, Quantity, Price) VALUES (@Name, @Category, @Quantity, @Price); SELECT last_insert_rowid();";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name.Trim());
                    cmd.Parameters.AddWithValue("@Category", category.Trim());
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@Price", price);

                    long newId = (long)cmd.ExecuteScalar();
                    Console.WriteLine("Item added successfully! (Product ID: " + newId + ")");
                }
            }
        }

        static void DisplayAllItems()
        {
            Console.WriteLine();
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ProductId, ProductName, Category, Quantity, Price FROM Products ORDER BY ProductId";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    PrintTableHeader();
                    bool hasRows = false;
                    while (reader.Read())
                    {
                        hasRows = true;
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string cat = reader.GetString(2);
                        int qty = reader.GetInt32(3);
                        double price = reader.GetDouble(4);
                        double value = qty * price;

                        Console.WriteLine(string.Format("{0,-5}{1,-25}{2,-15}{3,6}{4,12:N2}{5,14:N2}",
                            id, name, cat, qty, price, value));
                    }

                    if (!hasRows)
                        Console.WriteLine("No inventory records found.");

                    Console.WriteLine("-----------------------------------------------------------------------------");
                }
            }
        }

        static void UpdateItem()
        {
            DisplayAllItems();
            Console.Write("\nEnter Product ID to update: ");
            int id = int.Parse(Console.ReadLine());

            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string checkSql = "SELECT ProductName, Category, Quantity, Price FROM Products WHERE ProductId = @Id";
                string curName = null, curCat = null;
                int curQty = 0;
                double curPrice = 0;

                using (SqliteCommand checkCmd = new SqliteCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Id", id);
                    using (SqliteDataReader r = checkCmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            Console.WriteLine("Product with ID " + id + " not found.");
                            return;
                        }
                        curName = r.GetString(0);
                        curCat = r.GetString(1);
                        curQty = r.GetInt32(2);
                        curPrice = r.GetDouble(3);
                    }
                }

                Console.Write(string.Format("Enter New Name (leave blank for '{0}'): ", curName));
                string newName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newName)) newName = curName;

                Console.Write(string.Format("Enter New Category (leave blank for '{0}'): ", curCat));
                string newCat = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newCat)) newCat = curCat;

                Console.Write(string.Format("Enter New Quantity (leave blank for {0}): ", curQty));
                string qtyInput = Console.ReadLine();
                int newQty = string.IsNullOrWhiteSpace(qtyInput) ? curQty : int.Parse(qtyInput);
                if (newQty < 0)
                {
                    Console.WriteLine("Validation Error: Quantity cannot be negative.");
                    return;
                }

                Console.Write(string.Format("Enter New Price (leave blank for {0:N2}): ", curPrice));
                string priceInput = Console.ReadLine();
                double newPrice = string.IsNullOrWhiteSpace(priceInput) ? curPrice : double.Parse(priceInput);
                if (newPrice <= 0)
                {
                    Console.WriteLine("Validation Error: Price must be greater than zero.");
                    return;
                }

                string updateSql = "UPDATE Products SET ProductName = @Name, Category = @Category, Quantity = @Quantity, Price = @Price WHERE ProductId = @Id";
                using (SqliteCommand cmd = new SqliteCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", newName.Trim());
                    cmd.Parameters.AddWithValue("@Category", newCat.Trim());
                    cmd.Parameters.AddWithValue("@Quantity", newQty);
                    cmd.Parameters.AddWithValue("@Price", newPrice);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("Product ID " + id + " updated successfully!");
            }
        }

        static void DeleteItem()
        {
            DisplayAllItems();
            Console.Write("\nEnter Product ID to delete: ");
            int id = int.Parse(Console.ReadLine());

            Console.Write("Are you sure you want to delete Product ID " + id + "? (y/n): ");
            string confirm = Console.ReadLine();
            if (confirm == null || confirm.Trim().ToLower() != "y")
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }

            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string sql = "DELETE FROM Products WHERE ProductId = @Id";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                        Console.WriteLine("Product ID " + id + " deleted successfully.");
                    else
                        Console.WriteLine("Product with ID " + id + " not found.");
                }
            }
        }

        static void SearchItem()
        {
            Console.Write("\nEnter Product ID or Name keyword to search: ");
            string keyword = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                Console.WriteLine("Validation Error: Search keyword cannot be empty.");
                return;
            }

            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                int searchId;
                bool isNumeric = int.TryParse(keyword.Trim(), out searchId);

                string sql = isNumeric
                    ? "SELECT ProductId, ProductName, Category, Quantity, Price FROM Products WHERE ProductId = @Id OR LOWER(ProductName) LIKE LOWER(@Pattern)"
                    : "SELECT ProductId, ProductName, Category, Quantity, Price FROM Products WHERE LOWER(ProductName) LIKE LOWER(@Pattern)";

                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    if (isNumeric) cmd.Parameters.AddWithValue("@Id", searchId);
                    cmd.Parameters.AddWithValue("@Pattern", "%" + keyword.Trim() + "%");

                    using (SqliteDataReader reader = cmd.ExecuteReader())
                    {
                        PrintTableHeader();
                        bool found = false;
                        while (reader.Read())
                        {
                            found = true;
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string cat = reader.GetString(2);
                            int qty = reader.GetInt32(3);
                            double price = reader.GetDouble(4);
                            Console.WriteLine(string.Format("{0,-5}{1,-25}{2,-15}{3,6}{4,12:N2}{5,14:N2}",
                                id, name, cat, qty, price, qty * price));
                        }
                        if (!found)
                            Console.WriteLine("No matching inventory items found.");
                        Console.WriteLine("-----------------------------------------------------------------------------");
                    }
                }
            }
        }

        static void ViewStockAndValue()
        {
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*), COALESCE(SUM(Quantity), 0), COALESCE(SUM(Quantity * Price), 0) FROM Products";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        long itemCount = reader.GetInt64(0);
                        long totalStock = reader.GetInt64(1);
                        double totalValue = reader.GetDouble(2);

                        Console.WriteLine("\n--- INVENTORY SUMMARY ---");
                        Console.WriteLine("Distinct Products  : " + itemCount);
                        Console.WriteLine("Available Stock    : " + totalStock + " units");
                        Console.WriteLine(string.Format("Total Value (Rs.)  : {0:N2}", totalValue));
                    }
                }
            }
        }

        static void PrintTableHeader()
        {
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.WriteLine(string.Format("{0,-5}{1,-25}{2,-15}{3,6}{4,12}{5,14}",
                "ID", "Product Name", "Category", "Qty", "Price(Rs.)", "Value(Rs.)"));
            Console.WriteLine("-----------------------------------------------------------------------------");
        }
    }
}
