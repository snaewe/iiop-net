IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb
MAPPINGPLUGINDIR = ..\..\..\MappingPlugin\java\ 
bin         = bin\ 

build: build-plugin build-server build-idl

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
        copy /y $(MAPPINGPLUGINDIR)customMapping.xml .

$(bin)customGeneratorMapping.xml: $(bin) $(MAPPINGPLUGINDIR)customGeneratorMapping.xml
        copy /y $(MAPPINGPLUGINDIR)customGeneratorMapping.xml $(bin)

build-server: $(bin)IIOPChannel.dll $(bin)customCollectionMappings.dll $(bin)customMapping.xml $(bin)javaCollections.dll *.cs
    csc $(CSFLAGS) /res:customMapping.xml /t:exe /r:$(bin)IIOPChannel.dll /out:$(bin)TestServer.exe TestService.cs TestServer.cs
    

build-idl: $(bin)customGeneratorMapping.xml
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL -c $(bin)customGeneratorMapping.xml Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestService $(bin)testServer.exe
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestSerializableClassB1 $(bin)testServer.exe

build-plugin:
        cd $(MAPPINGPLUGINDIR)
        $(MAKE) build
        cd ..\..\IntegrationTests\MappingPluginJavaAsClient\TestServer

clean:
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin
	if exist customMapping.xml del customMapping.xml
