include ..\..\..\..\MakeVars

CLS2IDL     = ..\..\..\..\CLSToIDLGenerator\bin\CLSIDLGenerator.exe
IIOPCHANNEL = ..\..\..\..\IIOPChannel\bin\IIOPChannel.dll
bin         = bin\ 
IDL         = IDL\ 
IDLType     = Ch.Elca.Iiop.Demo.Chatroom.IChatroom
SERVEREXE      = $(bin)ChatServer.exe

LAUNCH = ..\..\..\..\Utils\launch.exe
KILL = ..\..\..\..\Utils\kill.exe


build: build-exe build-idl

build-exe: $(bin) $(SERVEREXE)

$(bin) $(IDL):
	if not exist $@nul mkdir $@

$(bin)IIOPChannel.dll: $(IIOPCHANNEL)
	copy /y $(IIOPCHANNEL) $(bin)IIOPChannel.dll

$(SERVEREXE): $(bin)IIOPChannel.dll *.cs
	$(CSC) $(CSFLAGS) /t:exe /out:$@ /r:$(bin)IIOPChannel.dll *.cs


build-idl: $(IDL) $(SERVEREXE) 
	$(CLS2IDL) -o $(IDL) $(IDLType) $(SERVEREXE)

start-server:
	@$(LAUNCH) bin\ChatServer.exe >> pid

stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

clean:
	if exist $(bin)nul rmdir /q /s $(bin)
	if exist $(IDL)nul rmdir /q /s $(IDL)