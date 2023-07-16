/*
 * This file is part of the libserialport project.
 *
 * Copyright (C) 2023 Martin Pischky <martin@pischky.de>
 *
 * Based on <c>libserialport.h</c>, which is
 *
 * Copyright (C) 2013, 2015 Martin Ling <martin-libserialport@earth.li>
 * Copyright (C) 2014 Uwe Hermann <uwe@hermann-uwe.de>
 * Copyright (C) 2014 Aurelien Jacobs <aurel@gnuage.org>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using IOException = System.IO.IOException;

namespace libserialport
{
    /// <summary>
    /// Exception thrown by SerialPortObj, PortConfigObj or EventSetObj.
    /// </summary>
    public class LibSerialPortException : IOException
    {

        public LibSerialPortException()
        :   base()
        {
        }

        public LibSerialPortException(string message)
        :   base(message)
        {
        }

        public LibSerialPortException(string message, int hresult)
        :   base(message, hresult)
        {
        }

        /*
        public string Message
        {
            get { return base.Message; }
        }
        */

        public new int HResult
        {
            get { return base.HResult; }
        }

    }
}
