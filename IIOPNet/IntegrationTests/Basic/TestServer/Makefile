IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb
bin         = bin\ 

build: build-server build-idl

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
	if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

build-server: $(bin)IIOPChannel.dll *.cs
    csc $(CSFLAGS) /t:exe /r:$(bin)IIOPChannel.dll /out:$(bin)TestServer.exe TestService.cs TestServer.cs
    

build-idl:
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.TestService bin\testServer.exe

