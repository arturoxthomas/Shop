namespace Shop.Web.Data
{
    using System.Linq;
    using System.Threading.Tasks;
    using Entities;
    using Microsoft.EntityFrameworkCore;

    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly DataContext context;

        public ProductRepository(DataContext context) : base(context)
        {
            this.context = context;
        }

        public IQueryable<Product> GetAllByUserEmail(string email)
        {
            return this.context.Products
                .AsNoTracking()
                .Where(product => product.User.Email == email);
        }

        public Task<Product> GetByIdAndUserEmailAsync(int id, string email)
        {
            return this.context.Products
                .Include(product => product.User)
                .FirstOrDefaultAsync(product => product.Id == id && product.User.Email == email);
        }
    }
}
