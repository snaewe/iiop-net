Dest = bin\ 


build: build-dll

build-dll: $(Dest)IIOPChannel.dll
	
$(Dest)IIOPChannel.dll: *.cs
	if not exist $@nul mkdir $@
	csc $(CSFLAGS) /t:library /out:$@ $? 
	
clean:
	del /s /q $(Dest)*.dll
	rmdir $(Dest) 
