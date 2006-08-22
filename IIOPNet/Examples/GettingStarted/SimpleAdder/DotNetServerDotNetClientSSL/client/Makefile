include ..\..\..\..\..\MakeVars

IDLSource=..\server\IDL\Ch\Elca\Iiop\Tutorial\Gettingstarted\*.idl 
Adder=Ch\Elca\Iiop\Tutorial\Gettingstarted\ 
IDLToCLS=..\..\..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe
TARGETASM=adder
IIOPCHANNEL = ..\..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
SSLPlugin = ..\..\..\..\..\SSLPLugin\bin\SSLPlugin.dll
SSLLib = ..\..\..\..\..\SSLPLugin\bin\org.mentalis.security.dll
BIN=bin\ 
IDL=IDL\ 

build: build-idl build-client

$(BIN)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(BIN)IIOPChannel.dll

$(BIN)SSLPlugin.dll: $(SSLPlugin)
	copy /y $(SSLPlugin) $(BIN)SSLPLugin.dll

$(BIN)org.mentalis.security.dll: $(SSLLib)
	copy /y $(SSLLib) $(BIN)org.mentalis.security.dll


$(BIN) $(IDL):
	if not exist $@nul mkdir $@

build-idl: $(IDL)
	copy ..\..\..\..\..\IDLToCLSCompiler\IDL\*.idl $(IDL)
	xcopy /s /y ..\server\IDL $(IDL)
	cd $(IDL)
	$(IDLToCLS) -o ..\$(BIN) $(TARGETASM) $(Adder)Adder.idl
	cd ..

build-client: $(BIN)$(TARGETASM).dll $(BIN)IIOPChannel.dll $(BIN)SSLPlugin.dll $(BIN)org.mentalis.security.dll NClient.cs
	$(CSC) $(CSFLAGS) /r:$(BIN)$(TARGETASM).dll /r:$(BIN)IIOPChannel.dll /r:$(BIN)SSLPlugin.dll /r:$(BIN)org.mentalis.security.dll /out:$(BIN)NClient.exe NClient.cs

clean:
	if exist $(IDL)nul rmdir /q /s $(IDL)
	if exist $(BIN)nul rmdir /q /s $(BIN)

