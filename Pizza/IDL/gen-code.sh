#!/bin/sh
pushd ..
thrift --version
thrift -gen csharp:hashcode  IDL/Pizzeria.thrift
thrift -gen csharp:hashcode  IDL/Diagnostics.thrift
popd
