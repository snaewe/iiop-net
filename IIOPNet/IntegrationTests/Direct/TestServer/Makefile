IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb
LAUNCH = ..\..\Utils\launch.exe
KILL = ..\..\Utils\kill.exe
DELAY = ..\..\Utils\delay.exe

bin         = bin\ 

build: build-union build-servicecommon build-server

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(bin) $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
	if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb


build-union: TestUnionAndConst.idl
	..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o $(bin) testUnion TestUnionAndConst.idl
	..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o $(bin) testUnionNotForClient TestUnionNotPresentClient.idl


build-servicecommon: $(bin)IIOPChannel.dll TestServiceCommon.cs
    csc $(CSFLAGS) /t:library /r:$(bin)testUnion.dll /r:$(bin)IIOPChannel.dll /out:$(bin)TestServiceCommon.dll TestServiceCommon.cs


build-server: $(bin)IIOPChannel.dll build-servicecommon TestServer.cs TestService.cs
    csc $(CSFLAGS) /t:exe /r:$(bin)IIOPChannel.dll /r:$(bin)testUnion.dll /r:$(bin)testUnionNotForClient.dll /r:$(bin)TestServiceCommon.dll /out:$(bin)TestServer.exe TestServer.cs TestService.cs

start-server:    
	@$(LAUNCH) bin\TestServer.exe >> pid
#	for /F "usebackq" %%r in (`..\..\Utils\launch.exe bin\TestServer.exe`) do set pid=%%r

stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

clean:
	if exist bin del /s /q bin
	if exist bin rmdir /s /q bin


