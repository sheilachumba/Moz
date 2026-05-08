using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;

namespace MOZ_UPGRADE.Repositories.Services
{
    public class BusinessCentralService : IBusinessCentralService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;

        public BusinessCentralService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<IEnumerable<ClaimDocument>> GetClaimDocumentsAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var token = user.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No access token found.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/ClaimDocuments";
            var response = await _httpClient.GetFromJsonAsync<ODataResponse<ClaimDocument>>(url);
            
            return response?.Value ?? new List<ClaimDocument>();
        }

        public async Task<IEnumerable<TermListItem>> GetTermListAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var token = user.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No access token found.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/TermList";
            var response = await _httpClient.GetFromJsonAsync<ODataResponse<TermListItem>>(url);
            return response?.Value ?? new List<TermListItem>();
        }
    }

    public class ODataResponse<T>
    {
        public List<T> Value { get; set; } = new List<T>();
    }
}
