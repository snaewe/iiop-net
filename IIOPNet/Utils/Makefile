include ..\MakeVars
Utils = launch.exe kill.exe delay.exe currenttime.exe

.SUFFIXES : .exe .cs

build: $(Utils)

.cs.exe:
	$(CSC) $(CSFLAGS) $<

clean:
	if exist *.exe del *.exe
	if exist *.pdb del *.pdb
