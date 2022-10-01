using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Valnet.Cart.API.Controllers;

[ApiController]
[Route("[controller]")]
public class CartApi : ControllerBase
{
   
    public ICartService _service;

    public CartApi(ICartService service)
    {
        _service = service;
    }

    [HttpGet("{userId}/cart", Name = "GetCart")]
    public Cart GetCartDetails(string userId)
    {
        try
        {
            return _service.GetCartDetails(userId);
        }
        catch
        {
            Log.Information("Something went wrong!");
            throw new ApplicationException("Something went wrong, please try again later!");
        }
    }

    [HttpPost("{userId}/cart/items", Name = "Add Or Update Product And Its Qty")]
    public Cart AddOrUpdate(string userId, ProductInCart item)
    {
        try
        {
            return _service.AddOrUpdate(userId, item);
        }
        catch
        {
            Log.Debug("Something went wrong!");
            throw new ApplicationException("Something went wrong, please try again later!");
        }
    }

    [HttpGet("import")]
    public string InitialDataLoad()
    {
        try
        {
            _service.InitialDataLoad();
            return "Success";
        }
        catch
        {
            Log.Debug("Something went wrong!");
            throw new ApplicationException("Something went wrong, please try again later!");
        }
    }
}

public class Cart
{
    [Key] [JsonIgnore] public int Id { get; set; }
    [JsonIgnore] public int UserId { get; set; }
    [NotMapped] public ICollection<ProductInCart> Products { get; set; }
    [JsonIgnore] public DateTime UpdatedDate { get; set; }
    [NotMapped] public float Total { get; set; }
}

public class ProductInCart
{
    [Key] [JsonIgnore] public int Id { get; set; }
    [Required] public long ProductId { get; set; }
    [Required] public int Quantity { get; set; }
     public decimal Price { get; set; }
    [JsonIgnore] public int CartId { get; set; }
}

public class CartContext : DbContext
{
    protected override void OnConfiguring
        (DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(databaseName: "ShoppingCart");
    }
    public DbSet<ProductInCart> Items { get; set; }
    public DbSet<Cart> Carts { get; set; }
}

/// <summary>
/// This service is a part os SDK provided by a different team
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Performs http call to another API endpoint
    /// </summary>
    /// <returns>
    /// Price of the product.
    /// Argument Exception if product is not found/available/etc.
    /// </returns>
    public Task<decimal> GetProductPriceAsync(string productId);
}

public class ProductService : IProductService
{
    public CartContext _context;
    public ProductService(CartContext context)
    {
        _context = context;
    }
    public Task<decimal> GetProductPriceAsync(string productId)
    {
      var price = _context.Items.Where(i => i.ProductId == long.Parse(productId)).FirstOrDefault().Price;
      return Task.FromResult(price);
    }
}

public interface ICartService
{
    public Cart GetCartDetails(string userId);
    public Cart AddOrUpdate(string userId, ProductInCart item);

    public void InitialDataLoad();
}

public class CartService : ICartService
{
    public CartContext _context;
    public IProductService _productService;
    public CartService(CartContext context, IProductService productService)
    {

        _context = context;
        _productService = productService;

    }

    public void InitialDataLoad()
    {
        var carts = new List<Cart>
            {
                new Cart
                {
                    Id = 1,
                    UserId=1,
                    UpdatedDate=DateTime.Now,
                    Total=0,
                    Products = new List<ProductInCart>()
                    {
                        new ProductInCart { Id=1,CartId=1,ProductId=11707,Price=10,Quantity=2},
                        new ProductInCart { Id=2,CartId=1,ProductId=78040,Price=5,Quantity=3},
                        new ProductInCart { Id=3,CartId=1,ProductId=24989,Price=15,Quantity=7}
                    }

                }
            };
        var items = new List<ProductInCart>()
                    {
                        new ProductInCart { Id=1,CartId=1,ProductId=11707,Price=10,Quantity=2},
                        new ProductInCart { Id=2,CartId=1,ProductId=78040,Price=5,Quantity=3},
                        new ProductInCart { Id=3,CartId=1,ProductId=24989,Price=15,Quantity=7}
                    };
        _context.Carts.AddRange(carts);
        _context.Items.AddRange(items);
        _context.SaveChanges();
    }

    public Cart GetCartDetails(string userId)
    {

             var cart = _context.Carts.First(c => c.UserId == int.Parse(userId));
            cart.Products = _context.Items.Where(i => i.CartId == cart.Id).ToArray();

            foreach (var product in cart.Products)
            {
                var price = _productService.GetProductPriceAsync(product.ProductId.ToString()).Result;
                product.Price = price;
                cart.Total += (float)product.Price * product.Quantity;
            }

            return cart;

    }

    public Cart AddOrUpdate(string userId, ProductInCart item)
    {
        var cart = _context.Carts.FirstOrDefault(c => c.UserId == int.Parse(userId));
        
        if (_context.Items.Any(p => p.ProductId == item.ProductId))
        {
            var priceResponse = _productService.GetProductPriceAsync(item.ProductId.ToString()).Result;
            item.Price = priceResponse;
        }
        else
        {
            _context.Items.Add(new ProductInCart
            { CartId = 1, Id = _context.Items.Count()+1, Quantity = item.Quantity, ProductId = item.ProductId, Price=new Random().Next(100) });
        }
        cart.UpdatedDate = DateTime.Today;
        _context.SaveChangesAsync().Wait();
        cart.Products = _context.Items.ToArray();
        return cart;
    }
}