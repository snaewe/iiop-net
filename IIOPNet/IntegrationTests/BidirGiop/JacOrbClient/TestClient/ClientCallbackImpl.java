

public class ClientCallbackImpl extends CallBackPOA {

    private java.lang.String m_msg;

    public void call_back(java.lang.String msg) {
        m_msg = msg;        
    }

    public java.lang.String GetMsg() {
        return m_msg;
    }

}