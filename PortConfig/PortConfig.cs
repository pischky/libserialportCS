using System;

using Lsp = libserialport.Libserialport;

namespace libserialport.example
{
    /// <summary>
    /// Example of how to configure a serial port.
    /// </summary>
    /// <para>
    /// This example file is released to the public domain.
    /// </para>
    /// <see>examples/port_config.c</see>
    unsafe public class PortConfig
    {
        public static int Main(string[] args)
        {
            try
            {
                int rc = PortConfigMain(args);
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

        public static int PortConfigMain(string[] args)
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

            Console.WriteLine("Looking for port {0}.", port_name);

            /* Call sp_get_port_by_name() to find the port. The port
             * pointer will be updated to refer to the port found. */
            check(Lsp.sp_get_port_by_name(port_name, &port));
            // fixed (Lsp.sp_port** p = &port)
            //    check(Lsp.sp_get_port_by_name(port_name, p));
            // ==> error CS0213: You cannot use the fixed statement to take the
            //     address of an already fixed expression
            // I really do not known whats the different here. See SendReceive.

            /* Display some basic information about the port. */
            Console.WriteLine("Port name: {0}",
                              Lsp.sp_get_port_name(port));
            Console.WriteLine("Description: {0}",
                              Lsp.sp_get_port_description(port));

            /* The port must be open to access its configuration. */
            Console.WriteLine("Opening port.");
            check(Lsp.sp_open(port, Lsp.sp_mode.SP_MODE_READ_WRITE));

            /* There are two ways to access a port's configuration:
             *
             * 1. You can read and write a whole configuration (all settings at
             *    once) using sp_get_config() and sp_set_config(). This is handy
             *    if you want to change between some preset combinations, or save
             *    and restore an existing configuration. It also ensures the
             *    changes are made together, via an efficient set of calls into
             *    the OS - in some cases a single system call can be used.
             *
             *    Use accessor functions like sp_get_config_baudrate() and
             *    sp_set_config_baudrate() to get and set individual settings
             *    from a configuration.
             *
             *    Configurations are allocated using sp_new_config() and freed
             *    with sp_free_config(). You need to manage them yourself.
             *
             * 2. As a shortcut, you can set individual settings on a port
             *    directly by calling functions like sp_set_baudrate() and
             *    sp_set_parity(). This saves you the work of allocating
             *    a temporary config, setting it up, applying it to a port
             *    and then freeing it.
             *
             * In this example we'll do a bit of both: apply some initial settings
             * to the port, read out that config and display it, then switch to a
             * different configuration and back using sp_set_config(). */

            /* First let's set some initial settings directly on the port.
             *
             * You should always configure all settings before using a port.
             * There are no "default" settings applied by libserialport.
             * When you open a port it has the defaults from the OS or driver,
             * or the settings left over by the last program to use it. */
            Console.WriteLine("Setting port to 115200 8N1, no flow control.");
            check(Lsp.sp_set_baudrate(port, 115200));
            check(Lsp.sp_set_bits(port, 8));
            check(Lsp.sp_set_parity(port, Lsp.sp_parity.SP_PARITY_NONE));
            check(Lsp.sp_set_stopbits(port, 1));
            check(Lsp.sp_set_flowcontrol(port,
                                       Lsp.sp_flowcontrol.SP_FLOWCONTROL_NONE));

            /* A pointer to a struct sp_port_config, which we'll use for the
             * config read back from the port. The pointer will be set by
             * sp_new_config(). */
            Lsp.sp_port_config* initial_config;

            /* Allocate a configuration for us to read the port config into. */
            check(Lsp.sp_new_config(&initial_config));

            /* Read the current config from the port into that configuration. */
            check(Lsp.sp_get_config(port, initial_config));

            /* Display some of the settings read back from the port. */
            int baudrate = 0, bits = 0, stopbits = 0;
            Lsp.sp_parity parity = Lsp.sp_parity.SP_PARITY_NONE;
            check(Lsp.sp_get_config_baudrate(initial_config, &baudrate));
            check(Lsp.sp_get_config_bits(initial_config, &bits));
            check(Lsp.sp_get_config_stopbits(initial_config, &stopbits));
            check(Lsp.sp_get_config_parity(initial_config, &parity));
            Console.WriteLine("Baudrate: {0}, data bits: {1}, parity: {2}, "
                              + "stop bits: {3}", baudrate, bits,
                              parity_name(parity), stopbits);

            Lsp.sp_rts rts;
            check(Lsp.sp_get_config_rts(initial_config, &rts));
            Console.WriteLine("RTS: " + rts);
            Lsp.sp_cts cts;
            check(Lsp.sp_get_config_cts(initial_config, &cts));
            Console.WriteLine("CTS: " + cts);
            Lsp.sp_dtr dtr;
            check(Lsp.sp_get_config_dtr(initial_config, &dtr));
            Console.WriteLine("DTR: " + dtr);
            Lsp.sp_dsr dsr;
            check(Lsp.sp_get_config_dsr(initial_config, &dsr));
            Console.WriteLine("DSR: " + dsr);
            Lsp.sp_xonxoff xonxoff;
            check(Lsp.sp_get_config_xon_xoff(initial_config, &xonxoff));
            Console.WriteLine("xonxoff: " + xonxoff);

            /* Create a different configuration to have ready for use. */
            Console.WriteLine("Creating new config for 9600 7E2, XON/XOFF flow control.");
            Lsp.sp_port_config* other_config;
            check(Lsp.sp_new_config(&other_config));
            check(Lsp.sp_set_config_baudrate(other_config, 9600));
            check(Lsp.sp_set_config_bits(other_config, 7));
            check(Lsp.sp_set_config_parity(other_config,
                                           Lsp.sp_parity.SP_PARITY_EVEN));
            check(Lsp.sp_set_config_stopbits(other_config, 2));
            check(Lsp.sp_set_config_flowcontrol(other_config,
                                    Lsp.sp_flowcontrol.SP_FLOWCONTROL_XONXOFF));

            /* We can apply the new config to the port in one call. */
            Console.WriteLine("Applying new configuration.");
            check(Lsp.sp_set_config(port, other_config));

            /* And now switch back to our original config. */
            Console.WriteLine("Setting port back to previous config.");
            check(Lsp.sp_set_config(port, initial_config));

            /* Now clean up by closing the port and freeing structures. */
            check(Lsp.sp_close(port));
            Lsp.sp_free_port(port);
            Lsp.sp_free_config(initial_config);
            Lsp.sp_free_config(other_config);

            return 0;
        }

        /* Helper function for error handling. */
        private static int check(Lsp.sp_return result)
        {
            /* For this example we'll just exit on any error by calling abort(). */
            string error_message;

            switch (result)
            {
                case Lsp.sp_return.SP_ERR_ARG:
                    Console.WriteLine("Error: Invalid argument.");
                    Environment.Exit(-1);
                    break;
                case Lsp.sp_return.SP_ERR_FAIL:
                    error_message = Lsp.sp_last_error_message();
                    Console.WriteLine("Error: Failed: {0}", error_message);
                    Environment.Exit(-1);
                    break;
                case Lsp.sp_return.SP_ERR_SUPP:
                    Console.WriteLine("Error: Not supported.");
                    Environment.Exit(-1);
                    break;
                case Lsp.sp_return.SP_ERR_MEM:
                    Console.WriteLine("Error: Couldn't allocate memory.");
                    Environment.Exit(-1);
                    break;
                case Lsp.sp_return.SP_OK:
                default:
                    return (int)result;
            }
            return -1; // keep compiler happy when Exit(-1) is used
        }

        /* Helper function to give a name for each parity mode. */
        private static string parity_name(Lsp.sp_parity parity)
        {
            switch (parity)
            {
                case Lsp.sp_parity.SP_PARITY_INVALID:
                    return "(Invalid)";
                case Lsp.sp_parity.SP_PARITY_NONE:
                    return "None";
                case Lsp.sp_parity.SP_PARITY_ODD:
                    return "Odd";
                case Lsp.sp_parity.SP_PARITY_EVEN:
                    return "Even";
                case Lsp.sp_parity.SP_PARITY_MARK:
                    return "Mark";
                case Lsp.sp_parity.SP_PARITY_SPACE:
                    return "Space";
                default:
                    return "";
            }
        }

    }
}
