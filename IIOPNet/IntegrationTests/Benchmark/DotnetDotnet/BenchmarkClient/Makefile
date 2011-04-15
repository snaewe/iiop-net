include ..\..\..\..\MakeVars

IIOPCHANNEL = ..\..\..\..\IIOPChannel\bin\IIOPChannel.dll 
IIOPCHANNELPDB = ..\..\..\..\IIOPChannel\bin\IIOPChannel.pdb 
IDLTOCLSCOMP   = ..\..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe
CLSFORIDL      = client.dll
TestServerIDLDir = ..\BenchmarkServer\IDL
TestServerIDLService = $(TestServerIDLDir)\$(ServiceIDLFileName)
ServiceIDLFileName   = Ch\Elca\Iiop\Benchmarks\TestService.idl 
bin            = bin\ 
IDL            = IDL\ 

build: compile-idl build-client

compile-idl: $(IDL)$(ServiceIdlFileName) $(bin)$(CLSFORIDL)

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
        if not exist $(bin) mkdir $(bin)
        copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
        if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb

$(IDL)$(ServiceIdlFileName) : $(TestServerIDLService)
    if not exist $(IDL)nul mkdir $(IDL)
    cd $(IDL)
    xcopy /s /q /y ..\$(TestServerIDLDir) .
    cd ..    
    
$(bin)$(CLSFORIDL): $(IDL)$(ServiceIDLFileName)
    cd $(IDL)
    $(IDLTOCLSCOMP) -o ..\$(bin) client $(ServiceIDLFileName)
    cd ..


build-client: $(bin)IIOPChannel.dll $(bin)$(CLSFORIDL) *.cs
    $(CSC) $(CSFLAGS) /t:exe /r:$(bin)$(CLSFORIDL) /r:$(bin)IIOPChannel.dll /out:bin\ClientTest.exe TestClient.cs ValueTypeImpls.cs

start-client:
    @echo "Benchmark .NET <-> .NET"
    $(bin)ClientTest.exe 1000
    
    
    
clean:
    if exist $(bin) rmdir /s /q $(bin)
    if exist $(IDL) rmdir /s /q $(IDL)

