IIOPChannel = ..\..\IIOPChannel\bin\IIOPChannel.dll
Dest = bin\ 

build: build-dll

build-dll: bin\IDLPreprocessor.dll

$(Dest)IIOPChannel.dll: $(IIOPChannel)
	copy $(IIOPChannel) $@

$(Dest)IDLPreprocessor.dll: $(Dest)IIOPChannel.dll *.java *.jsl
	if not exist $(Dest)nul mkdir $(Dest)
	vjc $(CSFLAGS) /r:$(Dest)IIOPChannel.dll /t:library /out:$@ *.java *.jsl 


clean:
	del /s /q $(Dest)*
	rmdir $(Dest)
