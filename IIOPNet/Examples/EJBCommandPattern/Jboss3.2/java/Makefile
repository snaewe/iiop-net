include ..\..\..\..\MakeVars

CommandProc=ch\elca\iiop\demo\ejbCommand\ 
JBOSS_CLP=%JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;.
IDL=IDL\ 
BIN=bin\ 



build: copy-meta-data build-classes build-jar build-idl 

copy-meta-data: $(BIN)
        if not exist bin\META-INF mkdir bin\META-INF
	if exist *.xml copy *.xml bin\META-INF

build-classes: $(CommandProc)CommandTarget.java $(CommandProc)CommandTargetHome.java $(CommandProc)CommandTargetBean.java $(CommandProc)ArithOp.java $(CommandProc)DyadicOp.java $(CommandProc)AddOp.java $(CommandProc)SubOp.java $(CommandProc)Command.java $(CommandProc)CommandException.java
	javac -classpath $(JBOSS_CLP) -d bin $?

build-jar: $(BIN)
	cd bin
        jar cv0f command_ejb.jar META-INF ch
        cd ..

$(IDL):
	if not exist $@nul mkdir $@

$(BIN):
	if not exist $@nul mkdir $@

build-idl: $(IDL)
	cd bin
	rmic -idl -noValueMethods -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbCommand.Command
	rmic -idl -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbCommand.CommandTarget
	rmic -idl -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbCommand.CommandTargetHome		
	rmic -idl -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbCommand.ArithOp
	rmic -idl -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbCommand.DyadicOp
	rmic -idl -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbCommand.AddOp
	rmic -idl -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbCommand.SubOp
    rmic -idl -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbCommand.CommandException
	cd ..


start-server: deploy-bean
        @echo Starting JBoss in configuration all
        @start $(JBOSS_HOME)\bin\run.bat -c All
		
deploy-bean:
    copy bin\command_ejb.jar %JBOSS_HOME%\server\all\deploy

clean:
	if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
	if exist IDL del /s /q IDL
	if exist IDL rmdir /s /q IDL
