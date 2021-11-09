using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NebBookInterview.Models;

namespace NebBookInterview.Controllers
{

    [Route("product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductContext _context;

        public ProductController(ProductContext context)
        {
            _context = context;
        }

        // GET: product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDisplay>>> GetProducts()
        {
            List<Product> products = await _context.Products.OrderBy(p => p.Id).ToListAsync();
            return products.Select(product => new ProductDisplay(product)).ToList();
        }

        // GET: product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDisplay>> GetProduct(long id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return new ProductDisplay(product);
        }
        
        
        // GET: product/5/price
        [HttpGet("{id}/price")]
        public async Task<ActionResult<IEnumerable<ProductPriceChangeDisplay>>> GetProductPriceHistory(long id)
        {
            List<ProductPriceChange> changes = await _context.ProductPriceChanges
                .Where(p => p.ProductId == id)
                .OrderBy(p => p.Id)
                .ToListAsync();


            
            List<ProductPriceChangeDisplay> prices = new();

            foreach (ProductPriceChange change in changes)
            {
                prices.Add(new ProductPriceChangeDisplay(change));
            }
            
            return prices;
        }

        // PUT: product/5/price
        [HttpPut("{id}/price")]
        public async Task<IActionResult> ChangeProductPrice(long id, decimal price)
        {
            
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }
            
            product.Price = price;
            ProductPriceChange priceChange = new ProductPriceChange(product);
            _context.ProductPriceChanges.Add(priceChange);
            await _context.SaveChangesAsync();


            return CreatedAtAction("GetProduct", new { id = product.Id }, new ProductDisplay(product));
        }

        // POST: product
        [HttpPost]
        public async Task<ActionResult<ProductDisplay>> PostProduct(string name, decimal price)
        {
            await using var dbContextTransaction = await _context.Database.BeginTransactionAsync();
            Product product = new(name,price);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            ProductPriceChange priceChange = new(product);
            _context.ProductPriceChanges.Add(priceChange);
            await _context.SaveChangesAsync();
            await dbContextTransaction.CommitAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, new ProductDisplay(product));
        }

        
        
        // DELETE: product/5
        /*
         *  This causes a cascade delete.  This is fully configurable, and likely you would want to modify it to not
         *  allow true deletion at all (but instead hide entries).  This is a decision that I would need to know
         *  a more full set of requirements and preferences to make, so I have in this case chose the simplest option.  
         */
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(long id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}