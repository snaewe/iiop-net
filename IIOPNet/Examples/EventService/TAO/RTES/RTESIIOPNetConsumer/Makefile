include ..\..\..\..\..\MakeVars

bin=bin\ 
idl=idl\ 
IDLToCLSCompiler=..\..\..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe
CLSForIDL=RTES
Consumer=RTESConsumer.exe
IIOPChannelAsm=..\..\..\..\..\IIOPChannel\bin\IIOPChannel.dll

build: build-cls build-consumer


build-cls: $(bin)$(CLSForIDL).dll

$(bin)$(CLSForIDL).dll : $(idl)*.idl
    if NOT EXIST $(bin) mkdir $(bin)   
    cd IDL
    $(IDLToCLSCompiler) -o ..\$(bin) $(CLSForIDL) RtecEventChannelAdmin.idl
    cd ..
    

$(bin)IIOPChannel.dll: $(IIOPChannelAsm)
    if NOT EXIST $(bin) mkdir $(bin)
    copy $(IIOPChannelAsm) $(bin)IIOPChannel.dll


build-consumer: $(bin)IIOPChannel.dll $(bin)$(CLSForIDL).dll $(bin)$(Consumer)


$(bin)$(Consumer): *.cs
    $(CSC) /out:$(bin)$(Consumer) /r:$(bin)$(CLSForIDL).dll /r:$(bin)IIOPChannel.dll *.cs
    
clean:
    if exist $(bin) rmdir /s /q $(bin)