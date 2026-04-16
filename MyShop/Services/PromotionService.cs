using MyShop.Models;
using MyShop.Data;

namespace MyShop.Services;

public interface IPromotionService
{
    Task<List<Promotion>> GetActivePromotions();
    Task<Promotion?> GetPromotionByCode(string code);
}

public class PromotionService : IPromotionService
{
    private readonly IRepository<Promotion> _repository;

    public PromotionService(IRepository<Promotion> repository)
    {
        _repository = repository;
    }

    public async Task<List<Promotion>> GetActivePromotions()
    {
        var now = DateTime.UtcNow;
        var results = await _repository.GetAsync(p =>
            p.IsActive &&
            p.StartDate <= now &&
            p.EndDate >= now);

        return results;
    }

    public async Task<Promotion?> GetPromotionByCode(string code)
    {
        var now = DateTime.UtcNow;
        var results = await _repository.GetAsync(p =>
            p.PromoCode == code &&
            p.IsActive &&
            p.StartDate <= now &&
            p.EndDate >= now);

        return results.FirstOrDefault();
    }
}
