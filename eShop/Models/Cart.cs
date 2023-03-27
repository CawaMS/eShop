namespace eShop.Models;

public class Cart:BaseEntity
{
    public string? BuyerId { get; private set; }
    private readonly List<CartItem> _items = new List<CartItem>();

    public int TotalItems => _items.Sum(i => i.Quantity);

    public Cart(string buyerId)
    {
        BuyerId = buyerId;
    }

    public void AddItem(int itemId, decimal unitPrice, int quantity = 1)
    {
        if (!_items.Any(i => i.ItemId == itemId))
        {
            _items.Add(new CartItem(itemId, quantity, unitPrice, this.Id));
            return;
        }
        var existingItem = _items.First(i => i.ItemId == itemId);
        existingItem.AddQuantity(quantity);
    }

    public void RemoveEmptyItems()
    {
        _items.RemoveAll(i => i.Quantity == 0);
    }

    public void SetNewBuyerId(string buyerId)
    {
        BuyerId = buyerId;
    }


}
