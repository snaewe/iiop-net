NUNIT=$(NUNITV2_HOME)\bin\nunit.framework.dll 
IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
MAPPINGPLUGINDIR=..\..\..\MappingPlugin\java\ 
bin         = bin\ 

build: build-plugin copy-idl compile-idl build-client

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(bin) $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
        if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

$(bin)javaCollections.dll: $(bin) $(MAPPINGPLUGINDIR)\bin\javaCollections.dll
	copy /y $(MAPPINGPLUGINDIR)bin\javaCollections.* $(bin)
	copy /y $(MAPPINGPLUGINDIR)bin\_javaCollections*.netmodule $(bin)

$(bin)customCollectionMappings.dll: $(bin) $(MAPPINGPLUGINDIR)\bin\customCollectionMappings.dll
	copy /y $(MAPPINGPLUGINDIR)bin\customCollectionMappings.* $(bin)

$(bin)customMapping.xml: $(bin) $(MAPPINGPLUGINDIR)customMapping.xml
	copy /y $(MAPPINGPLUGINDIR)customMapping.xml $(bin)
	

build-client: $(bin)IIOPChannel.dll $(bin)customCollectionMappings.dll $(bin)customMapping.xml $(bin)javaCollections.dll *.cs
	csc $(CSFLAGS) /t:library /r:$(NUNIT) /r:bin\IIOPChannel.dll /r:$(bin)customCollectionMappings.dll /r:$(bin)javaCollections.dll /r:bin\Client.dll /out:bin\ClientTest.dll TestClient.cs ValueTypeImpls.cs


copy-idl:
    if not exist IDL\nul mkdir IDL
    cd IDL
    copy ..\..\..\..\IDLToCLSCompiler\IDL\*.idl .
    xcopy /s /q /y ..\..\TestServer\IDL .
    cd ..

compile-idl: $(bin)javaCollections.dll $(bin)customMapping.xml
	cd IDL 
	..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -c ..\bin\customMapping.xml -o ..\bin -r ..\bin\javaCollections.dll client Ch\Elca\Iiop\IntegrationTests\MappingPlugin\TestService.idl Ch\Elca\Iiop\IntegrationTests\MappingPlugin\TestSerializableClassB1.idl
	cd ..


build-plugin:
	cd $(MAPPINGPLUGINDIR)
	$(MAKE) build
	cd ..\..\IntegrationTests\MappingPluginJava\TestClient

start-client:
	@echo "Mapping plugin java server IntegrationTest"
	%NUNITV2_HOME%\bin\nunit-console.exe /assembly:bin\ClientTest.dll

clean-plugin:
	cd $(MAPPINGPLUGINDIR)
	$(MAKE) clean
	cd ..\..\IntegrationTests\MappingPluginJava\TestClient

clean: clean-plugin
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin



