using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microfichas_App.Services
{
    public class AzureService : IAzureService
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<JObject> GetTokenAsync(string email)
        {
            var url = $"https://qa.wscentralizado.domonline.cl/api/personas/ObtenerAcceso?correo={email}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Verifica que la respuesta fue exitosa

            var responseBody = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseBody);
        }

        public async Task<JObject> PublishDocumentAsync(string token, string fileName, int fileType, string fileContentBase64)
        {
            // Configurar encabezado personalizado
            client.DefaultRequestHeaders.Remove("Token"); // Remover cualquier encabezado existente
            client.DefaultRequestHeaders.Add("Token", token); // Agregar encabezado personalizado

            var data = new { nombre = fileName, tipo = fileType, archivo = fileContentBase64 };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://qa.wscentralizado.domonline.cl/api/documentos/PublicarMicroficha", content);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody2 = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, {responseBody2}");
                throw new HttpRequestException($"Response status code does not indicate success: {response.StatusCode} ({responseBody2})");
            }

            response.EnsureSuccessStatusCode(); // Verifica que la respuesta fue exitosa

            var responseBody = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseBody);
        }
    }

}
