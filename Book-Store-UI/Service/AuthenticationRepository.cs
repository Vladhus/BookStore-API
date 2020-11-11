using Blazored.LocalStorage;
using Book_Store_UI.Contracts;
using Book_Store_UI.Providers;
using Book_Store_UI.Static;
using Book_Store_UI.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace Book_Store_UI.Service
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IHttpClientFactory _client;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthenticationRepository(IHttpClientFactory client, ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
        {
            _localStorage = localStorage;
            _client = client;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<bool> Login(LoginModel user)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Endpoints.LoginEndpoint); //RegisterEndPoint = URL

            request.Content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            var client = _client.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<TokenResponse>(content);

            //storage token
            await _localStorage.SetItemAsync("authToken",token.Token);

            //Change auth state of app
            await ((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedIn();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer",token.Token);

            return true;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).LoggedOut();
        }

        public async Task<bool> Register(RegistrationModel user)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Endpoints.RegisterEndpoint); //RegisterEndPoint = URL
            request.Content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            var client = _client.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
