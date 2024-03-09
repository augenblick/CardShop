using CardShop.Interfaces;

namespace CardShop.Logic
{
    

    public class InventoryManager
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryManager(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public bool InsertInventory()
        {
            // TODO
            return false;
        }
    }
}
