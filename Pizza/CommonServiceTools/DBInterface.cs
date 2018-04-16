using Pizzeria;

namespace DB
{
    public interface IDbSession
    {
        void EnsureKeyspaceAndTables();
        string PlaceOrder(Order order);
        WorkItem GetSomeWork(string BakerID);
        void MealPrepared(string OrderID, string DishID, int Quantity, string BakerID);
        bool CheckAndDeliver(string orderID);
    }

    public interface IDbAdapter
    {
        IDbSession CreateSession();
    }

}
