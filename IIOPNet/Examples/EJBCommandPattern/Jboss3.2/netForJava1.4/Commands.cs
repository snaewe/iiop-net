/* Commands.cs
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


using System;


namespace ch.elca.iiop.demo.ejbCommand {

    [Serializable()]
    public class CommandImpl : Command {
        
        public CommandImpl() {
        }
    }
    
    
    [Serializable()]
    public class ArithOpImpl : ArithOp {
        
        public ArithOpImpl() {
        }
        
        public override int result {
            get {
                return m_result;
            }
            set {
                m_result = value;
            }
        }
        
    }
   
    
    [Serializable()]
    public class DyadicOpImpl : DyadicOp {
        
        public DyadicOpImpl() {
        }
        
        public override int result {
            get {
                return m_result;
            }
            set {
                m_result = value;
            }
        }
        
        public override int operand1 {
            get {
                return m_operand1;
            }
            set {
                m_operand1 = value;
            }
        }
        
        public override int operand2 {
            get {
                return m_operand2;
            }
            set {
                m_operand2 = value;
            }
        }
        
    }
    
    
    [Serializable()]
    public class SubOpImpl : SubOp {
        
        public SubOpImpl() {
        }
        
        public override int result {
            get {
                return m_result;
            }
            set {
                m_result = value;
            }
        }
        
        public override int operand1 {
            get {
                return m_operand1;
            }
            set {
                m_operand1 = value;
            }
        }
        
        public override int operand2 {
            get {
                return m_operand2;
            }
            set {
                m_operand2 = value;
            }
        }

    }
    
    
    [Serializable()]
    public class AddOpImpl : AddOp {
        
        public AddOpImpl() {
        }
        
        public override int result {
            get {
                return m_result;
            }
            set {
                m_result = value;
            }
        }
        
        public override int operand1 {
            get {
                return m_operand1;
            }
            set {
                m_operand1 = value;
            }
        }
        
        public override int operand2 {
            get {
                return m_operand2;
            }
            set {
                m_operand2 = value;
            }
        }

    }

    
}
