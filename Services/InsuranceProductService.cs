using Moz.Models; 
using System.Net; 
using System.Text.Json; 

namespace Moz.Services 
{
    // Define the interface for the insurance product service
    public interface IInsuranceProductService
    {
        // Asynchronous method to fetch a list of insurance products
        Task<List<InsuranceProduct>> GetInsuranceProducts();
    }

    // Implementation of the IInsuranceProductService interface
    public class InsuranceProductService : IInsuranceProductService
    {
        // Method to make an HTTP request and return the list of insurance products
        public async Task<List<InsuranceProduct>> GetInsuranceProducts()
        {
            // Create an HttpClientHandler to handle authentication
            var handler = new HttpClientHandler
            {
                
                Credentials = new NetworkCredential("Administrator", "Insurance@2030#", "YourDomain")
            };

            // Create the HttpClient object using the handler above
            var client = new HttpClient(handler);

            // Set the URL for the insurance products endpoint (replace with your exact endpoint if needed)
            var url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/productclasscard";

            // Make the GET request asynchronously
            var response = await client.GetAsync(url);

            // Check if the HTTP response was successful
            if (response.IsSuccessStatusCode)
            {
                // Read the raw JSON string from the response
                var jsonData = await response.Content.ReadAsStringAsync();

                // Log the response data to the console (for debugging purposes)
                Console.WriteLine($"Response Data: {jsonData}");

                // Deserialize the JSON into the InsuranceProductResponse class
                var productResponse = JsonSerializer.Deserialize<InsuranceProductResponse>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Ignore case when matching property names
                });

                // Return the list of insurance products, or an empty list if the response is null
                return productResponse?.Value ?? new List<InsuranceProduct>();
            }
            else
            {
                // Read and log the error response content
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed Response: {response.StatusCode}, {errorContent}");

                // Throw an exception if the request failed
                throw new Exception($"Failed to fetch data: {response.StatusCode}");
            }
        }
    }
}
