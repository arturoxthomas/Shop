namespace Shop.Web.Data
{
    using System.Linq;
    using System.Threading.Tasks;
    using Entities;

    public interface IProductRepository : IGenericRepository<Product>
    {
        IQueryable<Product> GetAllByUserEmail(string email);

        Task<Product> GetByIdAndUserEmailAsync(int id, string email);
    }
}
