include ..\..\..\..\MakeVars

Chatroom=ch\elca\iiop\demo\ejbChatroom\ 
JBOSS_CLP=%JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;.
IDL=IDL\ 
BIN=bin\ 



build: copy-meta-data build-classes build-stubs build-jar build-idl 

copy-meta-data: $(BIN)
        if not exist bin\META-INF mkdir bin\META-INF
	if exist *.xml copy *.xml bin\META-INF

build-classes: $(Chatroom)Chatroom.java $(Chatroom)ChatroomHome.java $(Chatroom)ChatroomBean.java $(Chatroom)ChatroomServer.java $(Chatroom)Message.java $(Chatroom)MessageListener.java $(Chatroom)NotRegisteredException.java $(Chatroom)AlreadyRegisteredException.java
	javac -classpath $(JBOSS_CLP) -d bin $?

build-stubs: $(Chatroom)MessageListener.java
	REM cd bin
	REM rmic -iiop ch.elca.iiop.demo.ejbChatroom.MessageListener
	REM cd ..

build-jar: $(BIN)
	cd bin
        jar cv0f chatroom_ejb.jar META-INF ch
        cd ..

$(IDL):
	if not exist $@nul mkdir $@

$(BIN):
	if not exist $@nul mkdir $@

build-idl: $(IDL)
	cd bin
	rmic -idl -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbChatroom.Chatroom
	rmic -idl -classpath %JBOSS_HOME%\server\all\lib\jboss-j2ee.jar;. -d ..\IDL ch.elca.iiop.demo.ejbChatroom.ChatroomHome
	cd ..

start-server: deploy-bean
        @echo Starting JBoss in configuration all
        @start $(JBOSS_HOME)\bin\run.bat -c All

deploy-bean:
	copy bin\chatroom_ejb.jar %JBOSS_HOME%\server\all\deploy


clean:
        if exist bin del /s /q bin
	if exist bin rmdir /s /q bin
	if exist IDL del /s /q IDL
	if exist IDL rmdir /s /q IDL
