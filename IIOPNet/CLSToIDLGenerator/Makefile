include ..\MakeVars

Key = ..\Key.snk
IIOPChannel = ..\IIOPChannel\bin\IIOPChannel.dll
Dest = bin\ 

build: build-exe

build-exe: bin\CLSIDLGenerator.exe

$(Dest)IIOPChannel.dll: $(IIOPChannel)
	if not exist $(Dest)nul mkdir $(Dest)
	copy $(IIOPChannel) $@

$(Dest)CLSIDLGenerator.exe: $(Dest)IIOPChannel.dll *.cs $(Key)
	$(CSC) $(CSFLAGS) /res:GeneratorMappingPluginSchema.xsd /r:$(Dest)IIOPChannel.dll /t:exe /out:$@ *.cs 


clean:
	if exist $(Dest)* del /s /q $(Dest)*
	if exist $(Dest) rmdir /s /q $(Dest)
