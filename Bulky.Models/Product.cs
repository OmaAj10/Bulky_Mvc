using System.ComponentModel.DataAnnotations;

namespace Bulky.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    public string Description { get; set; }
    [Required]
    public string ISBN { get; set; }
    [Required]
    public string Author { get; set; }
    [Required]
    [Display(Name = "List Price")]
    [Range(1, 100)]
    public double ListPrice { get; set; }

    [Display(Name = "List Price 1-50")]
    [Range(1, 100)]
    public double Price { get; set; }

    [Display(Name = "List Price 50+")]
    [Range(1, 100)]
    public double Price50 { get; set; }

    [Display(Name = "List Price 100+")]
    [Range(1, 100)]
    public double Price100 { get; set; }

}