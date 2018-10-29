using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication3.Data
{
    public class TitemQuery : ITitemQuery
    {
 //       private string _connection;
        private int offset = 10;
        public TitemQuery()
        {
          //  this._connection = connection;
        }
        public async Task<IEnumerable<TitemData>> GetAll()
        {
            using (IDbConnection conn = this.GetDbConnection())
            {
                string sQuery = "select * from TitemData order by Id desc OFFSET @offset ROWS FETCH NEXT @offset ROWS ONLY; ";
                conn.Open();
                return await conn.QueryAsync<TitemData>(sQuery, new { ID = 1 });
            }
        }

        public async Task<dynamic> GetSingle(int id)
        {
            using (IDbConnection conn = this.GetDbConnection())
            {
                string sQuery = "SELECT Id,Name,StartDate,Points FROM TitemData WHERE Id = @ID";
                conn.Open();
                return await conn.QueryFirstAsync<dynamic>(sQuery, new { ID = id });
            }
        }

        public Task<IEnumerable<int>> Insert(IEnumerable<TitemData> data)
        {
            throw new NotImplementedException();
        }

        public Task<int> Insert(TitemData data)
        {
            throw new NotImplementedException();
        }

        public IDbConnection GetDbConnection()
        {
            return new SqlConnection("Data Source=sqldb;Initial Catalog=MyDb;User id=sa;Password=Your_p@ssword123");
        }
    }
}
