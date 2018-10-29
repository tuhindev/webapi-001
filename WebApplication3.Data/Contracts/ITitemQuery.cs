using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication3.Data
{
    public interface ITitemQuery
    {
        Task<IEnumerable<TitemData>> GetAll();

        Task<dynamic> GetSingle(int id);

        Task<IEnumerable<int>> Insert(IEnumerable<TitemData> data);

        Task<int> Insert(TitemData data);
    }
}
