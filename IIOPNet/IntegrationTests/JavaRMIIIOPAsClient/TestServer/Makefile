include ..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb
LAUNCH = ..\..\..\Utils\launch.exe
KILL = ..\..\..\Utils\kill.exe
DELAY = ..\..\..\Utils\delay.exe

bin         = bin\ 

build: build-server build-idl

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(bin) $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
	if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

build-server: $(bin)IIOPChannel.dll *.cs
    $(CSC) $(CSFLAGS) /t:exe /r:$(bin)IIOPChannel.dll /out:$(bin)TestServer.exe TestService.cs TestServer.cs
    

build-idl:
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.TestService bin\testServer.exe


start-server:
	@$(LAUNCH) bin\TestServer.exe >> pid
#	for /F "usebackq" %%r in (`..\..\..\Utils\launch.exe bin\TestServer.exe`) do set pid=%%r

stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

clean:
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin
