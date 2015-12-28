using System.Collections.Generic;
using GrowingData.Mung.Web.Models;

namespace GrowingData.Mung.Web.ViewModels
{
    public class ShoppingCartViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}
