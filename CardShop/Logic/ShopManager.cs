using CardShop.Interfaces;
using CardShop.Models;
using CardShop.Repositories.Models;
using System.Reflection.Metadata.Ecma335;

namespace CardShop.Logic
{
    public class ShopManager : IShopManager
    {
        private readonly ICardProductBuilder _cardProductBuilder;
        private readonly List<Product> _inventory = new List<Product>();
        private const string _userName = "ShopKeeper";
        private const int _userId = 0;

        public ShopManager(ICardProductBuilder cardProductBuilder)
        {
            _cardProductBuilder = cardProductBuilder;
        }

        public void Initialize()
        {
            // TODO
            var _inventory = (_cardProductBuilder.GetProducts(ProductType.BoosterBox, Enums.CardSetCode.Premiere, 5));



        }

        public List<Inventory> GetShopInventory()
        {
            // TODO
            //return _inventoryRepository.GetFullInventoryByUserId(_userId);
            return new List<Inventory>();
        }
    }
}
