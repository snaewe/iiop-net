IF exist nameserviceIOR del nameserviceIOR
start %TAO_ROOT%\orbsvcs\Naming_Service\Naming_Service.exe -p pid -o nameserviceIOR -ORBEndPoint iiop://localhost:12345
..\..\..\..\..\IntegrationTests\Utils\delay.exe 5
start %TAO_ROOT%\orbsvcs\Event_Service\Event_Service.exe -ORBInitRef NameService=file://nameserviceIOR
..\..\..\..\..\IntegrationTests\Utils\delay.exe 5
EchoEventSupplier.exe -ORBInitRef NameService=file://nameserviceIOR