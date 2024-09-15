using BAL.IServices;
using BOL.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text.Json;

namespace EaseapiBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EaseAPIController : ControllerBase
    {
        private readonly IAPIConfigurationService _apiConfigurationService;
        private readonly IMemoryCache _cache;
        public EaseAPIController(IAPIConfigurationService apiConfigurationService, IMemoryCache cache)
        {
            _apiConfigurationService = apiConfigurationService;
            _cache = cache;
        }

        [Route("AddApiConfiguration")]
        [HttpPost]
        public async Task<ActionResult> AddConfiguration(APIConfigurationResponse configurationAPI)
        {
            var isAdded = await _apiConfigurationService.SaveConfiguration(configurationAPI);
            if (!isAdded)
            {
                return BadRequest("Configuration with same name already exists!");
            }
            return Ok();
        }

        [Route("GetInformation")]
        [HttpGet]
        public async Task<ActionResult> GetInformation(string name)
        {

            string response = await _apiConfigurationService.ProcessRequest(name);
            return Content(response, "application/json;");
        }
    }
}
