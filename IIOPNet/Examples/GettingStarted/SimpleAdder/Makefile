include ..\..\..\MakeVars

build:
	cd DotNetServerJavaClient
	$(MAKE) build
	cd ..
        cd JavaServerDotNetClient
        $(MAKE) build
        cd ..
	cd DotNetServerDotNetClient
	$(MAKE) build
	cd ..

clean:
	cd DotNetServerJavaClient
	$(MAKE) clean
	cd ..
        cd JavaServerDotNetClient
        $(MAKE) clean
        cd ..
        cd DotNetServerDotNetClient
        $(MAKE) clean
        cd ..
