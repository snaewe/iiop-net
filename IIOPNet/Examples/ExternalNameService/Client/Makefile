include ..\..\..\MakeVars

IDL         = IDL\ 

build: build-idl build-java

copy-idl: $(IDL)Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.idl

$(IDL)Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.idl: ..\Server\$(IDL)Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.idl
    if not exist $(IDL)nul mkdir $(IDL)
    xcopy /s ..\Server\IDL $(IDL)

build-idl: $(IDL)Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.idl JacORBHome
    copy $(JACORB_HOME)\idl\omg\orb.idl IDL
    cd IDL
    $(JACORB_HOME)\bin\idl -d .. -I. Ch\Elca\Iiop\GenericUserException.idl 
    $(JACORB_HOME)\bin\idl -d .. -I. Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.idl 
    cd ..

JacORBHome:
    if not defined JACORB_HOME echo define JACORB_HOME first (e.g. C:\JACORB)
    if not defined JACORB_HOME exit 2

build-java: JacORBHome compile-java

compile-java: *.java
    javac -classpath .;$(JACORB_HOME)\classes $?

start-client: JacORBHome
    @echo "JacORB client"
    $(JACORB_HOME)\bin\jaco -cp . Client

clean:
    if exist IDL\nul rmdir /q /s IDL
    if exist org\nul rmdir /q /s org
    if exist Ch\nul rmdir /q /s Ch
    if exist *.class del /q *.class


