using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MagazinOnline.Data;
using MagazinOnline.Models;
using Microsoft.AspNetCore.Hosting;
using MagazinOnline.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace MagazinOnline.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();

            if (User.IsInRole("Admin"))
            {
                return View("AdminIndex", products); 
            }

            return View("ClientIndex", products); 
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Calculăm media ratingurilor și numărul total de recenzii
            ViewBag.AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0;
            ViewBag.TotalReviews = product.Reviews.Count;
            ViewBag.Reviews = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList();

            return View("ClientDetails", product);
        }



        // GET: Products/Create

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel productViewModel)
        {
            if (!ModelState.IsValid)
            {
                // Debugging: Afișează erorile ModelState în consola Visual Studio
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($" Eroare ModelState: {error.ErrorMessage}");
                }
                return View(productViewModel);
            }

            var product = new Product
            {
                Name = productViewModel.Name,
                Description = productViewModel.Description,
                Price = productViewModel.Price,
                Stock = productViewModel.Stock
            };

            // Dacă utilizatorul a încărcat o imagine, o salvăm
            if (productViewModel.Image != null)
            {
                product.ImageUrl = UploadFile(productViewModel);
            }
            else
            {
                product.ImageUrl = "default.png"; // Imagine implicită dacă nu este încărcată nicio imagine
            }

            _context.Add(product);
            await _context.SaveChangesAsync();
            Console.WriteLine(" Produs creat cu succes!");

            return RedirectToAction(nameof(Index));
        }


        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var productViewModel = new ProductViewModel
            {
                Id = product.Id, // Trimite ID-ul pentru a ști ce produs edităm
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ExistingImage = product.ImageUrl // Salvăm imaginea existentă
            };

            return View(productViewModel);
        }

        // POST: Products/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel productViewModel)
        {
            if (id != productViewModel.Id)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.Name = productViewModel.Name;
                    product.Description = productViewModel.Description;
                    product.Price = productViewModel.Price;
                    product.Stock = productViewModel.Stock;

                    // Dacă utilizatorul a încărcat o nouă imagine, o salvăm
                    if (productViewModel.Image != null)
                    {
                        product.ImageUrl = UploadFile(productViewModel);
                    }
                    else
                    {
                        product.ImageUrl = productViewModel.ExistingImage; // Păstrăm imaginea existentă
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productViewModel);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5\
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        // Metodă pentru încărcarea fișierului
        private string UploadFile(ProductViewModel productViewModel)
        {
            string fileName = null;

            if (productViewModel.Image != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(productViewModel.Image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException("Tip de fișier nepermis. Doar JPG, PNG și GIF sunt acceptate.");
                }

                if (productViewModel.Image.Length > 2 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Dimensiunea fișierului nu trebuie să depășească 2MB.");
                }

                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                fileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadDir, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    productViewModel.Image.CopyTo(fileStream);
                }
            }

            return fileName;
        }

    }
}
