IIOPChannel: bin\Delivery\IIOPChannel.dll
	

bin\Delivery\IIOPChannel.dll: *.cs
	if not exist .\bin\Delivery mkdir .\bin\Delivery\;
	csc $(CSFLAGS) /t:library /out:bin\Delivery\IIOPChannel.dll *.cs 
