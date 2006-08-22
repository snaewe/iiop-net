include ..\..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\..\IIOPChannel\bin\IIOPChannel.pdb
DELAY=..\..\..\..\Utils\delay.exe
LAUNCH=..\..\..\..\Utils\launch.exe
KILL=..\..\..\..\Utils\kill.exe
bin         = bin\ 

build: $(bin)TestService.dll $(bin)TestServer.exe

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
    if not exist $(bin)nul mkdir $(bin)
    copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
    if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb


$(bin)TestService.dll: service.idl
    ..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o $(bin) TestService service.idl

$(bin)TestServer.exe: $(bin)IIOPChannel.dll $(bin)TestService.dll TestService.cs TestServer.cs
    $(CSC) $(CSFLAGS) /t:exe /r:$(bin)IIOPChannel.dll /r:$(bin)TestService.dll /out:$(bin)TestServer.exe TestServer.cs TestService.cs

start-server:    
    @$(LAUNCH) bin\TestServer.exe >> pid

stop-server:
    @for /F %%p in (pid) do @$(KILL) %%p
    @del pid

clean:
    if exist bin del /s /q bin
    if exist bin rmdir /s /q bin
