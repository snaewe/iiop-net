NUNIT=$(NUNITV2_HOME)\bin\nunit.framework.dll 
IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
TestService = ..\TestServer\bin\TestServiceCommon.dll 
TestServicePDB = ..\TestServer\bin\TestServiceCommon.pdb  
TestUnion = ..\TestServer\bin\TestUnion.dll
bin         = bin\ 

build: $(bin)ClientTest.dll


$(bin)TestUnion.dll : $(TestUnion)
        copy /y $(TestUnion) $(bin)
	

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
        if not exist $(bin)nul mkdir $(bin)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
        if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

$(bin)TestServiceCommon.dll: $(TestService)
        if not exist $(bin)nul mkdir $(bin)
        copy /y $(TestService) $(bin)TestServiceCommon.dll
        if exist $(TestServicePDB) copy /y $(TestServicePDB) $(bin)TestServiceCommon.pdb

$(bin)ClientTest.dll: $(bin)IIOPChannel.dll $(bin)TestUnion.dll $(bin)TestServiceCommon.dll *.cs
	csc $(CSFLAGS) /t:library /r:$(NUNIT) /r:$(bin)TestUnion.dll /r:$(bin)TestServiceCommon.dll /r:$(bin)IIOPChannel.dll /out:bin\ClientTest.dll TestClient.cs

start-client:
	@echo "Direct IntegrationTest"
	%NUNITV2_HOME%\bin\nunit-console.exe /assembly:bin\ClientTest.dll

clean:
	if exist bin del /s /q bin
	if exist bin rmdir /s /q bin



