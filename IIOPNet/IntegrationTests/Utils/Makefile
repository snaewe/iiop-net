
Utils = launch.exe kill.exe delay.exe

.SUFFIXES : .exe .cs

build: $(Utils)

.cs.exe:
	csc $<

clean:
	if exist kill.exe del kill.exe
	if exist launch.exe del launch.exe
	if exist delay.exe del delay.exe
