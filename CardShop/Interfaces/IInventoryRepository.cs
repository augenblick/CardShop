namespace CardShop.Interfaces
{
    public interface IInventoryRepository
    {

        Task<bool> InsertInventory(string productCode, string setCode, int count, string userId);
    }
}
