using My_money.Model;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IFinancialHealthScoreService
    {
        Task<int> GetFinancialHealthScoreAsync(HouseholdMember member);
    }
}
