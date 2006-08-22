include ..\..\MakeVars
IDLToCLS=..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe
TARGETASMCOLL=javaCollections
TARGETASMMAP=IIOPNetCustomJavaTypeMapping.dll
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
        rmic -d $(IDL) -idl -noValueMethods java.util.ArrayList
        rmic -d $(IDL) -idl -noValueMethods java.util.HashMap
        rmic -d $(IDL) -idl -noValueMethods java.lang.Long
        rmic -d $(IDL) -idl -noValueMethods java.lang.Integer
        rmic -d $(IDL) -idl -noValueMethods java.lang.Short
        rmic -d $(IDL) -idl -noValueMethods java.lang.Double
        rmic -d $(IDL) -idl -noValueMethods java.lang.Float
        rmic -d $(IDL) -idl -noValueMethods java.lang.Byte
        rmic -d $(IDL) -idl -noValueMethods java.lang.Boolean
        rmic -d $(IDL) -idl -noValueMethods java.lang.Character
        rmic -d $(IDL) -idl -noValueMethods java.util.Date
    cd $(IDL)
    $(IDLToCLS) -o ..\$(BIN) $(TARGETASMCOLL) java\util\HashMap.idl java\util\ArrayList.idl java\lang\Long.idl java\lang\Integer.idl java\lang\Short.idl java\lang\Byte.idl java\lang\Double.idl java\lang\Float.idl java\lang\Boolean.idl java\lang\Character.idl java\util\Date.idl
    cd ..

build-lib: $(BIN)$(TARGETASMCOLL).dll $(BIN)IIOPChannel.dll CollectionsImpl.cs BaseTypesImpl.cs InstanceMappers.cs
    $(CSC) $(CSFLAGS) /t:library /r:$(BIN)$(TARGETASMCOLL).dll /r:$(BIN)IIOPChannel.dll /out:$(BIN)$(TARGETASMMAP) CollectionsImpl.cs BaseTypesImpl.cs InstanceMappers.cs

clean:
    if exist $(IDL)nul rmdir /q /s $(IDL)
    if exist $(BIN)nul rmdir /q /s $(BIN)

