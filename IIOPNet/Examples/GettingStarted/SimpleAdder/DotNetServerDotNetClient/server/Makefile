include ..\..\..\..\..\MakeVars

CLS2IDL     = ..\..\..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe
IIOPCHANNEL = ..\..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
bin         = bin\ 
IDL         = IDL\ 
IDLType     = Ch.Elca.Iiop.Tutorial.GettingStarted.Adder
IDLPath     = Ch\Elca\Iiop\Tutorial\GettingStarted\Adder
SERVEREXE   = $(bin)AdderServer.exe
COMMONDLL   = $(bin)Adder.dll

LAUNCH = ..\..\..\..\..\Utils\launch.exe
KILL = ..\..\..\..\..\Utils\kill.exe
DELAY = ..\..\..\..\..\Utils\delay.exe


build: build-commondll build-serverexe build-idl

build-commondll: $(bin) $(COMMONDLL)
build-serverexe: $(bin) $(SERVEREXE)


$(bin) $(IDL):
	if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll

$(COMMONDLL): $(bin)IIOPChannel.dll NAdder.cs
	$(CSC) $(CSFLAGS) /t:library /out:$@ /r:$(bin)IIOPChannel.dll NAdder.cs

$(SERVEREXE): $(bin)IIOPChannel.dll $(COMMONDLL) NAdderServer.cs
	$(CSC) $(CSFLAGS) /t:exe /out:$@ /r:$(bin)IIOPChannel.dll /r:$(COMMONDLL) NAdderServer.cs


build-idl: $(IDL) $(IDL)$(IDLPath).idl $(COMMONDLL) 

$(IDL)$(IDLPath).idl: $(COMMONDLL) 
	$(CLS2IDL) -o $(IDL) $(IDLType) $?

start-server:
    @$(LAUNCH) $(SERVEREXE) >> pid


stop-server:
    @for /F %%p in (pid) do @$(KILL) %%p
    @del pid


clean:
	if exist $(bin)nul rmdir /q /s $(bin)
	if exist $(IDL)nul rmdir /q /s $(IDL)