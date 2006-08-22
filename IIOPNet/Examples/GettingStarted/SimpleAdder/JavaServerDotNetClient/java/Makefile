include ..\..\..\..\..\MakeVars

IDLType	    = ch.elca.iiop.tutorial.gettingstarted.AdderImpl
IDLPath     = ch\elca\iiop\tutorial\gettingstarted\ 
IMPLPath    = $(IDLPath)AdderImpl
IFPath      = $(IDLPath)Adder
RMISTUB     = rmic
RMI2IDL     = rmic
IDL         = IDL\ 

build: build-server build-idl

build-server: *.java $(IFPath).java $(IMPLPath).java
	javac -classpath . $?
	$(RMISTUB) -iiop -classpath . $(IDLType)

$(IDL):
	if not exist $@nul mkdir $@

build-idl: $(IDL) $(IDL)$(IFPath).idl $(IMPLPath).java

$(IDL)$(IFPath).idl: $(IMPLPath).java 
	$(RMI2IDL) -idl -d $(IDL) $(IDLType)

clean:
	if exist $(IDL)nul rmdir /q /s $(IDL)
        if exist *.class del /q *.class
	if exist $(IDLPath)*.class del /q $(IDLPath)*.class
