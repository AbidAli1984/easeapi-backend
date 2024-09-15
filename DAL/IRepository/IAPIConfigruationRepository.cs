using BOL.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.IRepository
{
    public interface IAPIConfigruationRepository
    {
        Task SaveConfiguration(ApiConfiguration apiConfiguration);
        Task<ApiConfiguration> GetConfiguration(string name);
    }
}
