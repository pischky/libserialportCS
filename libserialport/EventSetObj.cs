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

using sp_event     = libserialport.Libserialport.sp_event;
using sp_event_set = libserialport.Libserialport.sp_event_set;
using sp_port      = libserialport.Libserialport.sp_port;
using sp_return    = libserialport.Libserialport.sp_return;

namespace libserialport
{
    unsafe public class EventSetObj : IDisposable
    {

        private sp_event_set* eventSet = null;

        #region constructors, dispose, destructors

        public EventSetObj()
        {
            sp_return result;
            fixed (sp_event_set** p = &eventSet)
            {
                result = Libserialport.sp_new_event_set(p);
            }
            throwExceptionWhenNegative(result);
        }

        public EventSetObj(sp_event_set* eventSet) //TODO really needed?
        {
            if (eventSet == null) throw new ArgumentNullException("eventSet");
            this.eventSet = eventSet;  // we now have ownership
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EventSetObj()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // The full disposal treatment
                if (eventSet != null)
                {
                }
            }
            // Bare-bones disposal
            if (eventSet != null)
            {
                Libserialport.sp_free_event_set(eventSet);
                eventSet = null;
            }
            // ...
        }

        #endregion

        #region properties
        #endregion

        #region methods

        public void addPortEvents(SerialPortObj port, sp_event mask)
        {
            this.throwExceptionWhenDisposed();
            if (port == null) throw new ArgumentNullException("port"); // garanties also that port.Port!=null
            sp_return result = Libserialport.sp_add_port_events(eventSet, 
                                                               port.Port, mask);
            throwExceptionWhenNegative(result);
        }

        public void wait(UInt32 timeoutMs)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_wait(eventSet, timeoutMs);
            // result is allways SP_OK independet of timeout or not
            // Console.WriteLine("wait({0}): result={1}", timeoutMs, result);
            throwExceptionWhenNegative(result);
        }

        #endregion

        #region helper: throwExceptionWhenDisposed(), throwExceptionWhenNegative()

        private void throwExceptionWhenDisposed()
        {
            if (eventSet == null)
            {
                throw new ObjectDisposedException(
                                                 "EventSetObj alread disposed");
            }
        }

        protected static void throwExceptionWhenNegative(sp_return result)
        {
            SerialPortObj.throwExceptionWhenNegative(result);
        }

        #endregion methods

    }
}
