using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository;

public class ShoppingRepository : Repository<ShoppingCart>, IShoppingCartRepository
{
    private ApplicationDbContext _db;

    public ShoppingRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(ShoppingCart obj)
    {
        _db.ShoppingCarts.Update(obj);
    }
    
}