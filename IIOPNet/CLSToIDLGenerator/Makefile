IIOPChannel = ..\IIOPChannel\bin\IIOPChannel.dll
Dest = bin\ 

build: build-exe

build-exe: bin\CLSIDLGenerator.exe

$(Dest):
	mkdir $(Dest)

$(Dest)IIOPChannel.dll: $(IIOPChannel)
	copy $(IIOPChannel) $@

$(Dest)CLSIDLGenerator.exe: $(Dest) $(Dest)IIOPChannel.dll *.cs
	csc $(CSFLAGS) /r:$(Dest)IIOPChannel.dll /t:exe /out:$@ *.cs 


clean:
	del /s /q $(Dest)*
	rmdir $(Dest)
