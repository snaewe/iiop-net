
Utils = launch.exe kill.exe delay.exe currenttime.exe

.SUFFIXES : .exe .cs

build: $(Utils)

.cs.exe:
	csc $(CSFLAGS) $<

clean:
	if exist kill.exe del kill.exe
	if exist launch.exe del launch.exe
	if exist delay.exe del delay.exe
	if exist currenttime.exe del currenttime.exe
