!include .\MakeVars
RELATIVE=$(OMNIORB_HOME)
DELAY=..\..\utils\delay.exe
LAUNCH=..\..\utils\launch.exe
KILL=..\..\utils\kill.exe


build: omniorb_home_var server.exe

omniorb_home_var:
    @IF NOT DEFINED OMNIORB_HOME echo define OMNIORB_HOME first
    @IF NOT DEFINED OMNIORB_HOME exit 2
    
server.exe: service.hh serviceSK.obj server.obj serviceDynSK.obj
	$(LD) $(LDFLAGS) /OUT:$@ serviceSK.obj serviceDynSK.obj server.obj $(LDLIBS)


service.hh serviceSK.cc serviceDynSK.cc : service.idl $(IDLGEN)
	$(IDL) -Wba service.idl

start-server: omniorb_home_var
        IF EXIST omninames* del /q omninames*
	$(LAUNCH) $(OMNIORB_HOME)\bin\x86_win32\omniNames.exe -start 11356 -logdir . >> pid
	$(DELAY) 5
	$(LAUNCH) server.exe -ORBInitRef NameService=corbaloc::localhost:11356/NameService >> pid

stop-server:
	@for /F %%p in (pid) do @$(KILL) %%p
	@del pid

clean:
	IF EXIST serviceSK.cc del /q serviceSK.cc
        IF EXIST service.hh del /q service.hh 
        IF EXIST *.obj del /q *.obj 
        IF EXIST server.exe del /q server.exe 
        IF EXIST *~ del /q *~ 
	IF EXIST *.pdb del /q *.pdb
