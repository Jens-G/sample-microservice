
Microservice Sample "Pizzeria"
==================

Architecture
-------------------

![](https://raw.githubusercontent.com/Jens-G/sample-microservice/master/doc/architecture.png)

The sample consists of three main pieces:
 
 * the **Pizzeria** frontend service, reachable via public Thrift API on port 9090
 * an internal backend **PizzaBaker** service, which prepares really wonderful Pizza
 * and a **Client** application which is very hungry and likes Pizza :-)
 
The fourth project is a shared assembly which contains a few tools (configuration, Cassandra connection) and a [configuration file](https://raw.githubusercontent.com/Jens-G/sample-microservice/master/CommonServiceTools/Hosts.config) used to set up the backend connections.
 

Changes in version 2
-------------------
The internal interface on port 9091 has been removed and control flow has been reverted. The frontend is still controlling 
the backends. But the work items are now actively requested by the backends, instead of getting them pushed from the frontend to the backends. That latter method just did not scale, because the frontend does not know how many PizzaBakers are actually up and running, and whether they are currently idle or not.


Dependencies
-------------------

To built and run the sample you'll need two things:

 * the [Apache Thrift](http://thrift.apache.org) C# [library bindings](https://github.com/apache/thrift) for Net 4.5
 * the [Datastax C# Cassandra driver](https://github.com/datastax/csharp-driver)
 


