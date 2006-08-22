import Ch.Elca.Iiop.IntegrationTests.TestServiceImpl;
import Ch.Elca.Iiop.IntegrationTests.TestService;
import Ch.Elca.Iiop.IntegrationTests.TestServiceHelper;
import org.omg.CORBA.ORB;
import org.omg.PortableServer.POA;
import org.omg.PortableServer.POAHelper;
import org.omg.CosNaming.NamingContextExt;
import org.omg.CosNaming.NamingContextExtHelper;
import org.omg.CosNaming.NameComponent;

public class TestServer {


    public static void main(String[] args) {
        try {
            // Initialize the ORB.
            ORB orb = ORB.init(args,null);

            POA rootPOA = POAHelper.narrow(orb.resolve_initial_references("RootPOA"));
            // activate the poa
            rootPOA.the_POAManager().activate();

            // Create a test object.
            TestServiceImpl test = 
                new TestServiceImpl(orb);
            // activate the object
            rootPOA.activate_object(test);

            // get object reference from the servant
            org.omg.CORBA.Object ref = rootPOA.servant_to_reference(test);
            TestService tsRef = TestServiceHelper.narrow(ref);
            BindInNameSerivce(tsRef, "test", orb);

            System.out.println("Server running");
            orb.run();
        } catch (Exception e) {
            System.err.println(
              "Stock server error: " + e);
            e.printStackTrace(System.out);
        }
    }

    private static void BindInNameSerivce(TestService tsRef, String name, ORB orb)
        throws Exception {      	  
        // get the root naming context
        // NameService invokes the name service
        org.omg.CORBA.Object objRef =
            orb.resolve_initial_references("NameService");
        // Use NamingContextExt which is part of the Interoperable
        // Naming Service (INS) specification.
        NamingContextExt ncRef = NamingContextExtHelper.narrow(objRef);

        // bind the Object Reference in Naming        
        NameComponent path[] = ncRef.to_name( name );
        System.out.println("name-service-name: ");
        for (int i = 0; i < path.length; i++) {
            System.out.println(path[i].id + "\\" + path[i].kind); 
        }
        ncRef.rebind(path, tsRef);
    }


}