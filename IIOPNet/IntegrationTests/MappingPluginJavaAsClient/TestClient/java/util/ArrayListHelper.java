package java.util;


public class ArrayListHelper {

  private static String  _id = "RMI:java.util.ArrayList:F655154F32815380:7881D21D99C7619D";

  public static String id ()
  {
    return _id;
  }

  public static java.util.ArrayList read (org.omg.CORBA.portable.InputStream istream)
  {
    return (java.util.ArrayList)((org.omg.CORBA_2_3.portable.InputStream) istream).read_value (java.util.ArrayList.class);
  }

  public static void write (org.omg.CORBA.portable.OutputStream ostream, java.util.ArrayList value)
  {
    ((org.omg.CORBA_2_3.portable.OutputStream) ostream).write_value (value, java.util.ArrayList.class);
  }    

}