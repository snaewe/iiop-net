IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb
bin         = bin\ 

build: build-servicecommon build-server

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(bin) $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
	if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb


build-servicecommon: $(bin)IIOPChannel.dll TestServiceCommon.cs
    csc $(CSFLAGS) /t:library /r:$(bin)IIOPChannel.dll /out:$(bin)TestServiceCommon.dll TestServiceCommon.cs


build-server: $(bin)IIOPChannel.dll build-servicecommon TestServer.cs TestService.cs
    csc $(CSFLAGS) /t:exe /r:$(bin)IIOPChannel.dll /r:$(bin)TestServiceCommon.dll /out:$(bin)TestServer.exe TestServer.cs TestService.cs
    

clean:
	if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
