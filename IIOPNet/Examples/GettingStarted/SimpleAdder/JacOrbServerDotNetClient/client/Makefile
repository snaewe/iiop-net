include ..\..\..\..\..\MakeVars

IDLSource=..\server\IDL\*.idl 
IDLToCLS=..\..\..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe
TARGETASM=adder
IIOPCHANNEL = ..\..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
BIN=bin\ 
IDL=IDL\ 

build: build-idl build-client

$(BIN)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(BIN)IIOPChannel.dll

$(BIN) $(IDL):
	if not exist $@nul mkdir $@

build-idl: $(IDL)
	copy ..\..\..\..\..\IDLToCLSCompiler\IDL\*.idl $(IDL)
	xcopy /s /y ..\server\IDL $(IDL)
	cd $(IDL)
	$(IDLToCLS) -o ..\$(BIN) $(TARGETASM) Adder.idl
	cd ..

build-client: $(BIN)$(TARGETASM).dll $(BIN)IIOPChannel.dll NClient.cs
	$(CSC) /r:$(BIN)$(TARGETASM).dll /r:$(BIN)IIOPChannel.dll /out:$(BIN)NClient.exe NClient.cs

clean:
	if exist $(IDL)nul rmdir /q /s $(IDL)
	if exist $(BIN)nul rmdir /q /s $(BIN)

