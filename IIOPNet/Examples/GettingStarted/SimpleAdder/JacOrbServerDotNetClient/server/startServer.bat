start %JACORB_HOME%\bin\jaco.bat org.jacorb.naming.NameServer -DOAPort=8091

pause

start %JACORB_HOME%\bin\jaco AdderServer -ORBInitRef.NameService=corbaloc::localhost:8091/StandardNS/NameServer-POA/_root
