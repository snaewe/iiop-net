include ..\..\..\..\MakeVars

build: build-cls build-client


build-cls:
	if not exist bin mkdir bin
	if not exist IDL mkdir IDL
	xcopy /s /q /y ..\java\IDL IDL\ 
	cd IDL
	copy ..\..\..\..\..\IDLToCLSCompiler\IDL\orb.idl .
	..\..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\bin chatroom ch\elca\iiop\demo\ejbChatroom\Chatroom.idl ch\elca\iiop\demo\ejbChatroom\ChatroomHome.idl
	cd ..
	


build-client:
	copy ..\..\..\..\IIOPChannel\bin\IIOPChannel.dll bin\ 
	$(CSC) $(CSFLAGS) /r:bin\IIOPChannel.dll /r:bin\chatroom.dll /t:exe /out:bin\Client.exe *.cs	


clean:
	if exist bin del /s /q bin\ 
	if exist bin rmdir /s /q bin\ 
	if exist IDL del /s /q IDL\ 
	if exist IDL rmdir /s /q IDL\ 

	
