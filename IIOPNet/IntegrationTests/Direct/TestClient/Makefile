NUNIT=$(NUNITV2_HOME)\bin\nunit.framework.dll 
IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
TestService = ..\TestServer\bin\TestServiceCommon.dll 
TestServicePDB = ..\TestServer\bin\TestServiceCommon.pdb  
bin         = bin\ 

build: build-client

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(bin) $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
        if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

$(bin)TestServiceCommon.dll: $(bin)
        copy /y $(TestService) $(bin)TestServiceCommon.dll
        if exist $(TestServicePDB) copy /y $(TestServicePDB) $(bin)TestServiceCommon.pdb

build-client: $(bin)IIOPChannel.dll $(bin)TestServiceCommon.dll *.cs
	if not exist bin\nul mkdir bin
	csc $(CSFLAGS) /t:library /r:$(NUNIT) /r:$(bin)TestServiceCommon.dll /r:$(bin)IIOPChannel.dll /out:bin\ClientTest.dll TestClient.cs

start-client:
	@echo "Direct IntegrationTest"
	%NUNITV2_HOME%\bin\nunit-console.exe /assembly:bin\ClientTest.dll

clean:
	if exist bin del /s /q bin
	if exist bin rmdir /s /q bin



