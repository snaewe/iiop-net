Getting started tutorials
-------------------------

SimpleAdder
-----------

The SimpleAdder application demostrates how to use the IIOP.NET
channel, the CLSToIDLGenerator and the IDLToCLSCompiler. 

On the server side, an adder object provides adding functionaity. 
A client accesses this adder object to add two doubles.

The SimpleAdder\DotNetServerJavaClient directory contains a tutorial showing how to access a
.NET MarshalByRefObject using a java client.

The SimpleAdder\DotNetServerDotNetClient directory contains a tutorial showing how to access a
.NET MarshalByRefObject using a .net client (via iiop).


The SimpleAdder\JavaServerDotNetClient directory contains a tutorial showing how to access a
java RMI object using a .NET client.

The SimpleAdder Tutorials can be built, using
	cd SimpleAdder
	nmake build

The DotNetServerJavaClient Tutorial can be run using
	cd DotNetServerJavaClient
	cd net
	startServer

	cd ..
	cd java
	runClient

The JavaServerDotNetClient Tutorial can be run using
	cd JavaServerDotNetClient
	cd java
	startServer

	cd ..
	cd net
	runClient
		
The DotNetServerDotNetClient Tutorial can be run using
	cd DotNetServerDotNetClient
	cd server
	startServer

	cd ..
	cd directClient
	runClient

or

	cd ..
	cd idlClient
	runClient

The DirectClient uses directly a common dll to retrieve the adder interfrace from.
The IDLClient uses idl to generate the adder interface from.





