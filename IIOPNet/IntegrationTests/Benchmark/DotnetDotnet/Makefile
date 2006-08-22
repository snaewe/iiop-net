include ..\..\..\MakeVars

DELAY = ..\..\..\Utils\delay.exe

build: build-server build-client

build-server:
    cd BenchmarkServer
    $(MAKE) build
        cd ..

build-client:
    cd BenchmarkClient
    $(MAKE) build   
    cd ..

start-server:
    @cd BenchmarkServer
    @$(MAKE) /C start-server
    @cd ..

stop-server:
    @cd BenchmarkServer
    @$(MAKE) /C stop-server
    @cd ..  

start-client:
    @cd BenchmarkClient
    @$(MAKE) /C /I start-client
    @cd ..

benchmark: build
    @$(MAKE) /C start-server
    @$(DELAY) 10
    @$(MAKE) /C start-client
    @$(MAKE) /C stop-server
    
    
clean:
    cd BenchmarkServer
    $(MAKE) clean
    cd ..
    cd BenchmarkClient
    $(MAKE) clean   
    cd ..
