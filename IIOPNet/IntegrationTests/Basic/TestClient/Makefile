NUNIT=D:\Programme\NUnitV2\bin\nunit.framework.dll 


build: compile-idl build-client

build-client:
	csc /t:library /r:$(NUNIT) /r:bin\IIOPChannel.dll /r:bin\Client.dll /out:bin\ClientTest.dll TestClient.cs


compile-idl:
	cd IDL 
	..\..\..\..\IDLToCLSCompiler\IDLCompiler\bin\IDLToCLSCompiler.exe -o ..\bin client Ch\Elca\Iiop\IntegrationTests\TestService.idl
	cd ..

