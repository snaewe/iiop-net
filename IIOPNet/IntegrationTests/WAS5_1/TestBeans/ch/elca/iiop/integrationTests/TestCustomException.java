package ch.elca.iiop.integrationTests;

public class TestCustomException extends Exception {

    private int m_number;

    public TestCustomException(String msg, int number) {
        super(msg);
        m_number = number;
    }

    public int GetNumber() {
        return m_number;
    }

}