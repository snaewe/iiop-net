All : Channel Generator

Channel :
	@echo building IIOP channel
	@cd IIOPChannel
	@nmake -f Makefile IIOPChannel
	@cd ..

Generator :
	@echo building CLS to IDL generator
	@if not exist .\CLSToIDLGenerator\bin\Delivery mkdir .\CLSToIDLGenerator\bin\Delivery\ ;
	@copy .\IIOPChannel\bin\Delivery\IIOPChannel.* .\CLSToIDLGenerator\bin\Delivery\ ;
	@cd CLSToIDLGenerator
	@nmake -f Makefile CLSToIDLGenerator
	@cd ..

