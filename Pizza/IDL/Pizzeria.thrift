namespace * Pizzeria

struct Dish {
	1: string ID
	2: double Price
	3: string Description;  // may contain \n
	4: string Notes  // may contain \n
}

struct OrderPosition {
	1: i32 Quantity
	2: string DishID
}

struct Order {
	//1: string  			  ID
	2: list<OrderPosition>	Positions
}


exception EPizzeria {
	1: string Msg
}


service Pizzeria {
	list<Dish>  GetTheMenue() throws (1: EPizzeria error)
	string      PlaceOrder(1: Order order) throws (1: EPizzeria error)
	bool		CheckAndDeliver(1: string orderID) throws (1: EPizzeria error)
}


service PizzeriaCallback {
	void MealPrepared( 1: string OrderID, 2: string DishID, 3: i32 Quantity, 4: string BakerID) throws (1: EPizzeria error)
}



// EOF
