include ..\..\..\..\MakeVars

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

build-java: junit-var JacORBHome compile-java

compile-java: *.java
    javac -classpath .;$(JACORB_HOME)\classes;$(JUNIT_JAR) $?

start-client: junit-var JacORBHome
    @echo "JacORB client IntegrationTest"
    $(JACORB_HOME)\bin\jaco -cp .;%JUNIT_HOME%\junit.jar TestClient

clean:
    if exist IDL\nul rmdir /q /s IDL
    if exist org\nul rmdir /q /s org
    if exist *.class del /q *.class
    if exist Callback*.java del /q Callback*.java
    if exist TestService*.java del /q TestService*.java
    if exist _*.java del /q _*.java


