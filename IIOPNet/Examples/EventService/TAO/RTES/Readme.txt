This example shows how to use the CORBA event service together with IIOP.NET.


For this example, a TAO+ACE installation is needed; 
see http://www.ece.uci.edu/~schmidt/TAO.html

Building the example:
- use the provided Makefiles


Running the example:


Starting the event supplier:
- start the tao name service
- start the tao event service
- change to the directory RTESSupplier and start the event supplier EchoEventSupplier.exe

Starting the event consumer:
- change to the directory RTESIIOPNetConsumer\bin and start RTESConsumer.exe.
HINT: enter in the host name and port field the connection information for the tao name service.



Credits:
This example is based on a contribution of SangHyun Park.
