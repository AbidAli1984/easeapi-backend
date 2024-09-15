using BAL.IServices;
using BOL.DataModel;
using BOL.DTO;
using DAL.IRepository;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.Data;

namespace BAL.Services
{
    public class APIConfigurationService : IAPIConfigurationService
    {
        private readonly IAPIConfigruationRepository _apiConfigruationRepository;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions;

        public APIConfigurationService(IAPIConfigruationRepository apiConfigruationRepository, IMemoryCache cache)
        {
            _apiConfigruationRepository = apiConfigruationRepository;
            _httpClient = new HttpClient();
            _cache = cache;
            _cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
        }

        public async Task<bool> SaveConfiguration(APIConfigurationResponse configurationAPI)
        {
            //Checking if the configuration is already exists with same name
            var existingConfiguration = await _apiConfigruationRepository.GetConfiguration(configurationAPI.Name);
            if (existingConfiguration != null)
                return false;

            Random random = new Random();
            ApiConfiguration apiConfiguration = new ApiConfiguration
            {
                Name = configurationAPI.Name,
                APIEndpoint = configurationAPI.Url,
                UserID = random.Next(1000, 9999),
                ApiFields = GetApiFields(configurationAPI.Fields) //Processing selected feilds to save
            };

            //Save configuration in the database with list of fields selected
            await _apiConfigruationRepository.SaveConfiguration(apiConfiguration);
            return true;
        }

        private static List<ApiField> GetApiFields(List<string> fields)
        {
            var ApiFields = new List<ApiField>();
            //Adding the list of fields selected
            foreach (string name in fields)
            {
                ApiFields.Add(new ApiField { Field = name });
            }
            return ApiFields;
        }

        //Receive request from client with unique configuration name.
        public async Task<string> ProcessRequest(string name)
        {
            //process the data if requested data is not available in cache
            if (!_cache.TryGetValue(name, out string response))
            {
                //Retrieve configuration from database using unique name.
                var apiConfiguration = await _apiConfigruationRepository.GetConfiguration(name);
                if (apiConfiguration != null && apiConfiguration.ApiFields.Any())
                {
                    //Fetch data from external API based on configuration.
                    string rawData = await FetchDataFromApi(apiConfiguration.APIEndpoint);
                    //Apply user-defined transformations to the retrieved data.
                    JArray array = await ProcessData(apiConfiguration, rawData);
                    response = array.ToString();
                }

                //store processed data in cache
                if (!string.IsNullOrEmpty(response))
                    _cache.Set(name, response, _cacheEntryOptions);
            }
            //Send the processed data back to the client.
            return response;
        }

        private async Task<JArray> ProcessData(ApiConfiguration apiConfiguration, string result)
        {
            JArray array = JArray.Parse(result);
            var fields = apiConfiguration.ApiFields.Select(d => d.Field);

            if (array.Any())
            {
                await MoveObjectFields(array);

                array.Descendants()
                .OfType<JProperty>()
                .Where(x => !fields.Contains(x.Name))
                .ToList()
                .ForEach(x => x.Remove());
            }

            return array;
        }

        private async Task MoveObjectFields(JArray array)
        {
            if (!array.Any())
            {
                return;
            }
            JObject item1 = (JObject)array.FirstOrDefault();
            List<string> objectFields = new List<string>();
            foreach (var property in item1.Properties())
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    objectFields.Add(property.Name);
                }
            }

            if (!objectFields.Any())
                return;

            foreach (JObject item in array)
            {
                // Process each property in the object
                var propertiesToRemove = new List<string>();

                foreach (var fieldName in objectFields)
                {
                    var property = item[fieldName];

                    // Add each property from the nested object to the root object
                    foreach (var nestedProperty in property)
                    {
                        var prop = (JProperty)nestedProperty;
                        if (prop.Value.Type == JTokenType.Object)
                            continue;

                        item[prop.Name] = prop.Value;
                    }

                    // Mark the property for removal
                    propertiesToRemove.Add(fieldName);
                }

                // Remove all nested object properties
                foreach (var propName in propertiesToRemove)
                {
                    item.Remove(propName);
                }
            }
        }

        private async Task<string> FetchDataFromApi(string url)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            var response = await _httpClient.SendAsync(request)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);
        }
    }
}