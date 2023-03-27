using System;

namespace eShop.Models;

public class CartItem : BaseEntity
{
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public int ItemId { get; private set; }
    public int CartId { get; private set; }

    public CartItem(int itemId, int quantity, decimal unitPrice)
    {
        ItemId = itemId;
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    public void AddQuantity(int quantity)
    {
        Quantity += quantity;
    }

    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
    }

}
