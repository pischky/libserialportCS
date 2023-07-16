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
using System.Collections.Generic; //List

using sp_buffer      = libserialport.Libserialport.sp_buffer;
using sp_cts         = libserialport.Libserialport.sp_cts;
using sp_dtr         = libserialport.Libserialport.sp_dtr;
using sp_dsr         = libserialport.Libserialport.sp_dsr;
using sp_flowcontrol = libserialport.Libserialport.sp_flowcontrol;
using sp_mode        = libserialport.Libserialport.sp_mode;
using sp_parity      = libserialport.Libserialport.sp_parity;
using sp_port        = libserialport.Libserialport.sp_port;
using sp_port_config = libserialport.Libserialport.sp_port_config;
using sp_return      = libserialport.Libserialport.sp_return;
using sp_rts         = libserialport.Libserialport.sp_rts;
using sp_signal      = libserialport.Libserialport.sp_signal;
using sp_transport   = libserialport.Libserialport.sp_transport;
using sp_xonxoff     = libserialport.Libserialport.sp_xonxoff;

namespace libserialport
{
    unsafe public class SerialPortObj : IDisposable
    {

        private sp_port* port = null;

        #region constructors, dispose, destructors

        /// <summary>
        /// Constructor from name.
        /// </summary>
        /// <param name="portName">
        /// [in] The OS-specific name of a serial port. Must not be NULL.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// portName is null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// invalid argument
        /// </exception>
        /// <exception cref="System.LibSerialPortException">
        /// libserialport returned error: sp_return.SP_ERR_FAIL
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// operation not supported
        /// </exception>
        /// <exception cref="System.OutOfMemoryException">
        /// out of memory
        /// </exception>
        public SerialPortObj(string portName)
        {
            if (portName == null) throw new ArgumentNullException("portName");
            sp_return result;
            fixed (sp_port** p = &port)
            {
                result = Libserialport.sp_get_port_by_name(portName, p);
            }
            throwExceptionWhenNegative(result);
            // port != null on success only
        }

        /// <summary>
        /// Constructor from sp_port*.
        /// </summary>
        /// <param name="port">
        /// [in] port Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// portName is null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// invalid argument
        /// </exception>
        /// <exception cref="System.LibSerialPortException">
        /// libserialport returned error: sp_return.SP_ERR_FAIL
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// operation not supported
        /// </exception>
        /// <exception cref="System.OutOfMemoryException">
        /// out of memory
        /// </exception>
        private SerialPortObj(sp_port* otherPort)
        {
            if (otherPort == null) throw new ArgumentNullException("otherPort");
            sp_return result;
            fixed (sp_port** p = &port)
            {
                result = Libserialport.sp_copy_port(otherPort, p);
            }
            throwExceptionWhenNegative(result);
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other"></param>
        public SerialPortObj(SerialPortObj other)
        :   this(other.Port)
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SerialPortObj()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // The full disposal treatment
                if (port != null)
                {
                    //sp_return result = Libserialport.sp_close(port);
                    //// result == sp_return.SP_ERR_ARG when aready closed
                }
            }
            // Bare-bones disposal
            if (port != null)
            {
                Libserialport.sp_free_port(port);
                port = null;
            }
            // ...
        }

        #endregion

        #region properties

        /// <summary>
        /// Needed in EventSetObj to access. Do not use elsewhere.
        /// </summary>
        internal sp_port* Port
        {
            get
            {
                this.throwExceptionWhenDisposed();
                return port;
            }
        }

        public string Name
        {
            get
            {
                this.throwExceptionWhenDisposed();
                return Libserialport.sp_get_port_name(port);
            }
        }

        public string Description
        {
            get
            {
                this.throwExceptionWhenDisposed();
                return Libserialport.sp_get_port_description(port);
            }
        }

        public sp_transport Transport
        {
            get
            {
                this.throwExceptionWhenDisposed();
                return Libserialport.sp_get_port_transport(port);
            }
        }

        /// <summary>
        /// Get the USB bus of a USB serial adapter port.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// When no USB device (PortTransport != sp_transport.SP_TRANSPORT_USB
        /// </exception>
        public int UsbBus
        {
            get
            {
                this.throwExceptionWhenDisposed();
                int usbBus;
                sp_return result = Libserialport.sp_get_port_usb_bus_address(
                                                           port, &usbBus, null);
                throwExceptionWhenNegative(result);
                return usbBus;
            }
        }

        /// <summary>
        /// Get the USB address on bus of a USB serial adapter port.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// When no USB device (PortTransport != sp_transport.SP_TRANSPORT_USB
        /// </exception>
        public int UsbAddress
        {
            get
            {
                this.throwExceptionWhenDisposed();
                int usbAddress;
                sp_return result = Libserialport.sp_get_port_usb_bus_address(
                                                       port, null, &usbAddress);
                throwExceptionWhenNegative(result);
                return usbAddress;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// When no USB device (PortTransport != sp_transport.SP_TRANSPORT_USB
        /// </exception>
        public int UsbVendorId
        {
            get
            {
                this.throwExceptionWhenDisposed();
                int vid;
                sp_return result = Libserialport.sp_get_port_usb_vid_pid(
                                                              port, &vid, null);
                throwExceptionWhenNegative(result);
                return vid;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// When no USB device (PortTransport != sp_transport.SP_TRANSPORT_USB
        /// </exception>
        public int UsbProductId
        {
            get
            {
                this.throwExceptionWhenDisposed();
                int pid;
                sp_return result = Libserialport.sp_get_port_usb_vid_pid(
                                                              port, null, &pid);
                throwExceptionWhenNegative(result);
                return pid;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <para>
        /// <c>null</c> is returned when device is no USB device.
        /// </para>
        public string UsbManufacturer
        {
            get
            {
                this.throwExceptionWhenDisposed();
                return Libserialport.sp_get_port_usb_manufacturer(port);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <para>
        /// <c>null</c> is returned when device is no USB device.
        /// </para>
        public string UsbProduct
        {
            get
            {
                this.throwExceptionWhenDisposed();
                return Libserialport.sp_get_port_usb_product(port);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <para>
        /// <c>null</c> is returned when device is no USB device.
        /// </para>
        public string UsbSerial
        {
            get
            {
                this.throwExceptionWhenDisposed();
                return Libserialport.sp_get_port_usb_serial(port);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <para>
        /// <c>null</c> is returned when device is no USB device.
        /// </para>
        public string BluetoothAddress
        {
            get
            {
                this.throwExceptionWhenDisposed();
                return Libserialport.sp_get_port_bluetooth_address(port);
            }
        }

        public IntPtr Handle
        {
            get
            {
                this.throwExceptionWhenDisposed();
                IntPtr handle;
                sp_return result = Libserialport.sp_get_port_handle(
                                                              port, out handle);
                throwExceptionWhenNegative(result);
                return handle;
            }
        }

        #endregion

        #region methodes Port enumeration - Enumerating the serial ports of a system.

        public static SerialPortObj[] listPorts()
        {
            List<SerialPortObj> list = new List<SerialPortObj>();
            sp_port** portList = null; // TODO: this should be fixed
            try
            {
                sp_return result;
                result = Libserialport.sp_list_ports(&portList);
                //fixed (sp_port*** p = &portList)
                //    result = Libserialport.sp_list_ports(p);
                // ==> error CS0213: You cannot use the fixed statement to take
                //                   the address of an already fixed expression
                throwExceptionWhenNegative(result);

                /* Iterate through the ports. When portList[i] is null
                 * this indicates the end of the list. */
                for (int i = 0; portList[i] != null; i++)
                {
                    list.Add(new SerialPortObj(portList[i]));
                }
            }
            finally
            {
                if (portList != null) Libserialport.sp_free_port_list(portList);
            }
            return list.ToArray(); // or IList<SerialPortObj> as return type
        }

        #endregion

        #region methods Ports handling - Opening, closing and querying ports.

        /// <summary>
        /// Open the specified serial port.
        /// </summary>
        /// <param name="flags">
        /// [in] Flags to use when opening the serial port.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// invalid argument
        /// </exception>
        /// <exception cref="System.LibSerialPortException">
        /// libserialport returned error: sp_return.SP_ERR_FAIL
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// operation not supported
        /// </exception>
        /// <exception cref="System.OutOfMemoryException">
        /// out of memory
        /// </exception>
        public void open(sp_mode flags)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_open(port, flags);
            throwExceptionWhenNegative(result);
        }

        /// <summary>
        /// Open the specified serial port in read/write mode.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// invalid argument
        /// </exception>
        /// <exception cref="System.LibSerialPortException">
        /// libserialport returned error: sp_return.SP_ERR_FAIL
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// operation not supported
        /// </exception>
        /// <exception cref="System.OutOfMemoryException">
        /// out of memory
        /// </exception>
        public void open()
        {
            this.open(sp_mode.SP_MODE_READ_WRITE);
        }

        /// <summary>
        /// Close the specified serial port.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// invalid argument
        /// </exception>
        /// <exception cref="System.LibSerialPortException">
        /// libserialport returned error: sp_return.SP_ERR_FAIL
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// operation not supported
        /// </exception>
        /// <exception cref="System.OutOfMemoryException">
        /// out of memory
        /// </exception>
        public void close()
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_close(port);
            throwExceptionWhenNegative(result);
        }

        #endregion

        #region methods Configuration - Setting and querying serial port parameters.

        public PortConfigObj getConfig() // TODO or property ??
        {
            this.throwExceptionWhenDisposed();
            sp_port_config* config = null;
            try
            {
                sp_return result;
                result = Libserialport.sp_new_config(&config);
                throwExceptionWhenNegative(result);
                result = Libserialport.sp_get_config(port, config);
                throwExceptionWhenNegative(result);
                PortConfigObj retVal = new PortConfigObj(config);
                config = null; // ownership taken by retVal
                return retVal;
            }
            finally
            {
                if (config != null) Libserialport.sp_free_config(config);
            }
        }

        public void setConfig(PortConfigObj config)
        {
            this.throwExceptionWhenDisposed();
            if (config == null) throw new ArgumentNullException("config");
            sp_return result = Libserialport.sp_set_config(port, config.Config);
            throwExceptionWhenNegative(result);
        }

        public void setBaudrate(Int32 baudrate)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_baudrate(port, baudrate);
            throwExceptionWhenNegative(result);
        }

        public void setBits(Int32 bits)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_bits(port, bits);
            throwExceptionWhenNegative(result);
        }

        public void setParity(sp_parity parity)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_parity(port, parity);
            throwExceptionWhenNegative(result);
        }

        public void setStopbits(Int32 stopbits)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_stopbits(port, stopbits);
            throwExceptionWhenNegative(result);
        }

        public void setRts(sp_rts rts) {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_rts(port, rts);
            throwExceptionWhenNegative(result);
        }

        public void setCts(sp_cts cts)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_cts(port, cts);
            throwExceptionWhenNegative(result);
        }

        public void setDtr(sp_dtr dtr)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_dtr(port, dtr);
            throwExceptionWhenNegative(result);
        }

        public void setDsr(sp_dsr dsr)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_dsr(port, dsr);
            throwExceptionWhenNegative(result);
        }

        public void setXonXoff(sp_xonxoff xonXoff)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_xon_xoff(port, xonXoff);
            throwExceptionWhenNegative(result);
        }

        public void setFlowcontrol(sp_flowcontrol flowcontrol)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_set_flowcontrol(port, 
                                                                flowcontrol);
            throwExceptionWhenNegative(result);
        }

        #endregion

        #region Data handling - Reading, writing, and flushing data.

        /// <summary>
        /// Read bytes from the specified serial port, blocking until complete.
        /// </summary>
        /// <para>
        /// <stron>warning</strong> 
        /// If your program runs on Unix, defines its own signal handlers, and
        /// needs to abort blocking reads when these are called, then you
        /// should not use this function. It repeats system calls that return
        /// with EINTR. To be able to abort a read from a signal handler, you
        /// should implement your own blocking read using sp_nonblocking_read()
        /// together with a blocking method that makes sense for your program.
        /// E.g. you can obtain the file descriptor for an open port using
        /// sp_get_port_handle() and use this to call select() or pselect(),
        /// with appropriate arrangements to return if a signal is received.
        /// </para>
        /// <param name="buffer">
        /// [out] Buffer in which to store the bytes read. Must not be NULL.
        /// </param>
        /// <param name="count">
        /// [in] Requested number of bytes to read.
        /// </param>
        /// <param name="timeoutMs">
        /// [in] Timeout in milliseconds, or zero to wait indefinitely.
        /// </param>
        /// <returns>
        /// The number of bytes read on success. If
        /// the number of bytes returned is less than that requested, the
        /// timeout was reached before the requested number of bytes was
        /// available. If timeout is zero, the function will always return
        /// either the requested number of bytes or a negative error code.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// invalid argument
        /// </exception>
        /// <exception cref="System.LibSerialPortException">
        /// libserialport returned error: sp_return.SP_ERR_FAIL
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// operation not supported
        /// </exception>
        /// <exception cref="System.OutOfMemoryException">
        /// out of memory
        /// </exception>
        public int blockingRead(byte[] buffer, int count, UInt32 timeoutMs)
        {
            this.throwExceptionWhenDisposed();
            if (buffer == null) throw new ArgumentNullException("buffer");
            int result = Libserialport.sp_blocking_read(port, buffer, 
                                                        count, timeoutMs);
            throwExceptionWhenNegative((sp_return)result);
            return result;
        }

        public int blockingRead(byte[] buffer, UInt32 timeoutMs)
        {
            return this.blockingRead(buffer, buffer.Length, timeoutMs);
        }
     
        public int blockingReadNext(byte[] buffer, int count, UInt32 timeoutMs)
        {
            this.throwExceptionWhenDisposed();
            if (buffer == null) throw new ArgumentNullException("buffer");
            int result = Libserialport.sp_blocking_read_next(port, buffer,
                                                             count, timeoutMs);
            throwExceptionWhenNegative((sp_return)result);
            return result;
        }

        public int blockingReadNext(byte[] buffer, UInt32 timeoutMs)
        {
            return this.blockingReadNext(buffer, buffer.Length, timeoutMs);
        }

        /// <summary>
        /// Read bytes from the specified serial port, returning as soon as 
        /// any data is available.
        /// </summary>
        /// <param name="buffer">
        /// [out] Buffer in which to store the bytes read. Must not be NULL.
        /// </param>
        /// <param name="count">
        /// [in] Requested number of bytes to read.
        /// </param>
        /// <returns>
        /// The number of bytes read on success, or a negative error code. The
        /// number of bytes returned may be any number from zero to the maximum
        /// that was requested.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// invalid argument
        /// </exception>
        /// <exception cref="System.LibSerialPortException">
        /// libserialport returned error: sp_return.SP_ERR_FAIL
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// operation not supported
        /// </exception>
        /// <exception cref="System.OutOfMemoryException">
        /// out of memory
        /// </exception>
        public int nonblockingRead(byte[] buffer, int count)
        {
            this.throwExceptionWhenDisposed();
            if (buffer == null) throw new ArgumentNullException("buffer");
            int result = Libserialport.sp_nonblocking_read(port, buffer, count);
            throwExceptionWhenNegative((sp_return)result);
            return result;
        }

        public int nonblockingRead(byte[] buffer)
        {
            return this.nonblockingRead(buffer, buffer.Length);
        }

        public int blockingWrite(byte[] buffer, int count, UInt32 timeoutMs)
        {
            this.throwExceptionWhenDisposed();
            if (buffer == null) throw new ArgumentNullException("buffer");
            int result = Libserialport.sp_blocking_write(port, buffer,
                                                         count, timeoutMs);
            throwExceptionWhenNegative((sp_return)result);
            return result;
        }

        public int blockingWrite(byte[] buffer, UInt32 timeoutMs)
        {
            return this.blockingWrite(buffer, buffer.Length, timeoutMs);
        }

        public int nonblockingWrite(byte[] buffer, int count, UInt32 timeoutMs)
        {
            this.throwExceptionWhenDisposed();
            if (buffer == null) throw new ArgumentNullException("buffer");
            int result = Libserialport.sp_nonblocking_write(port, buffer,
                                                            count);
            throwExceptionWhenNegative((sp_return)result);
            return result;
        }

        public int nonblockingWrite(byte[] buffer, UInt32 timeoutMs)
        {
            return this.nonblockingWrite(buffer, buffer.Length, timeoutMs);
        }

        public int inputWaiting()
        {
            this.throwExceptionWhenDisposed();
            int result = Libserialport.sp_input_waiting(port);
            throwExceptionWhenNegative((sp_return)result);
            return result;
        }

        public int outputWaiting()
        {
            this.throwExceptionWhenDisposed();
            int result = Libserialport.sp_output_waiting(port);
            throwExceptionWhenNegative((sp_return)result);
            return result;
        }

        public void flush(sp_buffer buffers)
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_flush(port, buffers);
            throwExceptionWhenNegative(result);
        }

        public void drain()
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_drain(port);
            throwExceptionWhenNegative(result);
        }

        #endregion

        #region Waiting - Waiting for events and timeout handling.

        // TODO

        #endregion

        #region Signals - Port signalling operations.

        /// <summary>
        /// Gets the status of the control signals for the specified port.
        /// </summary>
        /// <returns>The status of the control signals</returns>
        /// <exception cref="System.ArgumentException">
        /// invalid argument
        /// </exception>
        /// <exception cref="System.LibSerialPortException">
        /// libserialport returned error: sp_return.SP_ERR_FAIL
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// operation not supported
        /// </exception>
        /// <exception cref="System.OutOfMemoryException">
        /// out of memory
        /// </exception>
        public sp_signal getSignals()
        {
            this.throwExceptionWhenDisposed();
            sp_signal signal_mask;
            sp_return result = Libserialport.sp_get_signals(port, &signal_mask);
            //fixed (sp_signal* p = &signal_mask)
            //{
            //    result = Libserialport.sp_get_signals(port, p);
            //    signal_mask = *p;
            //}
            // ==> error CS0213: You cannot use the fixed statement to take the 
            //                   address of an already fixed expression
            throwExceptionWhenNegative(result);
            return signal_mask;
        }

        /// <summary>
        /// Put the port transmit line into the break state.
        /// </summary>
        public void startBreak()
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_start_break(port);
            throwExceptionWhenNegative(result);
        }

        /// <summary>
        /// Take the port transmit line out of the break state.
        /// </summary>
        public void endBreak()
        {
            this.throwExceptionWhenDisposed();
            sp_return result = Libserialport.sp_end_break(port);
            throwExceptionWhenNegative(result);
        }

        #endregion

        #region helper: throwExceptionWhenDisposed(), throwException(sp_return)

        private void throwExceptionWhenDisposed()
        {
            if (port == null)
            {
                throw new ObjectDisposedException(
                                               "SerialPortObj alread disposed");
            }
        }

        // used by class PortConfigObj
        internal static void throwExceptionWhenNegative(sp_return result)
        {
            if (result >= sp_return.SP_OK)
            {
                return; // do nothing on result>=0
            }
            // Handle each of the four negative error codes that can be returned.
            switch (result)
            {
                case sp_return.SP_ERR_ARG:
                    /* When SP_ERR_ARG is returned, there was a problem with one
                     * or more of the arguments passed to the function, e.g. a 
                     * null pointer or an invalid value. This generally implies 
                     * a bug in the calling code. */
                    throw new ArgumentException("libserialport returned SP_ERR_ARG");

                case sp_return.SP_ERR_FAIL:
                    /* When SP_ERR_FAIL is returned, there was an error from the 
                     * OS, which we can obtain the error code and message for. 
                     * These calls must be made in the same thread as the call 
                     * that returned SP_ERR_FAIL, and before any other system 
                     * functions are called in that thread, or they may not 
                     * return the correct results. */
                    int errorCode = Libserialport.sp_last_error_code();
                    string errorMessage = Libserialport.sp_last_error_message();
                    throw new LibSerialPortException(errorMessage, (int) errorCode);

                case sp_return.SP_ERR_SUPP:
                    /* When SP_ERR_SUPP is returned, the function was asked to do
                     * something that isn't supported by the current OS or device,
                     * or that libserialport doesn't know how to do in the current
                     * version. */
                    throw new NotSupportedException(
                                     "operation not supported on OS or device");

                case sp_return.SP_ERR_MEM:
                    /* When SP_ERR_MEM is returned, libserialport wasn't able to
                     * allocate some memory it needed. Since the library doesn't
                     * normally use any large data structures, this probably means
                     * the system is critically low on memory and recovery will
                     * require very careful handling. The library itself will
                     * always try to handle any allocation failure safely.
                     */
                    throw new OutOfMemoryException(
                           "libserialport wasn't able to allocate some memory");

                case sp_return.SP_OK:
                default:
                    return;
            }
        }

        #endregion methods

        #region Equals(), GetHashCode(), toString()

        public override bool Equals(object obj)
        {
            SerialPortObj other = obj as SerialPortObj;
            if (other==null) return false;
            return this.Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override string ToString()
        {
            return "SerialPortObj(\"" + this.Name + "\")";
        }

        #endregion

    }
}
