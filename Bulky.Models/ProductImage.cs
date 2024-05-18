using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;

namespace Bulky.Models;

public class ProductImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; }
}