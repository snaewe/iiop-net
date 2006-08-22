include ..\..\..\MakeVars

Test=ch\elca\iiop\integrationTests\ 
BIN=bin\ 
IDL=IDL\ 
TestPackage=ch.elca.iiop.integrationTests.
JBOSS_CLP=%JBOSS_HOME_4%\server\all\lib\jboss-j2ee.jar;.
LAUNCH = ..\..\..\Utils\launch.exe
DELAY = ..\..\..\Utils\delay.exe
KILL = ..\..\..\Utils\kill.exe


build: copy-meta-data build-classes build-jar build-idl

copy-meta-data: $(BIN)
	if not exist bin mkdir bin
        if not exist bin\META-INF mkdir bin\META-INF
	if exist *.xml copy *.xml bin\META-INF

build-classes: $(Test)Test.java $(Test)TestHome.java $(Test)TestBean.java $(Test)IntAdder.java $(Test)IntAdderHome.java $(Test)IntAdderBean.java $(Test)TestSerializableClassB1.java $(Test)TestSerializableClassB2.java $(Test)TestSerializableClassC.java $(Test)TestSerializableClassD.java $(Test)TestNonSerializableBaseClass.java
	javac -classpath $(JBOSS_CLP) -d bin $?

$(IDL):
	if not exist $@nul mkdir $@

$(BIN):
	if not exist $@nul mkdir $@

build-jar:
	cd bin
        jar cv0f jboss4-integration_ejb.jar META-INF ch
        cd ..

build-idl: $(IDL)
        cd bin
        rmic -idl -classpath %JBOSS_HOME_4%\server\all\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)TestHome
        rmic -idl -classpath %JBOSS_HOME_4%\server\all\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)Test 
        rmic -idl -classpath %JBOSS_HOME_4%\server\all\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)IntAdderHome
        rmic -idl -classpath %JBOSS_HOME_4%\server\all\lib\jboss-j2ee.jar;. -d ..\$(IDL) $(TestPackage)IntAdder 
        cd ..


deploy-bean:
	@copy bin\jboss4-integration_ejb.jar %JBOSS_HOME_4%\server\all\deploy\

undeploy-bean:
	@if exist %JBOSS_HOME_4%\server\all\deploy\jboss4-integration_ejb.jar del /q %JBOSS_HOME_4%\server\all\deploy\jboss4-integration_ejb.jar

start-server: deploy-bean
        @echo Starting JBoss in configuration all
        @$(LAUNCH) -d $(JBOSS_HOME_4)\bin run.bat -c All >> pid

stop-server: stop undeploy-bean

stop:
	@echo stopping JBoss
        @$(LAUNCH) -d $(JBOSS_HOME_4)\bin shutdown.bat -S >> pid
        @$(DELAY) 30
	@for /F %%p in (pid) do @$(KILL) %%p
	@del /q pid


clean:
        if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
	if exist IDL del /s /q IDL
	if exist IDL rmdir /s /q IDL
