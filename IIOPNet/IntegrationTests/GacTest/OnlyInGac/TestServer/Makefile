include ..\..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\..\IIOPChannel\bin\IIOPChannel.pdb
LAUNCH = ..\..\..\..\Utils\launch.exe
KILL = ..\..\..\..\Utils\kill.exe
DELAY = ..\..\..\..\Utils\delay.exe

bin         = bin\ 
binTmp      = binTmp\ 

build: build-idl

build-server: $(bin)TestServer.exe

build-idl: IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl IDL\Ch\Elca\Iiop\IntegrationTests\ISimpleTestInterface.idl


$(bin)TestServer.exe: $(binTmp)testIdlServices.dll *.cs
	if not exist $(bin)nul mkdir $(bin)
	$(CSC) $(CSFLAGS) /t:exe /r:$(IIOPCHANNEL) /r:$(binTmp)testIdlServices.dll /out:$(bin)TestServer.exe *.cs

$(binTmp)testIdlServices.dll: TestExceptionService.idl TestBoxedValueTypeService.idl
	..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o $(binTmp) -snk ..\..\..\..\Key.snk testIdlServices TestExceptionService.idl TestBoxedValueTypeService.idl

    
IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl: $(bin)TestServer.exe
	..\..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.TestService $(bin)TestServer.exe
	copy TestExceptionService.idl IDL\Ch\Elca\Iiop\IntegrationTests\ 
	copy TestBoxedValueTypeService.idl IDL\Ch\Elca\Iiop\IntegrationTests\ 

IDL\Ch\Elca\Iiop\IntegrationTests\ISimpleTestInterface.idl: $(bin)TestServer.exe
	..\..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.ISimpleTestInterface $(bin)TestServer.exe

start-server: install-gac
	@$(LAUNCH) bin\TestServer.exe >> pid

install-gac: $(IIOPCHANNEL)
	$(GACUTIL) /i $(IIOPCHANNEL)
	$(GACUTIL) /i $(binTmp)testIdlServices.dll

stop-server: uninstall-gac
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

uninstall-gac:
	$(GACUTIL) /u testIdlServices
	$(GACUTIL) /u IiopChannel


clean: uninstall-gac
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin
	if exist $(binTmp) rmdir /s /q $(binTmp)
