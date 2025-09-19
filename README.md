# ProductAPI

## Overview

ProductAPI is an ASP.NET Core Web API for managing products.  
It uses SQL Server for product data and Redis to store unique 6-digit product IDs, supporting distributed architecture.

---

## How to Run This Project

1. **Clone the repository:**
   ```sh
   git clone https://github.com/vasuklk/Product.git
   cd ProductAPI
   ```

2. **Update Configuration:**
   - Open `appsettings.json` (and/or `appsettings.Development.json`).
   - Add your SQL Server and Redis connection strings:
     ```json
     "ConnectionStrings": {
       "SQLConnection": "<your-sql-connection-string>",
       "RedisConnection": "<your-redis-connection-string>"
     }
     ```

3. **Restore NuGet packages:**
   ```sh
   dotnet restore
   ```

4. **Apply EF Core migrations:**
   ```sh
   dotnet ef database update
   ```

5. **Run the API:**
   ```sh
   dotnet run
   ```

6. **Access Swagger UI:**  
   Navigate to `https://localhost:<port>/swagger` in your browser to explore and test the APIs.

---

## API Endpoints

| Method | Endpoint                       | Description                       |
|--------|-------------------------------|-----------------------------------|
| GET    | `/api/products`               | Get all products                  |
| GET    | `/api/products/{id}`          | Get a product by Id               |
| POST   | `/api/products`               | Create a new product              |
| PUT    | `/api/products/{id}`          | Update an existing product        |
| DELETE | `/api/products/{id}`          | Delete a product                  |
| PUT    | `/api/products/decrement-stock/{id}/{quantity}` | Decrement product stock |
| PUT    | `/api/products/increment-stock/{id}/{quantity}` | Increment product stock |

---

## Redis Usage

- **Redis** is used to store all generated 6-digit product IDs in a set (`product_ids`).
- This ensures uniqueness of product IDs across all instances in a distributed environment.
- If the Redis set reaches 900,000 IDs, the API will throw a "Database full" exception.

---

## Configuration

- **SQL Server:**  
  Update the `SQLConnection` string in `appsettings.json` with your database details.

- **Redis:**  
  Update the `RedisConnection` string in `appsettings.json` with your Redis server details.

---

## Notes

- The API is designed for distributed systems, ensuring unique product IDs using Redis.
- Make sure both SQL Server and Redis are running and accessible from your application.

---