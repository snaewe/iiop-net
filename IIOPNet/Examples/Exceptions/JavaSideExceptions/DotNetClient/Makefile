include ..\..\..\..\MakeVars

ServicePath  = tutorial\ 
ServiceName  = Service

ServiceIDL   = $(ServicePath)$(ServiceName).idl

STUB         = service

IIOPBase     = ..\..\..\..\ 
IDLToCLS     = $(IIOPBase)IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe
IIOPCHANNEL  = $(IIOPBase)IIOPChannel\bin\IIOPChannel.dll
BIN=bin\ 
IDL=IDL\ 

build: build-idl build-client

$(BIN)IIOPChannel.dll: $(IIOPCHANNEL)
    copy /y $(IIOPCHANNEL) $(BIN)IIOPChannel.dll

$(BIN) $(IDL):
    if not exist $@nul mkdir $@

build-idl: $(IDL)
    copy $(IIOPBase)IDLToCLSCompiler\IDL\*.idl $(IDL)
    xcopy /s /y ..\JavaServer\IDL $(IDL)
    cd $(IDL)
    ..\$(IDLToCLS) -o ..\$(BIN) $(STUB) $(ServiceIDL)
    cd ..

build-client: $(BIN)$(STUB).dll $(BIN)IIOPChannel.dll NClient.cs ExceptionImplementation.cs
    $(CSC) $(CSFLAGS) /r:$(BIN)$(STUB).dll /r:$(BIN)IIOPChannel.dll /out:$(BIN)NClient.exe NClient.cs ExceptionImplementation.cs

clean:
    if exist $(IDL)nul rmdir /q /s $(IDL)
    if exist $(BIN)nul rmdir /q /s $(BIN)

