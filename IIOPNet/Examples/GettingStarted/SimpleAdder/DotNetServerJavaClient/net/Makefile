include ..\..\..\..\..\MakeVars

CLS2IDL     = ..\..\..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe
IIOPCHANNEL = ..\..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
bin         = bin\ 
IDL         = IDL\ 
IDLType     = Ch.Elca.Iiop.Tutorial.GettingStarted.AdderImpl
IDLPath     = Ch\Elca\Iiop\Tutorial\GettingStarted\AdderImpl
SERVEREXE      = $(bin)AdderServer.exe

LAUNCH = ..\..\..\..\..\Utils\launch.exe
KILL = ..\..\..\..\..\Utils\kill.exe
DELAY = ..\..\..\..\..\Utils\delay.exe


build: build-exe build-idl

build-exe: $(bin) $(SERVEREXE)

$(bin) $(IDL):
	if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll

$(SERVEREXE): $(bin)IIOPChannel.dll *.cs
	$(CSC) $(CSFLAGS) /t:exe /out:$@ /r:$(bin)IIOPChannel.dll *.cs


build-idl: $(IDL) $(IDL)$(IDLPath).idl $(SERVEREXE) 

$(IDL)$(IDLPath).idl: $(SERVEREXE) 
	$(CLS2IDL) -o $(IDL) $(IDLType) $?

start-server:
	@$(LAUNCH) $(SERVEREXE) >> pid
 

stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

clean:
	if exist $(bin)nul rmdir /q /s $(bin)
	if exist $(IDL)nul rmdir /q /s $(IDL)