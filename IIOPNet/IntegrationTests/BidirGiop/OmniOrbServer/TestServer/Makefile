include ..\..\..\..\MakeVars
!include .\MakeVars
RELATIVE=$(OMNIORB_HOME)
DELAY=..\..\..\..\Utils\delay.exe
LAUNCH=..\..\..\..\Utils\launch.exe
KILL=..\..\..\..\Utils\kill.exe


build: omniorb_home_var server.exe

server.exe: service.hh serviceSK.obj server.obj serviceDynSK.obj 
    $(LD) $(LDFLAGS) /OUT:$@ serviceSK.obj serviceDynSK.obj server.obj $(LDLIBS)

server.cc : service.hh


service.hh serviceSK.cc serviceDynSK.cc : service.idl $(IDLGEN)
    $(IDL) -Wba service.idl

start-server: omniorb_home_var
    IF EXIST omninames* del /q omninames*
    $(LAUNCH) $(OMNIORB_HOME)\bin\x86_win32\omniNames.exe -start 11356 -logdir . >> pid
    $(DELAY) 5
    $(LAUNCH) server.exe -ORBInitRef NameService=corbaloc::localhost:11356/NameService -ORBendPoint giop:::20001 -ORBtraceLevel 31 -ORBacceptBiDirectionalGIOP 1 -ORBserverTransportRule \"* unix,tcp,bidir\" >> pid
    
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
