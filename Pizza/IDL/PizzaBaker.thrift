namespace * PizzaBaker



exception EPizzaBaker {
	1: string Msg
}


service PizzaBaker {
	string GetID() throws (1: EPizzaBaker error)
	bool PrepareMeal( 1: string OrderID, 2: string DishID, 3: i32 Quantity) throws (1: EPizzaBaker error)
}


// EOF
