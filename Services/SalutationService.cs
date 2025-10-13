using Microsoft.Extensions.Configuration;
using Moz.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClientPortal.Models;

namespace ClientPortal.Services
{
    public class SalutationService
    {
        private readonly HttpClient _httpClient;

        public SalutationService()
        {
            _httpClient = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes("username:password");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<List<Salutation>> GetSalutationsAsync()
        {
            var url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD INSURANCE')/Salutations";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<Salutation>();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("value");

            var list = new List<Salutation>();
            foreach (var item in items.EnumerateArray())
            {
                list.Add(new Salutation
                {
                    Code = item.GetProperty("Code").GetString(),
                    Description = item.GetProperty("Description").GetString()
                });

            }

            return list;
        }
    }
}