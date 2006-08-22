include ..\..\MakeVars

DELAY = ..\..\Utils\delay.exe
PRCURRENTTIME = ..\..\Utils\currenttime.exe

build:
	cd TestBeans
	$(MAKE) build
	cd ..\TestClient
	$(MAKE) build
	cd ..

start-server:
        @cd TestBeans
        @$(MAKE) /C start-server
        @cd ..

stop-server:
        @cd TestBeans
        @$(MAKE) /C stop-server
        @cd ..

start-client:
        @cd TestClient
        @$(MAKE) /C /I start-client
        @cd ..

test:
        @echo Websphere 5.1 integration test starting at:
        @$(PRCURRENTTIME)
        @$(MAKE) /C start-server
        @$(DELAY) 5
        @$(MAKE) /C start-client
        @$(MAKE) /C stop-server

clean:
	cd TestBeans
	$(MAKE) clean
	cd ..\TestClient
	$(MAKE) clean
	cd ..
