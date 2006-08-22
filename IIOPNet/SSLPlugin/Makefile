include ..\MakeVars
Key = ..\Key.snk
IIOPCHANNEL = ..\IIOPChannel\bin\IIOPChannel.dll
SSLLibrary = ..\thirdparty\SSL\bin\Org.Mentalis.Security
BIN=bin\ 

build: build-lib

$(BIN)IIOPChannel.dll: $(IIOPCHANNEL)
    @if not exist $(BIN)nul mkdir $(BIN)
    @copy /y $(IIOPCHANNEL) $(BIN)IIOPChannel.dll
    
$(BIN)Org.Mentalis.Security.dll:
    @if not exist $(BIN)nul mkdir $(BIN)
    @if not defined MENTALISSECURITYLIB copy /y $(SSLLibrary).dll $(BIN)Org.Mentalis.Security.dll 
    @if not defined MENTALISSECURITYLIB (
        @if exist $(SSLLibrary).pdb copy /y $(SSLLibrary).pdb $(BIN)Org.Mentalis.Security.pdb 
    )
    @if defined MENTALISSECURITYLIB copy /y $(MENTALISSECURITYLIB) $(BIN)Org.Mentalis.Security.dll
    

build-lib: $(BIN)IIOPChannel.dll $(BIN)Org.Mentalis.Security.dll *.cs $(Key)
    $(CSC) $(CSFLAGS) /t:library /r:$(BIN)Org.Mentalis.Security.dll /r:$(BIN)IIOPChannel.dll /out:$(BIN)SSLPlugin.dll *.cs

clean:
    if exist $(BIN)nul rmdir /q /s $(BIN)

