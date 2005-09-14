import org.omg.CosNaming.*;
import org.omg.PortableServer.*;
import org.omg.CORBA.*;
import java.util.Properties;
import java.io.*;
import Ch.Elca.Iiop.Tutorial.GettingStarted.*;
import Ch.Elca.Iiop.*;


/**
 * Simple client using a .NET object registered in an external name service
 *
 */
public class Client {

    public static void main (String[] args) {
        try {
            Properties props = new Properties();
            props.put("ORBInitRef.NameService", "corbaloc::127.0.0.1:8099/NameService");
            org.omg.CORBA.ORB orb = org.omg.CORBA.ORB.init( new String[0], props );

            Adder adder = ResolveAdderService(orb);

            BufferedReader reader = new BufferedReader(new InputStreamReader(System.in));
            System.out.println("first argument");
            double arg1 = Double.parseDouble(reader.readLine());
            System.out.println("second argument");
            double arg2 = Double.parseDouble(reader.readLine());
            double result = adder.Add(arg1, arg2);
            System.out.println("result: " + result);
        } catch (Exception ex) {
            System.out.println("exception: " + ex);
        }
    }

    private static Adder ResolveAdderService(org.omg.CORBA.ORB orb) throws Exception {
        org.omg.CosNaming.NameComponent[] serviceName = 
            new org.omg.CosNaming.NameComponent[1];
        serviceName[0] = new NameComponent("adder", "");
        org.omg.CosNaming.NamingContext nc = 
            org.omg.CosNaming.NamingContextHelper.narrow(
                orb.resolve_initial_references( "NameService" ));        

        org.omg.CORBA.Object service = nc.resolve(serviceName);
        return AdderHelper.narrow(service);
    }

}
