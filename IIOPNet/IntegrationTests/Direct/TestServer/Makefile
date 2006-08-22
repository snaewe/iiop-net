include ..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb
LAUNCH = ..\..\..\Utils\launch.exe
KILL = ..\..\..\Utils\kill.exe
DELAY = ..\..\..\Utils\delay.exe

bin         = bin\ 

build: $(bin)TestServiceCommon.dll $(bin)TestServer.exe

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
        if not exist $(bin)nul mkdir $(bin)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
	if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

$(bin)testUnion.dll: TestUnionAndConst.idl
	..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o $(bin) testUnion TestUnionAndConst.idl

$(bin)testUnionNotForClient.dll: TestUnionNotPresentClient.idl
	..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o $(bin) testUnionNotForClient TestUnionNotPresentClient.idl

$(bin)TestServiceCommon.dll: $(bin)IIOPChannel.dll $(bin)testUnion.dll TestServiceCommon.cs
    $(CSC) $(CSFLAGS) /t:library /r:$(bin)testUnion.dll /r:$(bin)IIOPChannel.dll /out:$(bin)TestServiceCommon.dll TestServiceCommon.cs

$(bin)TestServer.exe: $(bin)IIOPChannel.dll $(bin)TestServiceCommon.dll $(bin)testUnion.dll $(bin)testUnionNotForClient.dll TestServer.cs TestService.cs
    $(CSC) $(CSFLAGS) /t:exe /r:$(bin)IIOPChannel.dll /r:$(bin)testUnion.dll /r:$(bin)testUnionNotForClient.dll /r:$(bin)TestServiceCommon.dll /out:$(bin)TestServer.exe TestServer.cs TestService.cs

start-server:    
	@$(LAUNCH) bin\TestServer.exe >> pid
#	for /F "usebackq" %%r in (`..\..\..\Utils\launch.exe bin\TestServer.exe`) do set pid=%%r

stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

clean:
	if exist bin del /s /q bin
	if exist bin rmdir /s /q bin


