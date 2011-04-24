include ..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
SSLPLUGIN = ..\..\..\SSLPlugin\bin\SslPlugin.dll 
SSLPLUGINPDB = ..\..\..\SSLPlugin\bin\SslPlugin.pdb 
MENTALIS = ..\..\..\SSLPlugin\bin\Org.Mentalis.Security.dll
MENTALISPDB = ..\..\..\SSLPlugin\bin\Org.Mentalis.Security.pdb

bin         = bin\ 

build: build-client

build-client: $(bin)ClientTest.dll

compile-idl: IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl


IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl: ..\TestServer\IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl
    if not exist IDL\nul mkdir IDL
    cd IDL
    copy ..\..\..\..\IDLToCLSCompiler\IDL\*.idl .
    xcopy /s /q /y ..\..\TestServer\IDL .
    cd ..

$(bin)Client.dll: IDL\Ch\Elca\Iiop\IntegrationTests\TestService.idl 
    cd IDL 
    ..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\bin client Ch\Elca\Iiop\IntegrationTests\TestService.idl 
    cd ..

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

$(bin)ClientTest.dll: $(bin)IIOPChannel.dll $(bin)SslPlugin.dll $(bin)Org.Mentalis.Security.dll $(bin)Client.dll *.cs
    $(CSC) $(CSFLAGS) /t:library /r:$(NUNIT_FRAMEWORK_DLL) /r:$(bin)IIOPChannel.dll /r:$(bin)SSLPlugin.dll /r:$(bin)Client.dll /out:$(bin)ClientTest.dll TestClient.cs
    copy $(NUNIT_FRAMEWORK_DLL) $(bin)


start-client:
    @echo "Codeset IntegrationTest"
    $(NUNIT_CONSOLE) $(NUNIT_ASSEMBLY_ARG_OPTION)bin\ClientTest.dll

clean:
    if exist IDL del /s /q IDL
    if exist bin del /s /q bin
    if exist IDL rmdir /s /q IDL
    if exist bin rmdir /s /q bin

