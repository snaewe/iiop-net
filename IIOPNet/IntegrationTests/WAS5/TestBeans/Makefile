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

clean:
        if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
	if exist IDL del /s /q IDL
	if exist IDL rmdir /s /q IDL
