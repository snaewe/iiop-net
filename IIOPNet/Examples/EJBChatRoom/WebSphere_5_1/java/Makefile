include ..\..\..\..\MakeVars

ANT_ARGS=-Dbasedir="." -Dwas_home="$(WAS5_1_HOME)" 
JAVA_HOME=$(WAS5_1_HOME)\java 
ANTCLASSPATH="$(JAVA_HOME)\lib\tools.jar";"$(WAS5_1_HOME)\lib\xerces.jar";"$(WAS5_1_HOME)\lib\j2ee.jar";"$(WAS5_1_HOME)\lib\ant.jar"

build: build-app build-idl build-deploy

build-app: 
	"$(JAVA_HOME)\bin\java" -classpath $(ANTCLASSPATH) org.apache.tools.ant.Main $(ANT_ARGS) buildApp
	@endlocal

build-idl: 
	"$(JAVA_HOME)\bin\java" -classpath $(ANTCLASSPATH) org.apache.tools.ant.Main $(ANT_ARGS) buildIdl
	@endlocal

build-deploy:
	"$(WAS5_1_HOME)\bin\wsadmin" -conntype none -c "$$AdminApp install bin/WAS5EJBChatroom.ear {-appname WAS5EJBChatroom -usedefaultbindings -deployejb }"

undeploy:
	"$(WAS5_1_HOME)\bin\wsadmin" -conntype none -c "$$AdminApp uninstall WAS5EJBChatroom"

start-server: undeploy build-deploy
        @echo Starting WAS5 in default configuration (server1)
        @call $(WAS5_1_HOME)\bin\startServer.bat server1

stop-server:
	@echo stopping WAS5
	@call $(WAS5_1_HOME)\bin\stopServer.bat server1	

clean:
	"$(WAS5_1_HOME)\bin\wsadmin" -conntype none -c "$$AdminApp uninstall WAS5EJBChatroom"
        if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
	if exist IDL del /s /q IDL
	if exist IDL rmdir /s /q IDL
