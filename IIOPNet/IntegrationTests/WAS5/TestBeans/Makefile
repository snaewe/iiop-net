include ..\..\..\MakeVars

LAUNCH = ..\..\..\Utils\launch.exe
DELAY = ..\..\..\Utils\delay.exe
KILL = ..\..\..\Utils\kill.exe

ANT_ARGS=-Dbasedir="." -Dwas_home="$(WAS_HOME)" 
JAVA_HOME=$(WAS_HOME)\java 
ANTCLASSPATH="$(JAVA_HOME)\lib\tools.jar";"$(WAS_HOME)\lib\xerces.jar";"$(WAS_HOME)\lib\j2ee.jar";"$(WAS_HOME)\lib\ant.jar"

build: build-app build-idl

build-app: 
	"$(JAVA_HOME)\bin\java" -classpath $(ANTCLASSPATH) org.apache.tools.ant.Main $(ANT_ARGS) buildApp
	@endlocal

build-idl: 
	"$(JAVA_HOME)\bin\java" -classpath $(ANTCLASSPATH) org.apache.tools.ant.Main $(ANT_ARGS) buildIdl
	@endlocal


deploy-bean:
	@"%WAS_HOME%\bin\wsadmin" -conntype none -c "$$AdminApp install bin/WAS5Integration.ear {-appname WAS5Integration -usedefaultbindings -deployejb }"

undeploy-bean:
	@%WAS_HOME%\bin\wsadmin -conntype none -c "$$AdminApp uninstall WAS5Integration"

start-server: undeploy-bean deploy-bean
        @echo Starting WAS5 in default configuration (server1)
        @call $(WAS_HOME)\bin\startServer.bat server1

stop-server: stop undeploy-bean

stop:
	@echo stopping WAS5
        @call $(WAS_HOME)\bin\stopServer.bat server1

clean:
        if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
	if exist IDL del /s /q IDL
	if exist IDL rmdir /s /q IDL
