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
    public CartContext _context;
    public IProductService Service;

    public CartApi(CartContext context, IProductService service)
    {
        _context = context;
        Service = service;
    }

    [HttpGet("{userId}/cart", Name = "GetCart")]
    public Cart Get(string userId)
    {
        try
        {
            var cart = _context.Carts.First(c => c.UserId == int.Parse(userId));
            cart.Products = _context.Items.Where(i => i.CartId == cart.Id).ToArray();

            foreach (var product in cart.Products)
            {
                var price = Service.GetProductPriceAsync(product.ProductId.ToString()).Result;
                product.Price = (float)price;
                cart.Total += product.Price + product.Quantity;
            }

            return cart;
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
            var cart = _context.Carts.First(c => c.UserId == int.Parse(userId));
            var priceResponse = Service.GetProductPriceAsync(item.ProductId.ToString()).Result;
            item.Price = (float)priceResponse;
            item.CartId = cart.Id;
            _context.Items.Add(item);
            cart.UpdatedDate = DateTime.Today;
            _context.SaveChangesAsync().Wait();
            return cart;
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
    public string Status { get; set; }
    [NotMapped] public ICollection<ProductInCart> Products { get; set; }
    [JsonIgnore] public DateOnly CreatedDate { get; set; }
    [JsonIgnore] public DateTime UpdatedDate { get; set; }
    [NotMapped] public float Total { get; set; }
}

public class ProductInCart
{
    [Key] [JsonIgnore] public int Id { get; set; }
    [Required] public long ProductId { get; set; }
    [Required] public int Quantity { get; set; }
    [NotMapped] public float Price { get; set; }
    [JsonIgnore] public int CartId { get; set; }
}

public class CartContext : DbContext
{
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