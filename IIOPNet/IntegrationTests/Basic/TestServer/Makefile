
build: build-server build-idl


build-server:
    csc /t:exe /r:bin\IIOPChannel.dll /out:bin\TestServer.exe TestService.cs TestServer.cs
    

build-idl:
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.TestService bin\testServer.exe

