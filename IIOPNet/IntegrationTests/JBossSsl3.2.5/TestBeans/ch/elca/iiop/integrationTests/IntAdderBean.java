/* IntAdderBean.java
 *
 * Project: IIOP.NET
 * Examples
 *
 * WHEN      RESPONSIBLE
 * 16.07.03  Dominic Ullmann (DUL), dominic.ullmann -at- elca.ch
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


package ch.elca.iiop.integrationTests;

import javax.ejb.CreateException;
import javax.ejb.SessionBean;
import javax.ejb.SessionContext;


/**
 * IntAdderBean is a stateless Session Bean. It implements the IntAdder functionality.
 */
public class IntAdderBean implements SessionBean {

    private SessionContext m_ctx;

 
    /**
     * This method is required by the EJB Specification,
     * but is not used here.
     *
     */
    public void ejbActivate() {
    }

    /**
     * This method is required by the EJB Specification,
     * but is not used here.
     *
     */
    public void ejbRemove() {
    }

    /**
     * This method is required by the EJB Specification,
     * but is not used by this example.
     *
     */
    public void ejbPassivate() {
    }

    /**
     * Sets the session context.
     *
     * @param ctx               SessionContext Context for session
     */
    public void setSessionContext(SessionContext ctx) {
        m_ctx = ctx;
    }

    /**
     * This method corresponds to the create method in the home interface
     * "TestHome.java".
     *
     */
    public void ejbCreate () throws CreateException {
        // nothing special to do here
    }

    public int add(int sum1, int sum2) {
        return sum1 + sum2;
    }

}

