import org.omg.CORBA.*;
import org.omg.PortableServer.*;
import org.omg.CosNaming.*;
import Ch.Elca.Iiop.Tutorial.GettingStarted.*;


public class AdderServer {


    public static void main(String[] args) {
        try 
        {            
            //init ORB
	    ORB orb = ORB.init( args, null );

	    //init POA
	    POA poa = 
                POAHelper.narrow( orb.resolve_initial_references( "RootPOA" ));

	    poa.the_POAManager().activate();

            // create a Adder object
            AdderImpl adderImpl = new AdderImpl();
    
            // create the object reference
            org.omg.CORBA.Object adderRef = 
                poa.servant_to_reference( adderImpl );


            org.omg.CORBA.Object nsObject =
                orb.resolve_initial_references("NameService");
            NamingContextExt nc =
                NamingContextExtHelper.narrow( nsObject );

            nc.rebind(nc.to_name("Adder"), adderRef);
    
            // wait for requests
	    orb.run();
        } catch( Exception e ) {
            System.out.println( e );
        }        
    }

}