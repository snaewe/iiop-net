start orbd -ORBInitialPort 1050

start java -Djava.naming.factory.initial=com.sun.jndi.cosnaming.CNCtxFactory -Djava.naming.provider.url=iiop://localhost:1050 -cp . Server
