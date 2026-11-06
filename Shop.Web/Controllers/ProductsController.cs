namespace Shop.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Shop.Web.Data;
    using Shop.Web.Data.Entities;
    using Shop.Web.Helpers;

    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IProductRepository productRepository;
        private readonly IUserHelper userHelper;

        public ProductsController(IProductRepository productRepository, IUserHelper userHelper)
        {
            this.productRepository = productRepository;
            this.userHelper = userHelper;
        }

        public IActionResult Index()
        {
            return View(this.productRepository.GetAllByUserEmail(User.Identity.Name));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await this.productRepository.GetByIdAndUserEmailAsync(id.Value, User.Identity.Name);
            if (product == null) return NotFound();

            return View(product);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            var currentUser = await this.userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (currentUser == null) return Challenge();

            product.User = currentUser;
            await this.productRepository.CreateAsync(product);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await this.productRepository.GetByIdAndUserEmailAsync(id.Value, User.Identity.Name);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product input)
        {
            if (id != input.Id) return NotFound();
            if (!ModelState.IsValid) return View(input);

            var product = await this.productRepository.GetByIdAndUserEmailAsync(id, User.Identity.Name);
            if (product == null) return NotFound();

            product.Name = input.Name;
            product.Price = input.Price;
            product.ImageUrl = input.ImageUrl;
            product.LastPurchase = input.LastPurchase;
            product.LastSale = input.LastSale;
            product.IsAvailabe = input.IsAvailabe;
            product.Stock = input.Stock;

            try
            {
                await this.productRepository.UpdateAsync(product);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await this.productRepository.ExistAsync(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await this.productRepository.GetByIdAndUserEmailAsync(id.Value, User.Identity.Name);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await this.productRepository.GetByIdAndUserEmailAsync(id, User.Identity.Name);
            if (product == null) return NotFound();

            await this.productRepository.DeleteAsync(product);
            return RedirectToAction(nameof(Index));
        }
    }
}
