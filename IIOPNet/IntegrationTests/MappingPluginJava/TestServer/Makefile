include ..\..\..\MakeVars

LAUNCH = ..\..\..\Utils\launch.exe
KILL = ..\..\..\Utils\kill.exe
DELAY = ..\..\..\Utils\delay.exe

bin         = bin\ 
IDL         = IDL\ 

build: build-server build-stubs build-idl

$(bin):
        if not exist $@nul mkdir $@

build-server: $(bin) *.java Ch\Elca\Iiop\IntegrationTests\MappingPlugin\*.java
    javac -d bin Ch\Elca\Iiop\IntegrationTests\MappingPlugin\TestServiceImpl.java
    javac -d bin TestServer.java    
    

build-stubs:
    cd bin
    rmic -iiop Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestServiceImpl
    cd ..

build-idl:
    if not exist $(IDL) mkdir $(IDL)
    cd bin
    rmic -d ..\$(IDL) -idl Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestServiceImpl
    rmic -d ..\$(IDL) -idl Ch.Elca.Iiop.IntegrationTests.MappingPlugin.TestSerializableClassB1
    rmic -d ..\$(IDL) -idl Ch.Elca.Iiop.IntegrationTests.MappingPlugin.CustomMappedSerializable    

    cd ..

start-server:
	@$(LAUNCH) orbd.exe -ORBInitialPort 1050 >> pid
	@$(DELAY) 10
	@cd bin
	@..\$(LAUNCH) java.exe -Djava.naming.factory.initial=com.sun.jndi.cosnaming.CNCtxFactory -Djava.naming.provider.url=iiop://localhost:1050 -cp . TestServer >> ..\pid
	@cd ..


stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

clean:
	if exist IDL del /s /q IDL
	if exist bin del /s /q bin
	if exist IDL rmdir /s /q IDL
	if exist bin rmdir /s /q bin
