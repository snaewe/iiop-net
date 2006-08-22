include ..\..\..\..\MakeVars

IIOPCHANNEL    = ..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
IIOPCHANNELPDB = ..\..\..\..\IIOPChannel\bin\IIOPChannel.pdb
CLSTOIDLGEN    = ..\..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe
bin         = bin\ 
IDL         = IDL\ 
LAUNCH      = ..\..\..\..\Utils\launch.exe
KILL        = ..\..\..\..\Utils\kill.exe
DELAY       = ..\..\..\..\Utils\delay.exe


build:  build-server build-idl

build-server :  $(bin)TestServiceCommon.dll $(bin)TestServer.exe

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
    if not exist $(bin)nul mkdir $(bin)
    copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll
    if exist $(IIOPCHANNELPDB) copy /y $(IIOPCHANNELPDB) $(bin)IIOPChannel.pdb


$(bin)TestServiceCommon.dll: $(bin)IIOPChannel.dll TestServiceCommon.cs
    $(CSC) $(CSFLAGS) /t:library /r:$(bin)IIOPChannel.dll /out:$(bin)TestServiceCommon.dll TestServiceCommon.cs


$(bin)TestServer.exe: $(bin)IIOPChannel.dll $(bin)TestServiceCommon.dll TestServer.cs TestService.cs
    $(CSC) $(CSFLAGS) /t:exe /r:$(bin)IIOPChannel.dll /r:$(bin)TestServiceCommon.dll /out:$(bin)TestServer.exe TestServer.cs TestService.cs
    

build-idl: IDL\Ch\Elca\Iiop\Benchmarks\TestService.idl

IDL\Ch\Elca\Iiop\Benchmarks\TestService.idl: $(bin)TestServiceCommon.dll
    $(CLSTOIDLGEN) -o IDL Ch.Elca.Iiop.Benchmarks.TestService $(bin)TestServiceCommon.dll


start-server:
    @$(LAUNCH) bin\TestServer.exe >> pid

stop-server:
    @for /F %%p in (pid) do @$(KILL) %%p
    @del pid


    
clean:    
    if exist $(bin) rmdir /s /q $(bin)
    if exist $(IDL) rmdir /s /q $(IDL)
