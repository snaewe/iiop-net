
build: launch.exe kill.exe delay.exe

launch.exe: launch.cs
	csc launch.cs

kill.exe: kill.cs
	csc kill.cs

delay.exe: delay.cs
	csc delay.cs

clean:
	if exist kill.exe del kill.exe
	if exist launch.exe del launch.exe
	if exist delay.exe del delay.exe
