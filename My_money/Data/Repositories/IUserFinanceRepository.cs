using My_money.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_money.Data.Repositories
{
    public interface IUserFinanceRepository
    {
        Task<UserFinance> GetAsync();
        Task UpdateAsync(UserFinance userFinance);
    }
}
