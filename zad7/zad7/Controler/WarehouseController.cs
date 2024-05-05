using System.Net;
using Microsoft.AspNetCore.Mvc;
using zad7.Model;

namespace zad7.Controler;

using System;
using System.Data.SqlClient;
using System.Web.Http;
using System.Data;


public class WarehouseController : ApiController
{
    private readonly string _connectionString = "Server=db-mssql16.pjwstk.edu.pl;Database=MagazynDB;User Id=s27611;Password=mssql;";

    [HttpPost]
    public IHttpActionResult AddProductToWarehouse(ProductWarehouseRequest request)
    {
        try
        {
            // Sprawdzenie czy wszystkie pola są wypełnione
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest("Nieprawidłowe żądanie.");
            }

            // Połączenie z bazą danych
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Sprawdzenie czy produkt o podanym IdProduct istnieje
                string productQuery = "SELECT COUNT(1) FROM Product WHERE IdProduct = @IdProduct";
                using (var productCmd = new SqlCommand(productQuery, connection))
                {
                    productCmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    int productExists = (int)productCmd.ExecuteScalar();
                    if (productExists == 0)
                    {
                        return NotFound();
                    }
                }

                // Sprawdzenie czy magazyn o podanym IdWarehouse istnieje
                string warehouseQuery = "SELECT COUNT(1) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
                using (var warehouseCmd = new SqlCommand(warehouseQuery, connection))
                {
                    warehouseCmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                    int warehouseExists = (int)warehouseCmd.ExecuteScalar();
                    if (warehouseExists == 0)
                    {
                        return NotFound();
                    }
                }

                // Sprawdzenie czy ilość jest większa niż 0
                if (request.Amount <= 0)
                {
                    return BadRequest("Ilość produktu musi być większa niż 0.");
                }

                // Sprawdzenie czy istnieje zamówienie dla danego produktu
                string orderQuery =
                    "SELECT COUNT(1) FROM [Order] WHERE IdProduct = @IdProduct AND Amount >= @Amount AND CreatedAt <= @CreatedAt";
                using (var orderCmd = new SqlCommand(orderQuery, connection))
                {
                    orderCmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    orderCmd.Parameters.AddWithValue("@Amount", request.Amount);
                    orderCmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
                    int orderExists = (int)orderCmd.ExecuteScalar();
                    if (orderExists == 0)
                    {
                        return BadRequest("Nie ma odpowiedniego zamówienia dla tego produktu.");
                    }
                }

                // Sprawdzenie czy zamówienie zostało już zrealizowane
                string fulfillmentQuery =
                    "SELECT COUNT(1) FROM Product_Warehouse WHERE IdOrder IN (SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct) AND IdOrder IS NOT NULL";
                using (var fulfillmentCmd = new SqlCommand(fulfillmentQuery, connection))
                {
                    fulfillmentCmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    int fulfillmentExists = (int)fulfillmentCmd.ExecuteScalar();
                    if (fulfillmentExists > 0)
                    {
                        return BadRequest("Zamówienie zostało już zrealizowane.");
                    }
                }

                // Aktualizacja kolumny FullfilledAt w tabeli Order
                string updateOrderQuery =
                    "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdProduct = @IdProduct AND Amount >= @Amount AND CreatedAt <= @CreatedAt";
                using (var updateOrderCmd = new SqlCommand(updateOrderQuery, connection))
                {
                    updateOrderCmd.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
                    updateOrderCmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    updateOrderCmd.Parameters.AddWithValue("@Amount", request.Amount);
                    updateOrderCmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
                    updateOrderCmd.ExecuteNonQuery();
                }

                // Wstawienie rekordu do tabeli Product_Warehouse
                string insertQuery =
                    "INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, (SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount >= @Amount AND CreatedAt <= @CreatedAt), @Amount, (SELECT Price FROM Product WHERE IdProduct = @IdProduct) * @Amount, @CreatedAt); SELECT SCOPE_IDENTITY();";
                using (var insertCmd = new SqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                    insertCmd.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    insertCmd.Parameters.AddWithValue("@Amount", request.Amount);
                    insertCmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
                    int productWarehouseId = Convert.ToInt32(insertCmd.ExecuteScalar());
                    return Ok(productWarehouseId);
                }
            }
        }
        catch (Exception ex)
        {
            return Content(HttpStatusCode.InternalServerError, $"Wystąpił błąd: {ex.Message}");
        }
    }
    
    
    
    
    
    public IHttpActionResult AddProductToWarehouseUsingStoredProc(ProductWarehouseRequest request)
    {
        try
        {
            // Sprawdzenie czy wszystkie pola są wypełnione
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest("Nieprawidłowe żądanie.");
            }

            // Połączenie z bazą danych
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Wywołanie procedury składowanej
                using (var command = new SqlCommand("AddProductToWarehouse", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                    command.Parameters.AddWithValue("@Amount", request.Amount);
                    command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

                    // Wywołanie procedury składowanej
                    var result = command.ExecuteScalar();
                    int productWarehouseId = Convert.ToInt32(result);
                    return Ok(productWarehouseId);
                }
            }
        }
        catch (Exception ex)
        {
            return Content(HttpStatusCode.InternalServerError, $"Wystąpił błąd: {ex.Message}");
        }
    }
}


