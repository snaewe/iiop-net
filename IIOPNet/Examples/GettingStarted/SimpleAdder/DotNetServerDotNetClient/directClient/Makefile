include ..\..\..\..\..\MakeVars

COMMONASMSOURCE = ..\server\bin\adder.dll
IIOPCHANNEL = ..\..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
BIN=bin\ 
COMMONASMTARGET = $(BIN)\adder.dll

build: copy-common build-client

$(BIN)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(BIN)IIOPChannel.dll

$(BIN):
	if not exist $@nul mkdir $@

copy-common: $(BIN) $(COMMONASMTARGET)


$(COMMONASMTARGET): $(BIN)
	copy $(COMMONASMSOURCE) $(BIN)

build-client: $(COMMONASMTARGET) $(BIN)IIOPChannel.dll NClient.cs
	$(CSC) /r:$(COMMONASMTARGET) /r:$(BIN)IIOPChannel.dll /out:$(BIN)NClient.exe NClient.cs

clean:
	if exist $(BIN)nul rmdir /q /s $(BIN)

