GenericCollections Demo
-----------------------

This demo application demostrates how to use the IIOP.NET
channel. Under .NET, a service providing generic collections
(i.e. a list of key / value pairs) is implemented. A Java
client application accesses the collections using RMI/IIOP.

Build the project using
	nmake build

start the server:
	cd net
	net\bin\Service.exe

start the client
	cd java
	java  -Djava.naming.factory.initial=com.sun.jndi.cosnaming.CNCtxFactory \
	-Djava.naming.provider.url=iiop://localhost:8087 Client
