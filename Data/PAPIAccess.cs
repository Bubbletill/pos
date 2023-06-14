using BT_COMMONS.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BT_COMMONS;
using System.Net.Http.Json;

namespace BT_POS.Data;

class PAPIAccess : IAPIAccess
{
    private string _token;
    private string _apiUrl;
    private readonly DatabaseAccess _dbAccess;

    public PAPIAccess(DatabaseAccess dbAccess)
    {
        _dbAccess = dbAccess;
    }

    private HttpClient GetClient()
    {
        HttpClient client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                return true;
            }
        });
        client.BaseAddress = new Uri(_apiUrl);

        if (_token != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        return client;
    }

    public async Task<APIResponse<T>> Get<T>(string url)
    {
        using (HttpResponseMessage response = await GetClient().GetAsync(url))
        {
            try
            {
                var responseData = await response.Content.ReadFromJsonAsync<T>();
                return new APIResponse<T>
                {
                    StatusCode = response.StatusCode,
                    Data = responseData
                };
            }
            catch (Exception ex)
            {
                return new APIResponse<T>
                {
                    StatusCode = response.StatusCode,
                    Data = default
                };
            }
        }
    }

    public async Task<APIResponse<T>> Post<T, U>(string url, U data)
    {
        using (HttpResponseMessage response = await GetClient().PostAsJsonAsync(url, data))
        {
            var responseData = await response.Content.ReadFromJsonAsync<T>();
            return new APIResponse<T>
            {
                StatusCode = response.StatusCode,
                Data = responseData
            };
        }
    }

    public void UpdateWithToken(string token)
    {
        _token = token;
    }

    public void UpdateWithUrl(string url)
    {
        _apiUrl = url;
    }
}
