IIOPChannel = ..\IIOPChannel\bin\IIOPChannel.dll
Dest = bin\ 

build: build-exe

build-exe: bin\CLSIDLGenerator.exe

$(Dest)IIOPChannel.dll: $(IIOPChannel)
	if not exist $(Dest)nul mkdir $(Dest)
	copy $(IIOPChannel) $@

$(Dest)CLSIDLGenerator.exe: $(Dest)IIOPChannel.dll *.cs
	csc $(CSFLAGS) /r:$(Dest)IIOPChannel.dll /t:exe /out:$@ *.cs 


clean:
	del /s /q $(Dest)*
	rmdir $(Dest)
