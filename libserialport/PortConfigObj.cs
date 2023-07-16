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

using sp_cts         = libserialport.Libserialport.sp_cts;
using sp_dtr         = libserialport.Libserialport.sp_dtr;
using sp_dsr         = libserialport.Libserialport.sp_dsr;
using sp_flowcontrol = libserialport.Libserialport.sp_flowcontrol;
using sp_parity      = libserialport.Libserialport.sp_parity;
using sp_port_config = libserialport.Libserialport.sp_port_config;
using sp_return      = libserialport.Libserialport.sp_return;
using sp_rts         = libserialport.Libserialport.sp_rts;
using sp_xonxoff     = libserialport.Libserialport.sp_xonxoff;

namespace libserialport
{
    unsafe public class PortConfigObj : IDisposable
    {

        private sp_port_config* config = null;

        #region constructors, dispose, destructors

        public PortConfigObj()
        {
            sp_return result;
            fixed (sp_port_config** p = &config)
            {
                result = Libserialport.sp_new_config(p);
            }
            throwExceptionWhenNegative(result);
        }

        internal PortConfigObj(sp_port_config* config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;  // we now have ownership
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PortConfigObj()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // The full disposal treatment
                if (config != null)
                {
                }
            }
            // Bare-bones disposal
            if (config != null)
            {
                Libserialport.sp_free_config(config);
                config = null;
            }
            // ...
        }

        #endregion

        #region properties

        /// <summary>
        /// Needed in PortConfigObj to access. Do not use elsewhere.
        /// </summary>
        internal sp_port_config* Config
        {
            get
            {
                this.throwExceptionWhenDisposed();
                return config;
            }
        }

        public Int32 Baudrate
        {
            get
            {
                this.throwExceptionWhenDisposed();
                Int32 baudrate;
                sp_return result = Libserialport.sp_get_config_baudrate(config, 
                                                                     &baudrate);
                throwExceptionWhenNegative(result);
                return baudrate;
            }
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_baudrate(config, 
                                                                        value);
                throwExceptionWhenNegative(result);
            }
        }

        public Int32 Bits
        {
            get
            {
                this.throwExceptionWhenDisposed();
                Int32 bits;
                sp_return result = Libserialport.sp_get_config_bits(config,
                                                                    &bits);
                throwExceptionWhenNegative(result);
                return bits;
            }
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_bits(config,
                                                                         value);
                throwExceptionWhenNegative(result);
            }
        }

        public sp_parity Parity
        {
            get
            {
                this.throwExceptionWhenDisposed();
                sp_parity parity;
                sp_return result = Libserialport.sp_get_config_parity(config,
                                                                      &parity);
                throwExceptionWhenNegative(result);
                return parity;
            }
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_parity(config,
                                                                         value);
                throwExceptionWhenNegative(result);
            }
        }

        public Int32 Stopbits
        {
            get
            {
                this.throwExceptionWhenDisposed();
                Int32 stopbits;
                sp_return result = Libserialport.sp_get_config_stopbits(config,
                                                                     &stopbits);
                throwExceptionWhenNegative(result);
                return stopbits;
            }
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_stopbits(config,
                                                                        value);
                throwExceptionWhenNegative(result);
            }
        }

        public sp_rts Rts
        {
            get
            {
                this.throwExceptionWhenDisposed();
                sp_rts rts;
                sp_return result = Libserialport.sp_get_config_rts(config,
                                                                   &rts);
                throwExceptionWhenNegative(result);
                return rts;
            }
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_rts(config,
                                                                   value);
                throwExceptionWhenNegative(result);
            }
        }

        public sp_cts Cts
        {
            get
            {
                this.throwExceptionWhenDisposed();
                sp_cts cts;
                sp_return result = Libserialport.sp_get_config_cts(config,
                                                                   &cts);
                throwExceptionWhenNegative(result);
                return cts;
            }
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_cts(config,
                                                                   value);
                throwExceptionWhenNegative(result);
            }
        }

        public sp_dtr Dtr
        {
            get
            {
                this.throwExceptionWhenDisposed();
                sp_dtr dtr;
                sp_return result = Libserialport.sp_get_config_dtr(config,
                                                                   &dtr);
                throwExceptionWhenNegative(result);
                return dtr;
            }
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_dtr(config,
                                                                   value);
                throwExceptionWhenNegative(result);
            }
        }

        public sp_dsr Dsr
        {
            get
            {
                this.throwExceptionWhenDisposed();
                sp_dsr dsr;
                sp_return result = Libserialport.sp_get_config_dsr(config,
                                                                   &dsr);
                throwExceptionWhenNegative(result);
                return dsr;
            }
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_dsr(config,
                                                                   value);
                throwExceptionWhenNegative(result);
            }
        }

        public sp_xonxoff XonXoff
        {
            get
            {
                this.throwExceptionWhenDisposed();
                sp_xonxoff xonXoff;
                sp_return result = Libserialport.sp_get_config_xon_xoff(config,
                                                                      &xonXoff);
                throwExceptionWhenNegative(result);
                return xonXoff;
            }
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_xon_xoff(config,
                                                                        value);
                throwExceptionWhenNegative(result);
            }
        }

        public sp_flowcontrol Flowcontrol
        {
            // no "sp_get_config_flowcontrol()" in libserialport.h
            //get
            //{
            //    this.throwExceptionWhenDisposed();
            //    sp_flowcontrol flowcontrol;
            //    sp_return result = Libserialport.sp_get_config_flowcontrol(config,
            //                                                          &flowcontrol);
            //    throwExceptionWhenNegative(result);
            //    return flowcontrol;
            //}
            set
            {
                this.throwExceptionWhenDisposed();
                sp_return result = Libserialport.sp_set_config_flowcontrol(config,
                                                                        value);
                throwExceptionWhenNegative(result);
            }
        }

        #endregion

        #region helper: throwExceptionWhenDisposed(), throwException(sp_return)

        private void throwExceptionWhenDisposed()
        {
            if (config == null)
            {
                throw new ObjectDisposedException(
                                               "PortConfigObj alread disposed");
            }
        }

        protected static void throwExceptionWhenNegative(sp_return result)
        {
            SerialPortObj.throwExceptionWhenNegative(result);
        }

        #endregion methods

    }
}
