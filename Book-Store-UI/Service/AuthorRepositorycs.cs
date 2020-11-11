using Blazored.LocalStorage;
using Book_Store_UI.Contracts;
using Book_Store_UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Book_Store_UI.Service
{
    public class AuthorRepositorycs : BaseRepository<Author>, IAuthorRepositorycs
    {
        private readonly IHttpClientFactory _client;
        private readonly ILocalStorageService _localStorage;
        public AuthorRepositorycs(IHttpClientFactory client, ILocalStorageService localStorage) : base(client, localStorage)
        {
            _client = client;
            _localStorage = localStorage;
        }
    }
}
