
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


Dependencies
-------------------

To built and run the sample you'll need two things:

 * the [Apache Thrift](http://thrift.apache.org) C# [library bindings](https://github.com/apache/thrift) for Net 4.5
 * the [Datastax C# Cassandra driver](https://github.com/datastax/csharp-driver)
 
 
Repository structure
==================

General organisation
-------------------
The repo has branches, one belonging to each part of the article series. The branches are named accordingly and built logically upon each other.

 * Part 1 deals with Docker basics
 * Part 2 introduces Docker Compose and puts the Docker Swarm Mode to use
 * Part 3 moves the service into a (local) Minikube cluster and explores Kubernetes
 * Part 4 brings the service into the cloud (Azure and GKE) and/or runs it in a local cluster
 
 
Version 1 - the initial "Teil 1" branch
-------------------
Introduces the sample code and comes with some example Dockerfiles included.


Changes in version 2 (the "Teil 2" branch)
-------------------
The internal interface on port 9091 has been removed and control flow has been reverted. The frontend is still controlling 
the backends. But the work items are now actively requested by the backends, instead of getting them pushed from the frontend 
to the backends. That latter method just did not scale, because the frontend does not know how many PizzaBakers are actually 
up and running, and whether they are currently idle or not.


Changes in version 3 (the "Teil 3" branch)
-------------------
Added a "minikube" folder with four versions of the YAML files discussed in the article. Additionally, the services get readiness and liveness capabilities.


Changes in version 4 (the "Teil 4" branch)
-------------------
Swiched to HTTP port 80 (although we don't use HTTP). 
SQL Server has been added as alternative backend, because there is no Cassandra image for Windows containers. 
Better configuration due to automatic peer service discovery.
And lots of small changes, more YAML files, ...

Have fun!

