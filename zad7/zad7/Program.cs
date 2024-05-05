using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Ustawienie adresu URL punktu końcowego API
        string apiUrl = "http://localhost:5295/api/warehouse/AddProductToWarehouse";

        // Dane produktu do dodania do magazynu w formacie JSON
        string jsonRequestData = @"{
            ""IdProduct"": 1,
            ""IdWarehouse"": 2,
            ""Amount"": 10,
            ""CreatedAt"": ""2024-05-05T12:00:00""
        }";

        // Przygotowanie żądania HTTP POST z danymi produktu
        using (var client = new HttpClient())
        {
            try
            {
                var content = new StringContent(jsonRequestData, Encoding.UTF8, "application/json");

                // Wykonanie żądania POST do punktu końcowego API
                var response = await client.PostAsync(apiUrl, content);

                // Sprawdzenie odpowiedzi
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Produkt został dodany do magazynu.");
                }
                else
                {
                    Console.WriteLine("Wystąpił błąd podczas dodawania produktu do magazynu:");
                    Console.WriteLine(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wystąpił błąd podczas wykonywania żądania:");
                Console.WriteLine(ex.Message);
            }
        }
    }
}