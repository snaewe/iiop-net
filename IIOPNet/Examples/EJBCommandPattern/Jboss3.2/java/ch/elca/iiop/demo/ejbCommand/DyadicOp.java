/* 
 * DyadicOp.java
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 14.03.04  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
 *
 * Copyright 2003 ELCA Informatique SA
 * Av. de la Harpe 22-24, 1000 Lausanne 13, Switzerland
 * www.elca.ch
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

package ch.elca.iiop.demo.ejbCommand;

import java.io.Serializable;


/**
 * This is the base class of all arithmetic ops with two operands. 
 **/
public abstract class DyadicOp extends ArithOp implements Serializable {

    private int m_operand1;
    private int m_operand2;
    
    public DyadicOp() {
        super();
    }
    
    /**
     * sets the first operand.
     **/
    public void setOperand1(int op1) {
        m_operand1 = op1;
    }
    
    public int getOperand1() {
        return m_operand1;
    }
    
    /**
     * sets the second operand.
     **/
    public void setOperand2(int op2) {
        m_operand2 = op2;
    }
    
    public int getOperand2() {
        return m_operand2;
    }       

}
