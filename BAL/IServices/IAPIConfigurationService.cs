using BOL.DTO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.IServices
{
    public interface IAPIConfigurationService
    {
        Task<bool> SaveConfiguration(APIConfigurationResponse configurationAPI);
        Task<string> ProcessRequest(string name);
    }
}
