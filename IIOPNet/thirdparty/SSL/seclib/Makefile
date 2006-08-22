include ..\..\..\MakeVars
Key = ..\..\..\Key.snk
Dest = ..\bin\ 

build: build-dll

build-dll: $(Dest)Org.Mentalis.Security.dll

$(Dest)Org.Mentalis.Security.dll: *.cs Security\*.cs Security\Certificates\*.cs Security\Cryptography\*.cs Security\Ssl\*.cs Security\Ssl\Shared\*.cs Security\Ssl\Tls1\*.cs Security\Ssl\Ssl3\*.cs $(Key)
    if not exist $(Dest)nul mkdir $(Dest)     
    $(CSC) $(CSFLAGS) /t:library /out:$(Dest)Org.Mentalis.Security.dll *.cs Security\*.cs Security\Certificates\*.cs Security\Cryptography\*.cs Security\Ssl\*.cs Security\Ssl\Shared\*.cs Security\Ssl\Tls1\*.cs Security\Ssl\Ssl3\*.cs
    
clean:
    if exist $(Dest)*.dll del /s /q $(Dest)*.dll
    if exist $(Dest)*.pdb del /s /q $(Dest)*.pdb
    if exist $(Dest)nul rmdir /s /q $(Dest) 
