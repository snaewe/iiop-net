build: build-cls build-client


build-cls:
	if not exist bin mkdir bin
	if not exist IDL mkdir IDL
	xcopy /s /q /y ..\Server\IDL IDL\ 
	cd IDL
	copy ..\..\..\..\..\IDLToCLSCompiler\IDL\orb.idl .
	..\..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\bin chatroom Ch\Elca\Iiop\Demo\Chatroom\IChatroom.idl
	cd ..
	


build-client:
	copy ..\..\..\..\IIOPChannel\bin\IIOPChannel.dll bin\ 
	csc $(CSFLAGS) /r:bin\IIOPChannel.dll /r:bin\chatroom.dll /t:exe /out:bin\Client.exe *.cs


clean:
	if exist bin del /s /q bin\ 
	if exist bin rmdir /s /q bin\ 
	if exist IDL del /s /q IDL\ 
	if exist IDL rmdir /s /q IDL\ 

	
