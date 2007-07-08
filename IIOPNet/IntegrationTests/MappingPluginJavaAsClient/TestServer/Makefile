include ..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb
MAPPINGPLUGINDIR = ..\..\..\MappingPlugin\java\ 
CUSTOMJAVATYPEMAPPING=IIOPNetCustomJavaTypeMapping

LAUNCH=..\..\..\Utils\launch.exe
KILL = ..\..\..\Utils\kill.exe
DELAY = ..\..\..\Utils\delay.exe

bin         = bin\ 

build: build-plugin build-server build-idl

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(bin) $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
	if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

$(bin)javaCollections.dll: $(bin) $(MAPPINGPLUGINDIR)\bin\javaCollections.dll
        copy /y $(MAPPINGPLUGINDIR)bin\javaCollections.* $(bin)

$(bin)$(CUSTOMJAVATYPEMAPPING).dll: $(bin) $(MAPPINGPLUGINDIR)\bin\$(CUSTOMJAVATYPEMAPPING).dll
        copy /y $(MAPPINGPLUGINDIR)bin\$(CUSTOMJAVATYPEMAPPING).* $(bin)

$(bin)customMapping.xml: $(bin) $(MAPPINGPLUGINDIR)customMapping.xml
        copy /y $(MAPPINGPLUGINDIR)customMapping.xml .

$(bin)customGeneratorMapping.xml: $(bin) $(MAPPINGPLUGINDIR)customGeneratorMapping.xml
        copy /y $(MAPPINGPLUGINDIR)customGeneratorMapping.xml $(bin)

build-server: $(bin)IIOPChannel.dll $(bin)$(CUSTOMJAVATYPEMAPPING).dll $(bin)customMapping.xml $(bin)javaCollections.dll *.cs
    $(CSC) $(CSFLAGS) /res:customMapping.xml /t:exe /r:$(bin)IIOPChannel.dll /out:$(bin)TestServer.exe TestService.cs TestServer.cs
    

build-idl: $(bin)customGeneratorMapping.xml
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL -c $(bin)customGeneratorMapping.xml Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestService $(bin)testServer.exe
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestSerializableClassB1 $(bin)testServer.exe
    ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe -o IDL Ch.Elca.Iiop.IntegrationTests.MappingPlugin.ByteArrayContainer $(bin)testServer.exe

build-plugin:
        cd $(MAPPINGPLUGINDIR)
        $(MAKE) build
        cd ..\..\IntegrationTests\MappingPluginJavaAsClient\TestServer

clean-plugin:
	cd $(MAPPINGPLUGINDIR)
	$(MAKE) clean
        cd ..\..\IntegrationTests\MappingPluginJavaAsClient\TestServer

start-server:
	@$(LAUNCH) bin\TestServer.exe >> pid
#	for /F "usebackq" %%r in (`..\..\..\Utils\launch.exe bin\TestServer.exe`) do set pid=%%r

stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

clean: clean-plugin
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin
	if exist customMapping.xml del customMapping.xml






