Dest = bin\ 


build: build-dll

build-dll: $(Dest)IIOPChannel.dll
	
$(Dest)IIOPChannel.dll: *.cs
	if not exist $(Dest)nul mkdir $@
	csc $(CSFLAGS) /t:library /out:$@ *.cs
	
clean:
	del /s /q $(Dest)*.dll
	rmdir $(Dest) 
