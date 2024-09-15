using BOL.DataModel;
using DAL.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Xml.Linq;

namespace DAL.Repository
{
    public class APIConfigruationRepository : IAPIConfigruationRepository
    {
        private const string _apiConfigurationCache = "ApiConfigurations";
        private readonly AppDbContext _appDbContext;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions;

        public APIConfigruationRepository(AppDbContext appDbContext, IMemoryCache cache)
        {
            _appDbContext = appDbContext;
            _cache = cache;
            _cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
        }

        public async Task SaveConfiguration(ApiConfiguration apiConfiguration)
        {
            await _appDbContext.ApiConfigurations.AddAsync(apiConfiguration);
            await _appDbContext.SaveChangesAsync();
            SaveConfigurationInCache();
        }

        public async Task<ApiConfiguration> GetConfiguration(string name)
        {
            var apiConfiguration = (List<ApiConfiguration>)_cache.Get(_apiConfigurationCache);

            if (apiConfiguration == null)
                apiConfiguration = SaveConfigurationInCache();

            return apiConfiguration.FirstOrDefault(x => x.Name == name);
        }


        #region Helper Method
        private List<ApiConfiguration> SaveConfigurationInCache()
        {
            var cachedData = _appDbContext.ApiConfigurations.Include(x => x.ApiFields).ToList();
            _cache.Set(_apiConfigurationCache, cachedData, _cacheEntryOptions);
            return cachedData;
        }
        #endregion
    }
}
