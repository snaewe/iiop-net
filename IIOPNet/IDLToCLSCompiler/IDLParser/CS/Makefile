include ..\..\..\MakeVars

IDLTOCLSCOMPILER=..\..\IDLCompiler\ 

all: build deploy


javaCCCs:
	IF NOT defined JAVACCCS_HOME echo define javaCC C# home
	IF NOT defined JAVACCCS_HOME exit 2

copyLastJJTFiles:
	@copy /y $(IDLTOCLSCOMPILER)Node.cs .
	@copy /y $(IDLTOCLSCOMPILER)SimpleNode.cs .
	@copy /y $(IDLTOCLSCOMPILER)SimpleNodeWithIdent.cs .
	@copy /y $(IDLTOCLSCOMPILER)AST*.cs .


build: javaCCCs copyLastJJTFiles
	%JAVACCCS_HOME%\bin\jjtree.bat IDL.jjt
	%JAVACCCS_HOME%\bin\javacc.bat IDL.jj


deploy:
	xcopy /D /Y *.cs $(IDLTOCLSCOMPILER)
	@echo updated files in IDL to CLS compiler directory
clean:
	IF EXIST *.cs del /q *.cs
	IF EXIST *.jj del /q *.jj