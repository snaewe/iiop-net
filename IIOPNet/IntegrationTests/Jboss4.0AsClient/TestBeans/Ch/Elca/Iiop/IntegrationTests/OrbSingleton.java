package Ch.Elca.Iiop.IntegrationTests;

import java.util.Properties;
import org.omg.CORBA.*;

public class OrbSingleton { 

    private static OrbSingleton s_instance = null; 

    private ORB m_orb; 


    private OrbSingleton() { 
        try { 

            Properties pr = new Properties();
            pr.setProperty("org.omg.CORBA.ORBClass", "org.jacorb.orb.ORB");
            pr.setProperty("org.omg.CORBA.ORBSingletonClass", "org.jacorb.orb.ORBSingleton");

            m_orb = ORB.init(new String[0], pr); 

        } catch (SystemException ex) { 
            System.out.println("OrbSingleton() constructor: ORB.init() exception: " + 
            ex); 
        } 
    } 

    public static synchronized OrbSingleton getInstance() { 
       if (null == s_instance) { 
           s_instance = new OrbSingleton(); 
       } 
       return s_instance; 
    }

    public ORB getOrb() { 
        return m_orb; 
    } 


}

