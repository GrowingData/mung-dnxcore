using System.Collections.Generic;
using GrowingData.Mung.WebTemplate.Models;

namespace GrowingData.Mung.WebTemplate.ViewModels
{
    public class ShoppingCartViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}
