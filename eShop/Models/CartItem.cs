using System;

namespace eShop.Models;

[Serializable]
public class CartItem 
{
    public int Id { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int ItemId { get; set; }
    public string? CartId { get; set; }

    public CartItem(int itemId, int quantity, decimal unitPrice)
    {
        ItemId = itemId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void AddQuantity(int quantity)
    {
        Quantity += quantity;
    }


}
