include ..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
SSLPLUGIN = ..\..\..\SSLPlugin\bin\SslPlugin.dll 
SSLPLUGINPDB = ..\..\..\SSLPlugin\bin\SslPlugin.pdb 
MENTALIS = ..\..\..\SSLPlugin\bin\Org.Mentalis.Security.dll
MENTALISPDB = ..\..\..\SSLPlugin\bin\Org.Mentalis.Security.pdb 

bin         = bin\ 
BEANPATH    = ch\elca\iiop\integrationTests\ 

build: copy-idl compile-idl build-client

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(bin) $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
        if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

$(bin)SslPlugin.dll: $(bin) $(SSLPLUGIN)
        copy /y $(SSLPLUGIN) $(bin)SslPlugin.dll
        if exist $(SSLPLUGINPDB) copy /y $(SSLPLUGINPDB) $(bin)SslPlugin.pdb

$(bin)Org.Mentalis.Security.dll: $(bin) $(MENTALIS)
        copy /y $(MENTALIS) $(bin)Org.Mentalis.Security.dll
        if exist $(MENTALISPDB) copy /y $(MENTALISPDB) $(bin)Org.Mentalis.Security.pdb


build-client: $(bin) $(bin)IIOPChannel.dll $(bin)SslPlugin.dll $(bin)Org.Mentalis.Security.dll *.cs
	$(CSC) $(CSFLAGS) /t:library /r:$(NUNIT_FRAMEWORK_DLL) /r:$(bin)SSLPlugin.dll /r:$(bin)IIOPChannel.dll /r:$(bin)Client.dll /out:$(bin)ClientTest.dll TestClient.cs ValueTypeImpls.cs ExceptionImpl.cs

copy-idl:
    if not exist IDL\nul mkdir IDL
    cd IDL
    copy ..\..\..\..\IDLToCLSCompiler\IDL\*.idl .
    xcopy /s /q /y ..\..\TestBeans\IDL .
    cd ..

compile-idl:
	cd IDL 
	..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\bin client $(BEANPATH)Test.idl $(BEANPATH)TestHome.idl $(BEANPATH)IntAdder.idl $(BEANPATH)IntAdderHome.idl
	cd ..

start-client:
        @echo "JBoss IntegrationTest"
        $(NUNIT_CONSOLE) $(NUNIT_ASSEMBLY_ARG_OPTION)bin\ClientTest.dll

clean:
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin
	