IIOPChannel = ..\..\IIOPChannel\bin\IIOPChannel.dll
IDLPreprocessor = ..\IDLPreprocessor\bin\IDLPreprocessor.dll
Dest = bin\ 

build: build-exe

build-exe: bin\IDLToCLSCompiler.exe

$(Dest)IIOPChannel.dll: $(IIOPChannel)
	copy $(IIOPChannel) $@

$(Dest)IDLPreprocessor.dll: $(IDLPreprocessor)
	copy $(IDLPreprocessor) $@

$(Dest)IDLToCLSCompiler.exe: $(Dest)IIOPChannel.dll $(Dest)IDLPreprocessor.dll *.java *.jsl
	if not exist $(Dest)nul mkdir $(Dest)
	vjc $(CSFLAGS) /r:$(Dest)IIOPChannel.dll /r:$(Dest)IDLPreprocessor.dll /t:exe /out:$@ *.java *.jsl 


clean:
	del /s /q $(Dest)*
	rmdir $(Dest)
