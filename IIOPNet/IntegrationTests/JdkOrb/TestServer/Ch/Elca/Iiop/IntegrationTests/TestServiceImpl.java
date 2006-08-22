package Ch.Elca.Iiop.IntegrationTests;

import org.omg.CORBA.ORB;
import org.omg.CORBA.Any;

public class TestServiceImpl extends TestServicePOA {


    private ORB m_orb;

    public TestServiceImpl(ORB orb) {
        m_orb = orb;
    }

    public int EchoLong(int arg) {
        return arg;
    }

    public Any StringArrayAsAny(String arg1, String arg2) {
        String[] array = new String[]{arg1, arg2}; 
        Any result = m_orb.create_any(); 
        result.insert_Value(array); 
        return result;        
    }
        


}