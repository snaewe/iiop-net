NUNIT=$(NUNITV2_HOME)\bin\nunit.framework.dll 
IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
bin         = bin\ 
IDL         = IDL\ 

build: compile-idl build-client

$(bin):
        if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(bin) $(IIOPCHANNEL)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
        if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

build-client: $(bin)IIOPChannel.dll *.cs
	if not exist bin\nul mkdir bin
	csc $(CSFLAGS) /t:library /r:$(NUNIT) /r:bin\IIOPChannel.dll /r:bin\Client.dll /out:bin\ClientTest.dll TestClient.cs


copy-idl: $(IDL)service.idl


$(IDL)service.idl: ..\TestServer\service.idl
        copy ..\TestServer\service.idl $(IDL)

compile-idl: copy-idl
	cd $(IDL) 
	..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\bin client service.idl
	cd ..


start-client:
	@echo "OmniORB Extension IntegrationTest"
	%NUNITV2_HOME%\bin\nunit-console.exe /assembly:bin\ClientTest.dll

clean:
	if exist bin del /s /q bin
	if exist bin rmdir /s /q bin

