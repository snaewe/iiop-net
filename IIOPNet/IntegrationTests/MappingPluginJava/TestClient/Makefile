include ..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
MAPPINGPLUGINDIR=..\..\..\MappingPlugin\java\ 
CUSTOMJAVATYPEMAPPING=IIOPNetCustomJavaTypeMapping
CUSTOMMAPPEDTEST=CustomMapped.dll
bin         = bin\ 

build: build-plugin $(bin)CustomMapped.dll copy-idl compile-idl build-client

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
	if not exist $(bin) mkdir $(bin)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
        if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

$(bin)javaCollections.dll: $(MAPPINGPLUGINDIR)\bin\javaCollections.dll
	copy /y $(MAPPINGPLUGINDIR)bin\javaCollections.* $(bin)

$(bin)$(CUSTOMJAVATYPEMAPPING).dll: $(MAPPINGPLUGINDIR)\bin\$(CUSTOMJAVATYPEMAPPING).dll
	copy /y $(MAPPINGPLUGINDIR)bin\$(CUSTOMJAVATYPEMAPPING).* $(bin)

$(bin)customMapping.xml: $(MAPPINGPLUGINDIR)customMapping.xml
	copy /y $(MAPPINGPLUGINDIR)customMapping.xml $(bin)

# mapping file for mapping CustomMappedSerilizable (java) <-> CustomMappedSerializableCls (CLS)
$(bin)customMappingTest.xml : customMappingTest.xml
        copy /y customMappingTest.xml $(bin)

# the assembly containing the cls definition for the custom mapped non-java library test type 
$(bin)$(CUSTOMMAPPEDTEST) : CustomMapped.cs
	if not exist $(bin) mkdir $(bin)
	$(CSC) /t:library /out:$(bin)$(CUSTOMMAPPEDTEST) CustomMapped.cs
	

build-client: $(bin)IIOPChannel.dll $(bin)$(CUSTOMJAVATYPEMAPPING).dll $(bin)customMapping.xml $(bin)customMappingTest.xml $(bin)javaCollections.dll TestClient.cs ValueTypeImpls.cs
	$(CSC) $(CSFLAGS) /t:library /r:$(NUNIT_FRAMEWORK_DLL) /r:bin\IIOPChannel.dll /r:$(bin)$(CUSTOMJAVATYPEMAPPING).dll /r:$(bin)javaCollections.dll /r:$(bin)$(CUSTOMMAPPEDTEST) /r:$(bin)\Client.dll /out:$(bin)\ClientTest.dll TestClient.cs ValueTypeImpls.cs


copy-idl:
    if not exist IDL\nul mkdir IDL
    cd IDL
    copy ..\..\..\..\IDLToCLSCompiler\IDL\*.idl .
    xcopy /s /q /y ..\..\TestServer\IDL .
    cd ..

compile-idl: $(bin)javaCollections.dll $(bin)customMapping.xml $(bin)customMappingTest.xml $(bin)CustomMapped.dll
	cd IDL 
	..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -c ..\$(bin)customMapping.xml -c ..\$(bin)customMappingTest.xml -o ..\$(bin) -r ..\$(bin)javaCollections.dll client Ch\Elca\Iiop\IntegrationTests\MappingPlugin\TestService.idl Ch\Elca\Iiop\IntegrationTests\MappingPlugin\TestSerializableClassB1.idl
	cd ..


build-plugin:
	cd $(MAPPINGPLUGINDIR)
	$(MAKE) build
	cd ..\..\IntegrationTests\MappingPluginJava\TestClient

start-client:
	@echo "Mapping plugin java server IntegrationTest"
	$(NUNIT_CONSOLE) $(NUNIT_ASSEMBLY_ARG_OPTION)bin\ClientTest.dll

clean-plugin:
	cd $(MAPPINGPLUGINDIR)
	$(MAKE) clean
	cd ..\..\IntegrationTests\MappingPluginJava\TestClient

clean: clean-plugin
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin



