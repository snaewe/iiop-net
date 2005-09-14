CLS2IDL     = ..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe
IIOPCHANNEL = ..\..\..\IIOPChannel\bin\IIOPChannel.dll
bin         = bin\ 
IDL         = IDL\ 
IDLType     = Ch.Elca.Iiop.Tutorial.GettingStarted.Adder
IDLPath     = Ch\Elca\Iiop\Tutorial\GettingStarted\Adder
SERVEREXE      = $(bin)AdderServer.exe

build: build-exe build-idl

build-exe: $(bin) $(SERVEREXE)

$(bin) $(IDL):
	if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll

$(SERVEREXE): $(bin)IIOPChannel.dll *.cs
	csc $(CSFLAGS) /t:exe /out:$@ /r:$(bin)IIOPChannel.dll *.cs


build-idl: $(IDL) $(IDL)$(IDLPath).idl $(SERVEREXE) 

$(IDL)$(IDLPath).idl: $(SERVEREXE) 
	$(CLS2IDL) -o $(IDL) $(IDLType) $?

clean:
	if exist $(bin)nul rmdir /q /s $(bin)
	if exist $(IDL)nul rmdir /q /s $(IDL)