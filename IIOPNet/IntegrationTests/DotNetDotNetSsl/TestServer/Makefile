include ..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb
SSLPLUGIN = ..\..\..\SSLPlugin\bin\SslPlugin.dll 
SSLPLUGINPDB = ..\..\..\SSLPlugin\bin\SslPlugin.pdb 
MENTALIS = ..\..\..\SSLPlugin\bin\Org.Mentalis.Security.dll
MENTALISPDB = ..\..\..\SSLPlugin\bin\Org.Mentalis.Security.pdb 

LAUNCH = ..\..\..\Utils\launch.exe
KILL = ..\..\..\Utils\kill.exe
DELAY = ..\..\..\Utils\delay.exe

bin         = bin\ 

build: build-idl

build-server: $(bin)TestServer.exe

build-idl: IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl 

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL) $(bin)
	copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
	if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

$(bin)SslPlugin.dll: $(bin) $(SSLPLUGIN)
        copy /y $(SSLPLUGIN) $(bin)SslPlugin.dll
        if exist $(SSLPLUGINPDB) copy /y $(SSLPLUGINPDB) $(bin)SslPlugin.pdb

$(bin)Org.Mentalis.Security.dll: $(bin) $(MENTALIS)
        copy /y $(MENTALIS) $(bin)Org.Mentalis.Security.dll
        if exist $(MENTALISPDB) copy /y $(MENTALISPDB) $(bin)Org.Mentalis.Security.pdb


$(bin)TestServer.exe: $(bin)IIOPChannel.dll $(bin)SslPlugin.dll $(bin)Org.Mentalis.Security.dll *.cs
    $(CSC) $(CSFLAGS) /t:exe /r:$(bin)IIOPChannel.dll /r:$(bin)SSLPlugin.dll /out:$(bin)TestServer.exe *.cs

   
IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl: $(bin)TestServer.exe
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.TestService $(bin)TestServer.exe

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
