include ..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb
LAUNCH = ..\..\..\Utils\launch.exe
KILL = ..\..\..\Utils\kill.exe
DELAY = ..\..\..\Utils\delay.exe

bin         = bin\ 

build: build-idl

build-server: $(bin)TestServer.exe

build-idl: IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl IDL\Ch\Elca\Iiop\IntegrationTests\ISimpleTestInterface.idl


$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
        if not exist $(bin)nul mkdir $(bin)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
    if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb


$(bin)TestServer.exe: $(bin)IIOPChannel.dll $(bin)testIdlServices.dll *.cs
    $(CSC) $(CSFLAGS) /t:exe /r:$(bin)IIOPChannel.dll /r:$(bin)testIdlServices.dll /out:$(bin)TestServer.exe *.cs

$(bin)testIdlServices.dll: TestExceptionService.idl TestIdlTypesService.idl
    ..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o $(bin) testIdlServices TestExceptionService.idl TestIdlTypesService.idl
    
IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl: $(bin)TestServer.exe
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.TestService $(bin)TestServer.exe
    copy TestExceptionService.idl IDL\Ch\Elca\Iiop\IntegrationTests\ 
    copy TestIdlTypesService.idl IDL\Ch\Elca\Iiop\IntegrationTests\ 

IDL\Ch\Elca\Iiop\IntegrationTests\ISimpleTestInterface.idl: $(bin)TestServer.exe
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.ISimpleTestInterface $(bin)TestServer.exe

start-server:
    @$(LAUNCH) bin\TestServer.exe >> pid


stop-server:
    @for /F %%p in (pid) do @$(KILL) %%p
    @del pid

clean:
    if exist IDL del /s /q IDL
    if exist bin del /s /q bin
    if exist IDL rmdir /s /q IDL
    if exist bin rmdir /s /q bin
