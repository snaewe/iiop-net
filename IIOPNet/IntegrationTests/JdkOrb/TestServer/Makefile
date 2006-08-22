include ..\..\..\MakeVars

LAUNCH = ..\..\..\Utils\launch.exe
KILL = ..\..\..\Utils\kill.exe
DELAY = ..\..\..\Utils\delay.exe


build: build-stubs compile

build-stubs: Ch\Elca\Iiop\IntegrationTests\TestService.java Ch\Elca\Iiop\IntegrationTests\TestServiceOperations.java Ch\Elca\Iiop\IntegrationTests\TestServicePOA.java

compile: Ch\Elca\Iiop\IntegrationTests\TestServiceImpl.class Ch\Elca\Iiop\IntegrationTests\TestServiceOperations.class TestServer.class

Ch\Elca\Iiop\IntegrationTests\TestService.java Ch\Elca\Iiop\IntegrationTests\TestServiceOperations.java Ch\Elca\Iiop\IntegrationTests\TestServicePOA.java : TestService.idl
	idlj -fAll TestService.idl

Ch\Elca\Iiop\IntegrationTests\TestServiceImpl.class Ch\Elca\Iiop\IntegrationTests\TestServiceOperations.class TestServer.class: Ch\Elca\Iiop\IntegrationTests\TestServiceImpl.java Ch\Elca\Iiop\IntegrationTests\TestServiceOperations.java TestServer.java 
	javac Ch\Elca\Iiop\IntegrationTests\*.java TestServer.java



start-server:
	@$(LAUNCH) orbd.exe -ORBInitialPort 1050 >> pid
	@$(DELAY) 10
        @$(LAUNCH) java.exe -cp . TestServer -ORBInitialPort 1050 -ORBInitialHost localhost >> pid
        @cd ..

stop-server:
        @for /F %%p in (pid) do @$(KILL) %%p
        @del pid

clean:
	if EXIST *.class del /Q *.class
	if EXIST Ch\Elca\Iiop\IntegrationTests\*.class del /Q Ch\Elca\Iiop\IntegrationTests\*.class
	if EXIST Ch\Elca\Iiop\IntegrationTests\TestServicePOA.java del /Q Ch\Elca\Iiop\IntegrationTests\TestServicePOA.java
	if EXIST Ch\Elca\Iiop\IntegrationTests\TestService.java del /Q Ch\Elca\Iiop\IntegrationTests\TestService.java
