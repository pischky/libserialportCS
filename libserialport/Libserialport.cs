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
using Win32Exception        = System.ComponentModel.Win32Exception;
using Path                  = System.IO.Path;
using Assembly              = System.Reflection.Assembly;
using CallingConvention     = System.Runtime.InteropServices.CallingConvention;
using CharSet               = System.Runtime.InteropServices.CharSet;
using DllImportAttribute    = System.Runtime.InteropServices.DllImportAttribute;
using LayoutKind            = System.Runtime.InteropServices.LayoutKind;
using Marshal               = System.Runtime.InteropServices.Marshal;
using StructLayoutAttribute = System.Runtime.InteropServices.StructLayoutAttribute;

namespace libserialport
{
    /// <summary>
    /// C# binding of libserialport.
    /// </summary>
    /// <para>
    /// To generate "libserialport.dll" see REDAME.md.
    /// </para>
    /// <see>libserialport.h</see>
    public static class Libserialport
    {

        #region enums sp_return, sp_mode, sp_event, sp_buffer, ..., sp_transport

        /** Return values. */
        public enum sp_return : int // = Int32
        {
            /** Operation completed successfully. */
            SP_OK = 0,
            /** Invalid arguments were passed to the function. */
            SP_ERR_ARG = -1,
            /** A system error occurred while executing the operation. */
            SP_ERR_FAIL = -2,
            /** A memory allocation failed while executing the operation. */
            SP_ERR_MEM = -3,
            /** The requested operation is not supported by this system or device. */
            SP_ERR_SUPP = -4
        }

        /** Port access modes. */
        [Flags]
        public enum sp_mode : int // = Int32 (but results in error CS1008)
        {
            /** Open port for read access. */
            SP_MODE_READ = 1,
            /** Open port for write access. */
            SP_MODE_WRITE = 2,
            /** Open port for read and write access. @since 0.1.1 */
            SP_MODE_READ_WRITE = 3
        }

        /** Port events. */
        [Flags]
        public enum sp_event : int // = Int32
        {
            /** Data received and ready to read. */
            SP_EVENT_RX_READY = 1,
            /** Ready to transmit new data. */
            SP_EVENT_TX_READY = 2,
            /** Error occurred. */
            SP_EVENT_ERROR = 4
        }

        /** Buffer selection. */
        [Flags]
        public enum sp_buffer : int // = Int32
        {
            /** Input buffer. */
            SP_BUF_INPUT = 1,
            /** Output buffer. */
            SP_BUF_OUTPUT = 2,
            /** Both buffers. */
            SP_BUF_BOTH = 3
        }

        /** Parity settings. */
        public enum sp_parity : int // = Int32
        {
            /** Special value to indicate setting should be left alone. */
            SP_PARITY_INVALID = -1,
            /** No parity. */
            SP_PARITY_NONE = 0,
            /** Odd parity. */
            SP_PARITY_ODD = 1,
            /** Even parity. */
            SP_PARITY_EVEN = 2,
            /** Mark parity. */
            SP_PARITY_MARK = 3,
            /** Space parity. */
            SP_PARITY_SPACE = 4
        }

        /** RTS pin behaviour. */
        public enum sp_rts : int // = Int32
        {
            /** Special value to indicate setting should be left alone. */
            SP_RTS_INVALID = -1,
            /** RTS off. */
            SP_RTS_OFF = 0,
            /** RTS on. */
            SP_RTS_ON = 1,
            /** RTS used for flow control. */
            SP_RTS_FLOW_CONTROL = 2
        }

        /** CTS pin behaviour. */
        public enum sp_cts : int // = Int32
        {
            /** Special value to indicate setting should be left alone. */
            SP_CTS_INVALID = -1,
            /** CTS ignored. */
            SP_CTS_IGNORE = 0,
            /** CTS used for flow control. */
            SP_CTS_FLOW_CONTROL = 1
        }

        /** DTR pin behaviour. */
        public enum sp_dtr : int // = Int32
        {
            /** Special value to indicate setting should be left alone. */
            SP_DTR_INVALID = -1,
            /** DTR off. */
            SP_DTR_OFF = 0,
            /** DTR on. */
            SP_DTR_ON = 1,
            /** DTR used for flow control. */
            SP_DTR_FLOW_CONTROL = 2
        }

        /** DSR pin behaviour. */
        public enum sp_dsr : int // = Int32
        {
            /** Special value to indicate setting should be left alone. */
            SP_DSR_INVALID = -1,
            /** DSR ignored. */
            SP_DSR_IGNORE = 0,
            /** DSR used for flow control. */
            SP_DSR_FLOW_CONTROL = 1
        }

        /** XON/XOFF flow control behaviour. */
        public enum sp_xonxoff : int // = Int32
        {
            /** Special value to indicate setting should be left alone. */
            SP_XONXOFF_INVALID = -1,
            /** XON/XOFF disabled. */
            SP_XONXOFF_DISABLED = 0,
            /** XON/XOFF enabled for input only. */
            SP_XONXOFF_IN = 1,
            /** XON/XOFF enabled for output only. */
            SP_XONXOFF_OUT = 2,
            /** XON/XOFF enabled for input and output. */
            SP_XONXOFF_INOUT = 3
        }

        /** Standard flow control combinations. */
        public enum sp_flowcontrol : int // = Int32
        {
            /** No flow control. */
            SP_FLOWCONTROL_NONE = 0,
            /** Software flow control using XON/XOFF characters. */
            SP_FLOWCONTROL_XONXOFF = 1,
            /** Hardware flow control using RTS/CTS signals. */
            SP_FLOWCONTROL_RTSCTS = 2,
            /** Hardware flow control using DTR/DSR signals. */
            SP_FLOWCONTROL_DTRDSR = 3
        }

        /** Input signals. */
        [Flags]
        public enum sp_signal : int // = Int32
        {
            /** Clear to send. */
            SP_SIG_CTS = 1,
            /** Data set ready. */
            SP_SIG_DSR = 2,
            /** Data carrier detect. */
            SP_SIG_DCD = 4,
            /** Ring indicator. */
            SP_SIG_RI = 8
        }

        /**
         * Transport types.
         *
         * @since 0.1.1
         */
        public enum sp_transport : int // = Int32
        {
            /** Native platform serial port. @since 0.1.1 */
            SP_TRANSPORT_NATIVE,
            /** USB serial port adapter. @since 0.1.1 */
            SP_TRANSPORT_USB,
            /** Bluetooth serial port adapter. @since 0.1.1 */
            SP_TRANSPORT_BLUETOOTH
        }

        #endregion

        #region structs sp_port, sp_port_config, sp_event_set

        /// <summary>
        /// An opaque structure representing a serial port.
        /// <code>struct sp_port</code>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct sp_port
        {
        }

        /// <summary>
        /// An opaque structure representing the configuration for a serial port.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct sp_port_config
        {
        }

        /// <summary>
        /// A set of handles to wait on for events.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        unsafe public struct sp_event_set {
	        /** Array of OS-specific handles. */
            void* handles;
	        /** Array of bitmasks indicating which events apply for each handle. */
            sp_event* masks;
	        /** Number of handles. */
	        UInt32 count;
        }

        #endregion

        #region methods Port enumeration - Enumerating the serial ports of a system.

        // ------------------------------------------- sp_get_port_by_name -----

        /// <summary>
        /// Obtain a pointer to a new sp_port structure representing the named
        /// port.
        /// </summary>
        /// <para>
        /// The user should allocate a variable of type "struct sp_port *"
        /// and pass a pointer to this to receive the result.
        /// </para>
        /// <para>
        /// The result should be freed after use by calling sp_free_port().
        /// </para>
        /// <param name="portname">
        /// [in] The OS-specific name of a serial port. Must not be NULL.
        /// </param>
        /// <param name="port_ptr">
        /// If any error is returned, the variable pointed to by
        /// port_ptr will be set to NULL. Otherwise, it will be set
        /// to point to the newly allocated port. Must not be NULL.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_port_by_name(
            string portname,        // const char *
            sp_port** port_ptr      // struct sp_port **port_ptr
        );

        // -------------------------------------------------- sp_free_port -----

        /// <summary>
        /// Free a port structure obtained from sp_get_port_by_name() or
        /// sp_copy_port().
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        void sp_free_port(
            sp_port* port           // struct sp_port *port
        );

        // ------------------------------------------------- sp_list_ports -----

        /// <summary>
        /// List the serial ports available on the system.
        /// </summary>
        /// <para>
        /// The result obtained is an array of pointers to sp_port structures,
        /// terminated by a NULL. The user should allocate a variable of type
        /// "struct sp_port **" and pass a pointer to this to receive the result.
        /// </para>
        /// <para>
        /// The result should be freed after use by calling sp_free_port_list().
        /// If a port from the list is to be used after freeing the list, it must be
        /// copied first using sp_copy_port().
        /// </para>
        /// <param name="list_ptr">
        /// [out]  If any error is returned, the variable pointed to by
        /// list_ptr will be set to NULL. Otherwise, it will be set
        /// to point to the newly allocated array. Must not be NULL.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_list_ports(
            sp_port*** list_ptr   // struct sp_port ***list_ptr
        );

        // -------------------------------------------------- sp_copy_port -----

        /// <summary>
        /// Make a new copy of an sp_port structure.
        /// </summary>
        /// <para>
        /// The user should allocate a variable of type "struct sp_port *" and
        /// pass a pointer to this to receive the result.
        /// </para>
        /// <para>
        /// The copy should be freed after use by calling sp_free_port().
        /// </para>
        /// <param name="port">
        /// [in] port Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <param name="copy_ptr">
        /// [out] copy_ptr If any error is returned, the variable pointed to by
        /// copy_ptr will be set to NULL. Otherwise, it will be set
        /// to point to the newly allocated copy. Must not be NULL.
        /// </param>
        /// <returns>SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_copy_port(
            sp_port* port,
            sp_port** copy_ptr
        );

        // --------------------------------------------- sp_free_port_list -----

        /// <summary>
        /// Free a port list obtained from sp_list_ports().
        /// </summary>
        /// <para>
        /// This will also free all the sp_port structures referred to from the
        /// list; any that are to be retained must be copied first using
        /// sp_copy_port().
        /// </para>
        /// <param name="ports">
        /// [in] Pointer to a list of port structures. Must not be NULL.
        /// </param>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        void sp_free_port_list(
            sp_port** ports     // struct sp_port **ports
        );

        #endregion

        #region methods Ports handling - Opening, closing and querying ports.

        // ------------------------------------------------------- sp_open -----

        /// <summary>
        /// Open the specified serial port.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <param name="flags">
        /// [in] Flags to use when opening the serial port.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_open(
            sp_port* port,          // struct sp_port *port
            sp_mode flags           // enum sp_mode flags
        );

        // ------------------------------------------------------ sp_close -----

        /// <summary>
        /// Close the specified serial port.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_close(
            sp_port* port           // struct sp_port *port
        );

        // ---------------------------------------------- sp_get_port_name -----

        /// <summary>
        /// Get the name of a port.
        /// </summary>
        /// <para>
        /// The name returned is whatever is normally used to refer to a port
        /// on the current operating system; e.g. for Windows it will usually
        /// be a "COMn" device name, and for Unix it will be a device path
        /// beginning with "/dev/".
        /// </para>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <returns>
        /// The port name, or NULL if an invalid port is passed. The name
        /// string is part of the port structure and may not be used after
        /// the port structure has been freed.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_get_port_name")]
        unsafe private static extern
        IntPtr sp_get_port_name_intern(
            sp_port* port           // const struct sp_port *port
        );

        /// <summary>
        /// Get the name of a port.
        /// </summary>
        /// <para>
        /// The name returned is whatever is normally used to refer to a port
        /// on the current operating system; e.g. for Windows it will usually
        /// be a "COMn" device name, and for Unix it will be a device path
        /// beginning with "/dev/".
        /// </para>
        /// <para>
        /// This is a C# only function.
        /// </para>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <returns>
        /// Managed copy of the port string.
        /// </returns>
        unsafe public static
        string sp_get_port_name(sp_port* port)
        {
            IntPtr ptr = sp_get_port_name_intern(port);
            // would expect that Marshal.PtrToStringUni or PtrToStringAuto does the job
            string str = Marshal.PtrToStringAnsi(ptr);
            return str;
        }

        // --------------------------------------- sp_get_port_description -----

        /// <summary>
        /// Get a description for a port, to present to end user.
        /// </summary>
        /// <param name="port">
        /// [in] port Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <returns>
        /// The port description, or NULL if an invalid port is passed.
        /// The description string is part of the port structure and may not
        /// be used after the port structure has been freed.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_get_port_description")]
        unsafe private static extern
        IntPtr sp_get_port_description_intern(
            sp_port* port           // const struct sp_port *port
        );

        /// <summary>
        /// Get a description for a port, to present to end user.
        /// </summary>
        /// <param name="port">
        /// [in] port Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <para>
        /// This is a C# only function.
        /// </para>
        /// <returns>
        /// The port description, or NULL if an invalid port is passed.
        /// The description string is part of the port structure and may not
        /// be used after the port structure has been freed.
        /// </returns>
        unsafe public static
        string sp_get_port_description(sp_port* port)
        {
            IntPtr ptr = sp_get_port_description_intern(port);
            if (ptr == IntPtr.Zero) return null;
            // would expect that Marshal.PtrToStringUni does the job
            string str = Marshal.PtrToStringAnsi(ptr);
            return str;
        }

        // ----------------------------------------- sp_get_port_transport -----

        /// <summary>
        /// Get the transport type used by a port.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <returns>
        /// The port transport type.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_transport sp_get_port_transport(
            sp_port* port           // const struct sp_port *port
        );

        // ----------------------------------- sp_get_port_usb_bus_address -----

        /// <summary>
        /// Get the USB bus number and address on bus of a USB serial adapter
        /// port.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <param name="usb_bus">
        /// [out] usb_bus Pointer to a variable to store the USB bus.
        /// Can be NULL (in that case it will be ignored).
        /// </param>
        /// <param name="usb_address">
        /// [out] Pointer to a variable to store the USB address.
        /// Can be NULL (in that case it will be ignored).
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_port_usb_bus_address(
            sp_port* port,          // const struct sp_port *port
            int* usb_bus,           // int *usb_bus
            int* usb_address        // int *usb_address
        );

        // ----------------------------------- sp_get_port_usb_bus_address -----

        /// <summary>
        /// Get the USB Vendor ID and Product ID of a USB serial adapter port.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <param name="usb_vid">
        /// [out] Pointer to a variable to store the USB VID.
        /// Can be NULL (in that case it will be ignored).
        /// </param>
        /// <param name="usb_pid">
        /// [out] Pointer to a variable to store the USB PID.
        /// Can be NULL (in that case it will be ignored).
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_port_usb_vid_pid(
            sp_port* port,          // const struct sp_port *port
            int* usb_vid,           // int *usb_vid
            int* usb_pid            // int *usb_pid
        );

        // ---------------------------------- sp_get_port_usb_manufacturer -----

        /// <summary>
        /// Get the USB manufacturer string of a USB serial adapter port.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <returns>
        /// The port manufacturer string, or NULL if an invalid port is passed.
        /// The manufacturer string is part of the port structure and may not
        /// be used after the port structure has been freed.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_get_port_usb_manufacturer")]
        unsafe private static extern
        IntPtr sp_get_port_usb_manufacturer_intern(
            sp_port* port           // const struct sp_port *port
        );

        /// <summary>
        /// Get the USB manufacturer string of a USB serial adapter port.
        /// </summary>
        /// <para>
        /// This is a C# only function.
        /// </para>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <returns>
        /// The port manufacturer string, or NULL if an invalid port is passed.
        /// The manufacturer string is part of the port structure and may not
        /// be used after the port structure has been freed.
        /// </returns>
        unsafe public static
        string sp_get_port_usb_manufacturer(sp_port* port)
        {
            IntPtr ptr = sp_get_port_usb_manufacturer_intern(port);
            // would expect that Marshal.PtrToStringUni does the job
            string str = Marshal.PtrToStringAnsi(ptr);
            return str;
        }

        // --------------------------------------- sp_get_port_usb_product -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_get_port_usb_product")]
        unsafe private static extern
        IntPtr sp_get_port_usb_product_intern(
            sp_port* port           // const struct sp_port *port
        );

        /// <summary>TODO</summary>
        unsafe public static
        string sp_get_port_usb_product(sp_port* port)
        {
            IntPtr ptr = sp_get_port_usb_product_intern(port);
            // would expect that Marshal.PtrToStringUni does the job
            string str = Marshal.PtrToStringAnsi(ptr);
            return str;
        }

        // --------------------------------------- sp_get_port_usb_product -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_get_port_usb_serial")]
        unsafe private static extern
        IntPtr sp_get_port_usb_serial_intern(
            sp_port* port           // const struct sp_port *port
        );

        /// <summary>TODO</summary>
        unsafe public static
        string sp_get_port_usb_serial(sp_port* port)
        {
            IntPtr ptr = sp_get_port_usb_serial_intern(port);
            // would expect that Marshal.PtrToStringUni does the job
            string str = Marshal.PtrToStringAnsi(ptr);
            return str;
        }

        // --------------------------------- sp_get_port_bluetooth_address -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_get_port_bluetooth_address")]
        unsafe private static extern
        IntPtr sp_get_port_bluetooth_address_intern(
            sp_port* port           // const struct sp_port *port
        );

        /// <summary>TODO</summary>
        unsafe public static
        string sp_get_port_bluetooth_address(sp_port* port)
        {
            IntPtr ptr = sp_get_port_bluetooth_address_intern(port);
            // would expect that Marshal.PtrToStringUni does the job
            string str = Marshal.PtrToStringAnsi(ptr);
            return str;
        }

        // -------------------------------------------- sp_get_port_handle -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_get_port_handle")]
        unsafe private static extern
        sp_return sp_get_port_handle_intern(
            sp_port* port,          // const struct sp_port *port
            void* result_ptr        // void *result_ptr
        );

        /// <summary>TODO</summary>
        unsafe public static
        sp_return sp_get_port_handle(sp_port* port, out IntPtr handle)
        {
            if (IntPtr.Size == 4)
            {
                // 32-bit platform
                Int32 h = 0;
                sp_return rc = sp_get_port_handle_intern(port, &h);
                handle = new IntPtr(h);
                return rc;
            }
            else if (IntPtr.Size == 8)
            {
                // 64-bit platform
                Int64 h = 0;
                sp_return rc = sp_get_port_handle_intern(port, &h);
                handle = new IntPtr(h);
                return rc;
            }
            else
            {
                throw new InvalidOperationException("sp_get_port_handle: "
                     + "unexpected platform (IntPtr.Size=" + IntPtr.Size + ")");
            }
        }

        #endregion

        #region methods Configuration - Setting and querying serial port parameters.

        // ------------------------------------------------- sp_new_config -----

        /// <summary>
        /// Allocate a port configuration structure.
        /// </summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_new_config(
            sp_port_config** config_ptr //struct sp_port_config **config_ptr
        );

        // ------------------------------------------------ sp_free_config -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        void sp_free_config(
            sp_port_config* port           // struct sp_port_config *config
        );

        // ------------------------------------------------- sp_get_config -----

        /// <summary>
        /// Get the current configuration of the specified serial port.
        /// </summary>
        /// <para>
        /// The user should allocate a configuration structure using
        /// sp_new_config() and pass this as the config parameter. The
        /// configuration structure will be updated with the port
        /// configuration.
        /// </para>
        /// <para>
        /// Any parameters that are configured with settings not recognised or
        /// supported by libserialport, will be set to special values that are
        /// ignored by sp_set_config().
        /// </para>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be <c>null</c>.
        /// </param>
        /// <param name="config">
        /// [out] config Pointer to a configuration structure that will hold
        /// the result. Upon errors the contents of the config struct will not
        /// be changed. Must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config(
            sp_port* port,                 // struct sp_port *port
            sp_port_config* config         // struct sp_port_config *config
        );

        // ------------------------------------------------- sp_set_config -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config(
            sp_port* port,                 // struct sp_port *port
            sp_port_config* config        // const struct sp_port_config *config
        );

        // ----------------------------------------------- sp_set_baudrate -----

        /// <summary>
        /// Set the baud rate for the specified serial port.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be <c>null</c>.
        /// </param>
        /// <param name="baudrate">
        /// [in] Baud rate in bits per second.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_baudrate(
            sp_port* port,                 // struct sp_port *port
            Int32 baudrate                 // int baudrate
        );

        // ---------------------------------------- sp_get_config_baudrate -----

        /// <summary>
        /// Get the baud rate from a port configuration.
        /// </summary>
        /// <para>
        /// The user should allocate a variable of type int and
        /// pass a pointer to this to receive the result.
        /// </para>
        /// <param name="config">
        /// [in] Pointer to a configuration structure. Must not be <c>null</c>.
        /// </param>
        /// <param name="baudrate_ptr">
        /// [out] baudrate_ptr Pointer to a variable to store the result.
        /// Must not be <c>null</c>.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config_baudrate(
            sp_port_config* config,       // const struct sp_port_config *config
            Int32* baudrate_ptr            // int *baudrate_ptr
        );

        // ---------------------------------------- sp_set_config_baudrate -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_baudrate(
            sp_port_config* config,        // struct sp_port_config *config
            Int32 baudrate                 // int baudrate
        );

        // --------------------------------------------------- sp_set_bits -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_bits(
            sp_port* port,                 // struct sp_port *port
            Int32 bits
        );

        // -------------------------------------------- sp_get_config_bits -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config_bits(
            sp_port_config *config,        // const struct sp_port_config *config
            Int32* bits_ptr                // int *bits_ptr
        );

        // -------------------------------------------- sp_set_config_bits -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_bits(
            sp_port_config* config,        // struct sp_port_config *config
            Int32 bits                     // int bits
        );

        // ------------------------------------------------- sp_set_parity -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_parity(
            sp_port* port,                 // struct sp_port *port
            sp_parity parity               // enum sp_parity parity
        );

        // ------------------------------------------ sp_get_config_parity -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config_parity(
            sp_port_config* config,        // const struct sp_port_config *config
            sp_parity* parity_ptr          // enum sp_parity *parity_ptr
        );

        // ------------------------------------------ sp_set_config_parity -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_parity(
            sp_port_config* config,        // struct sp_port_config *config
            sp_parity parity               // enum sp_parity parity
        );

        // ----------------------------------------------- sp_set_stopbits -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_stopbits(
            sp_port *port,                 // struct sp_port *port
            Int32 stopbits                 // int stopbits
        );

        // ---------------------------------------- sp_get_config_stopbits -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config_stopbits(
            sp_port_config* config,        // const struct sp_port_config *config
            Int32* stopbits_ptr            // int *stopbits_ptr
        );

        // ---------------------------------------- sp_set_config_stopbits -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_stopbits(
            sp_port_config* config,        // struct sp_port_config *config
            Int32 stopbits                 // int stopbits
        );

        // ---------------------------------------------------- sp_set_rts -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_rts(
            sp_port* port,                 // struct sp_port *port
            sp_rts rts                     // enum sp_rts rts
        );

        // --------------------------------------------- sp_get_config_rts -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config_rts(
            sp_port_config* config,       // const struct sp_port_config *config
            sp_rts* rts_ptr                // enum sp_rts *rts_ptr
        );

        //---------------------------------------------- sp_set_config_rts -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_rts(
            sp_port_config *config,        // struct sp_port_config *config
            sp_rts rts                     // enum sp_rts rts
        );

        //----------------------------------------------------- sp_set_cts -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_cts(
            sp_port *port,                 // struct sp_port *port
            sp_cts cts                     // enum sp_cts cts
        );

        //---------------------------------------------- sp_get_config_cts -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config_cts(
            sp_port_config *config,       // const struct sp_port_config *config
            sp_cts *cts_ptr                // enum sp_cts *cts_ptr
        );

        //---------------------------------------------- sp_set_config_cts -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_cts(
            sp_port_config *config,        //struct sp_port_config *config
            sp_cts cts                     // enum sp_cts cts
        );

        //----------------------------------------------------- sp_set_dtr -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_dtr(
            sp_port *port,                 // struct sp_port *port
            sp_dtr dtr                     // enum sp_dtr dtr
        );

        //---------------------------------------------- sp_get_config_dtr -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config_dtr(
            sp_port_config *config,        // const struct sp_port_config *config
            sp_dtr *dtr_ptr                // enum sp_dtr *dtr_ptr
        );

        //---------------------------------------------- sp_set_config_dtr -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_dtr(
            sp_port_config *config,        // struct sp_port_config *config
            sp_dtr dtr                     // enum sp_dtr dtr
        );

        //----------------------------------------------------- sp_set_dsr -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_dsr(
            sp_port *port,                 // struct sp_port *port
            sp_dsr dsr                     // enum sp_dsr dsr
        );

        //---------------------------------------------- sp_get_config_dsr -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config_dsr(
            sp_port_config *config,        // const struct sp_port_config *config
            sp_dsr *dsr_ptr                // enum sp_dsr *dsr_ptr
        );

        //---------------------------------------------- sp_set_config_dsr -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_dsr(
            sp_port_config *config,        // struct sp_port_config *config
            sp_dsr dsr                     // enum sp_dsr dsr
        );

        //------------------------------------------------ sp_set_xon_xoff -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_xon_xoff(
            sp_port *port,                 // struct sp_port *port
            sp_xonxoff xon_xoff            // enum sp_xonxoff xon_xoff
        );

        //----------------------------------------- sp_get_config_xon_xoff -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_config_xon_xoff(
            sp_port_config *config,        // const struct sp_port_config *config
            sp_xonxoff *xon_xoff_ptr       // enum sp_xonxoff *xon_xoff_ptr
        );

        //----------------------------------------- sp_set_config_xon_xoff -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_xon_xoff(
            sp_port_config* config,        // struct sp_port_config *config
            sp_xonxoff xon_xoff            // enum sp_xonxoff xon_xoff
        );

        // ------------------------------------- sp_set_config_flowcontrol -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_config_flowcontrol(
            sp_port_config* config,        //struct sp_port_config *config
            sp_flowcontrol flowcontrol     // enum sp_flowcontrol flowcontrol
        );

        // -------------------------------------------- sp_set_flowcontrol -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_set_flowcontrol(
            sp_port* port,                 // struct sp_port *port
            sp_flowcontrol flowcontrol     // enum sp_flowcontrol flowcontrol
        );
        #endregion

        #region methods Data handling - Reading, writing, and flushing data.

        // ---------------------- sp_blocking_read -----

        /// <summary>
        /// Read bytes from the specified serial port, blocking until complete.
        /// </summary>
        /// <para>
        /// <strong>Warning:</strong>
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
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <param name="buf">
        /// [out] Buffer in which to store the bytes read. Must not be NULL.
        /// </param>
        /// <param name="count">
        /// [in] Requested number of bytes to read.
        /// </param>
        /// <param name="timeout_ms">
        /// [in] timeout_ms Timeout in milliseconds, or zero to wait indefinitely.
        /// </param>
        /// <returns>
        /// The number of bytes read on success, or a negative error code. If
        /// the number of bytes returned is less than that requested, the
        /// timeout was reached before the requested number of bytes was
        /// available. If timeout is zero, the function will always return
        /// either the requested number of bytes or a negative error code.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        int /*sp_return*/ sp_blocking_read(
            sp_port* port,                 // struct sp_port *port
            void* buf,                     // void *buf
            Int64 count,                   // size_t count
            UInt32 timeout_ms              // unsigned int timeout_ms
        );

        /// <summary>TODO</summary>
        unsafe public static
        int /*sp_return*/ sp_blocking_read( sp_port* port, byte[] buffer,
                                            int count, UInt32 timeout_ms )
        {
            if (count > buffer.Length)
            {
                throw new ArgumentException("count should not exeed buffer length");
            }
            int bReadCnt = (int)sp_return.SP_OK;
            byte[] temp = new byte[buffer.Length];
            fixed (byte* ptrTemp = temp)
            {
                bReadCnt = sp_blocking_read(port, ptrTemp,
                                            (Int64) count, timeout_ms);
                if (bReadCnt > 0) Array.Copy(temp, buffer, bReadCnt);
            }
            return bReadCnt;
        }

        // ----------------------------------------- sp_blocking_read_next -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        int /*sp_return*/ sp_blocking_read_next(
            sp_port* port,                 // struct sp_port *port
            void* buf,                     // void *buf
            Int64 count,                   // size_t count
            UInt32 timeout_ms              // unsigned int timeout_ms
        );

        /// <summary>TODO</summary>
        unsafe public static
        int /*sp_return*/ sp_blocking_read_next(sp_port* port, byte[] buffer,
                                                int count, UInt32 timeout_ms)
        {
            if (count > buffer.Length)
            {
                throw new ArgumentException("count should not exeed buffer length");
            }
            int bReadCnt = (int)sp_return.SP_OK;
            byte[] temp = new byte[buffer.Length];
            fixed (byte* ptrTemp = temp)
            {
                bReadCnt = sp_blocking_read(port, ptrTemp,
                                            (Int64)count, timeout_ms);
                if (bReadCnt > 0) Array.Copy(temp, buffer, bReadCnt);
            }
            return bReadCnt;
        }

        // ------------------------------------------- sp_nonblocking_read -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        int /*sp_return*/ sp_nonblocking_read(
            sp_port* port,                 // struct sp_port *port
            void* buf,                     // void *buf
            Int64 count                    // size_t count
        );

        /// <summary>TODO</summary>
        unsafe public static
        int /*sp_return*/ sp_nonblocking_read(sp_port* port, byte[] buffer,
                                              int count)
        {
            if (count > buffer.Length)
            {
                throw new ArgumentException("count should not exeed buffer length");
            }
            int bReadCnt = (int)sp_return.SP_OK;
            byte[] temp = new byte[buffer.Length];
            fixed (byte* ptrTemp = temp)
            {
                bReadCnt = sp_nonblocking_read(port, ptrTemp, (Int64)count);
                if (bReadCnt > 0) Array.Copy(temp, buffer, bReadCnt);
            }
            return bReadCnt;
        }

        // --------------------------------------------- sp_blocking_write -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        int /*sp_return*/ sp_blocking_write(
            sp_port* port,                 // struct sp_port *port
            void* buf,                     // const void *buf
            Int64 count,                   // size_t count,
            UInt32 timeout_ms              // unsigned int timeout_ms
        );

        /// <summary>TODO</summary>
        unsafe public static
        int /*sp_return*/ sp_blocking_write(sp_port* port, byte[] buffer,
                                            int count, UInt32 timeout_ms )
        {
            if (count > buffer.Length)
            {
                throw new ArgumentException("count should not exeed buffer length");
            }
            int bWriteCnt = (int)sp_return.SP_OK;
            fixed (byte* ptrTemp = buffer)
            {
                bWriteCnt = sp_blocking_write(port, ptrTemp,
                                              (Int64)count, timeout_ms);
            }
            return bWriteCnt;
        }

        // ------------------------------------------ sp_nonblocking_write -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        int /*sp_return*/ sp_nonblocking_write(
            sp_port* port,                 // struct sp_port *port
            void* buf,                     // const void *buf
            Int64 count                    // size_t count
        );

        /// <summary>TODO</summary>
        unsafe public static
        int /*sp_return*/ sp_nonblocking_write(sp_port* port, byte[] buffer,
                                               int count)
        {
            if (count > buffer.Length)
            {
                throw new ArgumentException("count should not exeed buffer length");
            }
            int bWriteCnt = (int)sp_return.SP_OK;
            fixed (byte* ptrTemp = buffer)
            {
                bWriteCnt = sp_nonblocking_write(port, ptrTemp, (Int64)count);
            }
            return bWriteCnt;
        }

        // ---------------------------------------------- sp_input_waiting -----

        /// <summary>
        /// Gets the number of bytes waiting in the input buffer.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <returns>
        /// Number of bytes waiting on success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        int /*sp_return*/ sp_input_waiting(
            sp_port* port                  // struct sp_port *port
        );

        // --------------------------------------------- sp_output_waiting -----

        /// <summary>
        /// Gets the number of bytes waiting in the output buffer.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <returns>
        /// Number of bytes waiting on success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        int /*sp_return*/ sp_output_waiting(
            sp_port* port                  // struct sp_port *port
        );

        // ------------------------------------------------------ sp_flush -----

        /// <summary>
        /// Flush serial port buffers. Data in the selected buffer(s) is
        /// discarded.
        /// </summary>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <param name="buffers">
        /// [in] Which buffer(s) to flush.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_flush(
            sp_port* port,                 // struct sp_port *port
            sp_buffer buffers              // enum sp_buffer buffers
        );

        // ------------------------------------------------------ sp_drain -----

        /// <summary>
        /// Wait for buffered data to be transmitted.
        /// </summary>
        /// <para>
        /// <strong>Warning:</strong>
        /// If your program runs on Unix, defines its
        /// own signal handlers, and needs to abort draining the output
        /// buffer when when these are called, then you should not use this
        /// function. It repeats system calls that return with EINTR. To be
        /// able to abort a drain from a signal handler, you would need to
        /// implement your own blocking drain by polling the result of
        /// sp_output_waiting().
        /// </para>
        /// <param name="port">
        /// </param>
        /// <returns></returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_drain(
            sp_port* port                  // struct sp_port *port
        );

        // ---------------------------------------------- sp_new_event_set -----

        /// <summary>
        /// Allocate storage for a set of events.
        /// </summary>
        /// <para>
        /// The user should allocate a variable of type struct sp_event_set *,
        /// then pass a pointer to this variable to receive the result.
        /// </para>
        /// <para>
        /// The result should be freed after use by calling sp_free_event_set().
        /// </para>
        /// <param name="result_ptr">
        /// [out] If any error is returned, the variable pointed to by
        /// result_ptr will be set to NULL. Otherwise, it will
        /// be set to point to the event set. Must not be NULL.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_new_event_set(
            sp_event_set** result_ptr      // struct sp_event_set **result_ptr
        );

        // -------------------------------------------- sp_add_port_events -----

        /// <summary>
        /// Add events to a struct sp_event_set for a given port.
        /// </summary>
        /// <para>
        /// The port must first be opened by calling sp_open() using the same
        /// port structure.
        /// </para>
        /// <para>
        /// After the port is closed or the port structure freed, the results
        /// may no longer be valid.
        /// </para>
        /// <param name="event_set">
        /// [in,out] Event set to update. Must not be NULL.
        /// </param>
        /// <param name="port">
        /// [in] Pointer to a port structure. Must not be NULL.
        /// </param>
        /// <param name="mask">
        /// [in] mask Bitmask of events to be waited for.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_add_port_events(
            sp_event_set* event_set,       // struct sp_event_set *event_set
            sp_port *port,                 // const struct sp_port *port
            sp_event mask                  // enum sp_event mask
        );

        // ------------------------------------------------------- sp_wait -----

        /// <summary>
        /// Wait for any of a set of events to occur.
        /// </summary>
        /// <param name="event_set">
        /// [in] Event set to wait on. Must not be NULL.
        /// </param>
        /// <param name="timeout_ms">
        /// [in] Timeout in milliseconds, or zero to wait indefinitely.
        /// </param>
        /// <returns>
        /// SP_OK upon success, a negative error code otherwise.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_wait(
            sp_event_set* event_set,       // struct sp_event_set *event_set
            UInt32 timeout_ms              // unsigned int timeout_ms
        );

        // --------------------------------------------- sp_free_event_set -----

        /// <summary>
        /// Free a structure allocated by sp_new_event_set().
        /// </summary>
        /// <param name="event_set">
        /// [in] Event set to free. Must not be NULL.
        /// </param>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        void sp_free_event_set(
            sp_event_set* event_set        // struct sp_event_set *event_set
        );

        #endregion

        #region methods Signals - Port signalling operations.

        // ------------------------------------------------ sp_get_signals -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_get_signals(
            sp_port* port,                // struct sp_port *port
            sp_signal* signal_mask        // enum sp_signal *signal_mask
        );

        // ------------------------------------------------ sp_start_break -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_start_break(
            sp_port* port                  // struct sp_port *port
        );

        // -------------------------------------------------- sp_end_break -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern
        sp_return sp_end_break(
            sp_port* port                  // struct sp_port *port
        );

        #endregion

        #region methods Errors - Obtaining error information.

        // -------------------------------------------- sp_last_error_code -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern
        int sp_last_error_code();

        // ----------------------------------------- sp_last_error_message -----

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_last_error_message")]
        unsafe private static extern
        IntPtr sp_last_error_message_intern(
        );

        /// <summary>TODO</summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_free_error_message")]
        unsafe private static extern
        void sp_free_error_message_intern(char* message);

        /// <summary>TODO</summary>
        unsafe public static
        string sp_last_error_message()
        {
            IntPtr ptr = sp_last_error_message_intern();
            // would expect that Marshal.PtrToStringUni does the job
            string str = Marshal.PtrToStringAnsi(ptr); // copies
            sp_free_error_message_intern((char*)ptr.ToPointer());
            return str;
        }

        /// <summary>TODO</summary>
        [Obsolete("sp_free_error_message is not required. "
                  + "sp_last_error_message() returns managed String.", false)]
        public static
        void sp_free_error_message(string s)
        {
        }

        // TODO: void sp_set_debug_handler(void (*handler)(const char *format, ...))
        // TODO: void sp_default_debug_handler(const char *format, ...

        #endregion

        #region methods Versions - Version number querying functions.

        // ---------------------------------- sp_get_major_package_version -----

        /// <summary>
        /// Get the major libserialport package version number.
        /// </summary>
        /// <returns>
        /// The major package version number.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern
        int sp_get_major_package_version();

        // ---------------------------------- sp_get_minor_package_version -----

        /// <summary>
        /// Get the minor libserialport package version number.
        /// </summary>
        /// <returns>
        /// The minor package version number.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern
        int sp_get_minor_package_version();

        // ---------------------------------- sp_get_micro_package_version -----

        /// <summary>
        /// Get the micro libserialport package version number.
        /// </summary>
        /// <returns>
        /// The micro package version number.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern
        int sp_get_micro_package_version();

        // --------------------------------- sp_get_package_version_string -----

        /// <summary> TODO </summary>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_get_package_version_string")]
        unsafe private static extern
        IntPtr sp_get_package_version_string_intern(); // const char *

        /// <summary>
        /// Get the libserialport package version number as a string.
        /// </summary>
        /// <returns>
        /// The package version number string. The returned string is
        /// static and thus should NOT be free'd by the caller.
        /// </returns>
        unsafe public static
        string sp_get_package_version_string()
        {
            IntPtr ptr = sp_get_package_version_string_intern();
            // would expect that Marshal.PtrToStringUni does the job
            string str = Marshal.PtrToStringAnsi(ptr); // copies
            return str;
        }

        // ------------------------------------ sp_get_current_lib_version -----

        /// <summary>
        /// Get the "current" part of the libserialport library version number.
        /// </summary>
        /// <returns>
        /// The "current" library version number.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern
        int sp_get_current_lib_version();

        // ----------------------------------- sp_get_revision_lib_version -----

        /// <summary>
        /// Get the "revision" part of the libserialport library version number.
        /// </summary>
        /// <returns>
        /// The "revision" library version number.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern
        int sp_get_revision_lib_version();

        // ---------------------------------------- sp_get_age_lib_version -----

        /// <summary>
        /// Get the "age" part of the libserialport library version number.
        /// </summary>
        /// <returns>
        /// The "age" library version number.
        /// </returns>
        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl)]
        public static extern
        int sp_get_age_lib_version();

        // ------------------------------------- sp_get_lib_version_string -----

        [DllImport(LIBSERIALPORT_DLL_NAME,
                   CallingConvention = CallingConvention.Cdecl,
                   EntryPoint = "sp_get_package_version_string")]
        unsafe private static extern
        IntPtr sp_get_lib_version_string_intern(); // const char *

        /// <summary>
        /// Get the libserialport library version number as a string.
        /// </summary>
        /// <returns>
        /// The library version number string.
        /// </returns>
        unsafe public static
        string sp_get_lib_version_string()
        {
            IntPtr ptr = sp_get_lib_version_string_intern();
            // would expect that Marshal.PtrToStringUni does the job
            string str = Marshal.PtrToStringAnsi(ptr); // copies
            return str;
        }

        #endregion

        #region loading of native library

        // TODO: on linux this is "libserialport.so". Renaming on win fails.
        private const string LIBSERIALPORT_DLL_NAME
                                        = "libserialport.dll"; //+ "-0"? ".dll" or ".so"

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        static Libserialport()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                string dir = "";
                switch (IntPtr.Size)
                {
                    case 4: dir = "x86";    break;  // running as 32 bit app
                    case 8: dir = "x86_64"; break;  // running as 64 bit app
                    default: break;
                }
                String dllPath = Path.Combine(Path.GetDirectoryName(
                                  Assembly.GetCallingAssembly().Location), dir);
                // AddDllDirectory does not do the trick. Don't know why.
                // Add to search path.
                if (!SetDllDirectory(dllPath))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                // Call any function to load library
                int v = sp_get_current_lib_version();
                // restore Standardsuchereihenfolge 
                if (!SetDllDirectory(null))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // running Linux with mono ?
                // Call any function to load library
                // we assume that "sodo make install" has been excuted
                int v = sp_get_current_lib_version(); // fails because LIBSERIALPORT_DLL_NAME contains "-0"
            }
            else
            {
                throw new Exception("unsupported platform: "
                                    + Environment.OSVersion.Platform);
            }
        }

        #endregion

    }
}
