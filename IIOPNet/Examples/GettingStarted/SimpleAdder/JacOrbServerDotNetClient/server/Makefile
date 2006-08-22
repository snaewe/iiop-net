include ..\..\..\..\..\MakeVars

IDL         = IDL\ 
IDLType     = Ch.Elca.Iiop.Tutorial.GettingStarted.Adder
IDLPath     = Ch\Elca\Iiop\Tutorial\GettingStarted\Adder
SERVEREXE   = $(bin)AdderServer.exe
COMMONDLL   = $(bin)Adder.dll

build: build-idl build-server

build-server: AdderServer.class Ch\Elca\Iiop\Tutorial\GettingStarted\AdderImpl.class

AdderServer.class Ch\Elca\Iiop\Tutorial\GettingStarted\AdderImpl.class: AdderServer.java Ch\Elca\Iiop\Tutorial\GettingStarted\AdderImpl.java
	javac AdderServer.java Ch\Elca\Iiop\Tutorial\GettingStarted\*.java

build-idl: Ch\Elca\Iiop\Tutorial\GettingStarted\AdderPoa.java Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.java

Ch\Elca\Iiop\Tutorial\GettingStarted\AdderPoa.java Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.java: $(IDL)Adder.idl
	%JACORB_HOME%\bin\idl $(IDL)Adder.idl

clean:
	if exist AdderServer.class del /q AdderServer.class
	if exist Ch\Elca\Iiop\Tutorial\GettingStarted\*.class del /q Ch\Elca\Iiop\Tutorial\GettingStarted\*.class
	if exist Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.java del /q Ch\Elca\Iiop\Tutorial\GettingStarted\Adder.java
	if exist Ch\Elca\Iiop\Tutorial\GettingStarted\AdderPoa*.java del /q Ch\Elca\Iiop\Tutorial\GettingStarted\AdderPoa*.java
	if exist Ch\Elca\Iiop\Tutorial\GettingStarted\AdderOperations.java del /q Ch\Elca\Iiop\Tutorial\GettingStarted\AdderOperations.java
	if exist Ch\Elca\Iiop\Tutorial\GettingStarted\AdderHelper.java del /q Ch\Elca\Iiop\Tutorial\GettingStarted\AdderHelper.java
	if exist Ch\Elca\Iiop\Tutorial\GettingStarted\AdderHolder.java del /q Ch\Elca\Iiop\Tutorial\GettingStarted\AdderHolder.java

	if exist Ch\Elca\Iiop\Tutorial\GettingStarted\_*.java del /q Ch\Elca\Iiop\Tutorial\GettingStarted\_*.java
