This example demonstrates register an object from a .NET server in an external name service and how to access it with a jac orb client.

For this example, you need to set the environment variable JACORB_HOME to the directory containing JacORB, e.g. C:\JacOrb

To run the example:
- use nmake to build the client and the server part
- start a nameservice; find out the corbaloc/ior for the nameservice; e.g. corbaloc::localhost:8099/NameService
- go to the Server\bin directory and start the server with AdderServer.exe corbaloc::localhost:8099/NameService
- go the the client directory and start the client with startclient.bat
- end the server by pressing enter; this unregisters the server object from the nameservice

