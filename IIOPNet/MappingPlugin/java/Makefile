IDLToCLS=..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe
TARGETASMCOLL=javaCollections
TARGETASMMAP=customCollectionMappings.dll
IIOPCHANNEL = ..\..\IIOPChannel\bin\IIOPChannel.dll
BIN=bin\ 
IDL=IDL\ 

build: build-idl build-lib

$(BIN)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(BIN)IIOPChannel.dll

$(BIN) $(IDL):
	if not exist $@nul mkdir $@

build-idl: $(IDL)
	copy ..\..\IDLToCLSCompiler\IDL\*.idl $(IDL)
        rem create the idl for the collection classes and the boxed java base types
        rmic -d $(IDL) -idl java.util.ArrayList
        rmic -d $(IDL) -idl java.lang.Integer
        rmic -d $(IDL) -idl java.lang.Short
        rmic -d $(IDL) -idl java.lang.Double        
        rmic -d $(IDL) -idl java.lang.Float
        rmic -d $(IDL) -idl java.lang.Byte
        rmic -d $(IDL) -idl java.lang.Boolean
        rmic -d $(IDL) -idl java.lang.Character
	cd $(IDL)
	$(IDLToCLS) -o ..\$(BIN) $(TARGETASMCOLL) java\util\ArrayList.idl java\lang\Integer.idl java\lang\Short.idl java\lang\Byte.idl java\lang\Double.idl java\lang\Float.idl java\lang\Boolean.idl java\lang\Character.idl
	cd ..

build-lib: $(BIN)$(TARGETASMCOLL).dll $(BIN)IIOPChannel.dll CollectionsImpl.cs BaseTypesImpl.cs InstanceMappers.cs
	csc $(CSFLAGS) /t:library /r:$(BIN)$(TARGETASMCOLL).dll /r:$(BIN)IIOPChannel.dll /out:$(BIN)$(TARGETASMMAP) CollectionsImpl.cs BaseTypesImpl.cs InstanceMappers.cs

clean:
	if exist $(IDL)nul rmdir /q /s $(IDL)
	if exist $(BIN)nul rmdir /q /s $(BIN)

