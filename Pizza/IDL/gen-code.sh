#!/bin/sh
pushd ..
thrift -gen csharp:hashcode  IDL/Pizzeria.thrift
thrift -gen csharp:hashcode  IDL/PizzaBaker.thrift
popd
