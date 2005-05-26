JUNIT_JAR = $(JUNIT_HOME)\junit.jar
IDL         = IDL\ 

build: build-idl build-java

copy-idl: $(IDL)service.idl

$(IDL)service.idl: ..\TestServer\service.idl
    if not exist $(IDL)nul mkdir $(IDL)
    copy ..\TestServer\service.idl $(IDL)

build-idl: $(IDL)service.idl JacORBHome
    copy ..\..\..\..\IDL\*.idl IDL
    cd IDL
    $(JACORB_HOME)\bin\idl -d .. service.idl 
    cd ..

junit-jar:
    if not defined JUNIT_HOME echo define JUNIT_HOME first (e.g. C:\JUNIT\Junit3.8.1)
    if not defined JUNIT_HOME exit 2

JacORBHome:
    if not defined JACORB_HOME echo define JACORB_HOME first (e.g. C:\JACORB)
    if not defined JACORB_HOME exit 2

build-java: junit-jar JacORBHome compile-java

compile-java: *.java
    javac -classpath .;$(JACORB_HOME)\classes;$(JUNIT_JAR) $?

start-client: junit-jar JacORBHome
    @echo "JacORB client IntegrationTest"
    $(JACORB_HOME)\bin\jaco -cp .;%JUNIT_HOME%\junit.jar TestClient

clean:
    if exist IDL\nul rmdir /q /s IDL
    if exist org\nul rmdir /q /s org
    if exist *.class del /q *.class
    if exist Callback*.java del /q Callback*.java
    if exist TestService*.java del /q TestService*.java
    if exist _*.java del /q _*.java


