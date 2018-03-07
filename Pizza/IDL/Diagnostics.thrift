namespace * Diagnostics

exception EDiagnostics {
	1: string Msg
}


service Diagnostics {
	i64  PerformanceTest(1: i32 seconds) throws (1: EDiagnostics error)
}



// EOF
