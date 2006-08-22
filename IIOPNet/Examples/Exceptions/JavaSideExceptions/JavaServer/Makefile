include ..\..\..\..\MakeVars

Package     = tutorial.
PackagePath = tutorial\ 
IMPLPath    = $(PackagePath)ServiceImpl
IFPath      = $(PackagePath)Service
IDLType     = $(Package)ServiceImpl
RMISTUB     = rmic
RMI2IDL     = rmic
IDL         = IDL\ 

build: build-server build-idl

build-server: *.java $(PackagePath)*.java 
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
    if exist $(PackagePath)*.class del /q $(PackagePath)*.class
    if exist $(IDLPath)*.class del /q $(IDLPath)*.class
