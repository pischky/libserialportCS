using System;

using Lsp = libserialport.Libserialport;

namespace libserialport.example
{

    /// <summary>
    /// Example of how to get information about a serial port.
    /// </summary>
    /// <para>
    /// This example file is released to the public domain.
    /// </para>
    /// <see>examples/port_info.c</see>
    unsafe public class PortInfo
    {

        public static int Main(string[] args)
        {
            try
            {
                int rc = PortInfoMain(args);
                Console.WriteLine("press any key to continue (rc={0})", rc);
                Console.ReadKey();
                return rc;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("press any key to continue");
                Console.ReadKey();
                return -1;
            }
        }

        public static int PortInfoMain(string[] args)
        {
            /* Get the port name from the command line. */
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: "
                       + AppDomain.CurrentDomain.FriendlyName + " <port name>");
                return -1;
            }
            string port_name = args[0];

            /* A pointer to a struct sp_port, which will refer to
             * the port found. */
            Lsp.sp_port* port = null;

            Console.WriteLine("Looking for port " + port_name + ".");

            /* Call sp_get_port_by_name() to find the port. The port
             * pointer will be updated to refer to the port found. */
            Lsp.sp_return result = Lsp.sp_get_port_by_name(port_name, &port);
            if (result != Lsp.sp_return.SP_OK)
            {
                Console.WriteLine("sp_get_port_by_name() failed!");
                return -1;
            }

            /* Display some basic information about the port. */
            Console.WriteLine("Port name: "
                              + Lsp.sp_get_port_name(port));
            Console.WriteLine("Description: "
                              + Lsp.sp_get_port_description(port));

            /* Identify the transport which this port is connected through,
	         * e.g. native port, USB or Bluetooth. */
            Lsp.sp_transport transport = Lsp.sp_get_port_transport(port);

            if (transport == Lsp.sp_transport.SP_TRANSPORT_NATIVE)
            {
                /* This is a "native" port, usually directly connected
                 * to the system rather than some external interface. */
                Console.WriteLine("Type: Native");
            }
            else if (transport == Lsp.sp_transport.SP_TRANSPORT_USB)
            {
                /* This is a USB to serial converter of some kind. */
                Console.WriteLine("Type: USB");

                /* Display string information from the USB descriptors. */
                Console.WriteLine("Manufacturer: "
                                  + Lsp.sp_get_port_usb_manufacturer(port));
                Console.WriteLine("Product: "
                                  + Lsp.sp_get_port_usb_product(port));
                Console.WriteLine("Serial: "
                                  + Lsp.sp_get_port_usb_serial(port));

                /* Display USB vendor and product IDs. */
                int usb_vid = 0, usb_pid = 0;
                Lsp.sp_get_port_usb_vid_pid(port, &usb_vid, &usb_pid);
                Console.WriteLine("VID: " + usb_vid.ToString("X04")
                                  + " PID: " + usb_pid.ToString("X04"));

                /* Display bus and address. */
                int usb_bus = 0, usb_address = 0;
                Lsp.sp_get_port_usb_bus_address(port, &usb_bus, &usb_address);
                Console.WriteLine("Bus: " + usb_bus
                                  + " Address: " + usb_address);
            }
            else if (transport == Lsp.sp_transport.SP_TRANSPORT_BLUETOOTH)
            {
                /* This is a Bluetooth serial port. */
                Console.WriteLine("Type: Bluetooth");

                /* Display Bluetooth MAC address. */
                Console.WriteLine("MAC: "
                                  + Lsp.sp_get_port_bluetooth_address(port));
            }

            Console.WriteLine("Freeing port.");

            /* Free the port structure created by sp_get_port_by_name(). */
            Lsp.sp_free_port(port);

            /* Note that this will also free the port name and other
             * strings retrieved from the port structure. If you want
             * to keep these, copy them before freeing the port. */

            Console.WriteLine("Major package version="
                              + Lsp.sp_get_major_package_version());
            Console.WriteLine("Minor package version="
                              + Lsp.sp_get_minor_package_version());
            Console.WriteLine("Micro package version="
                              + Lsp.sp_get_micro_package_version());
            Console.WriteLine("Package version='{0}'",
                              Lsp.sp_get_package_version_string());

            Console.WriteLine("Current part library version="
                              + Lsp.sp_get_current_lib_version());
            Console.WriteLine("Revision part library version="
                              + Lsp.sp_get_revision_lib_version());
            Console.WriteLine("Age part library version="
                              + Lsp.sp_get_age_lib_version());
            Console.WriteLine("Library version='{0}'",
                              Lsp.sp_get_lib_version_string());
            return 0;
        }
    }
}
