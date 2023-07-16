using System;

using Lsp = libserialport.Libserialport;

namespace libserialport.example
{
    /// <summary>
    /// Example of how to wait for events on multiple ports.
    /// </summary>
    /// <para>
    /// This example file is released to the public domain.
    /// </para>
    /// <see>examples/await_events.c</see>
    /// </summary>
    unsafe public class AwaitEvents
    {
        public static int Main(string[] args)
        {
            try
            {
                int rc = AwaitEventsMain(args);
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

        public static int AwaitEventsMain(string[] args)
        {
            /* Get the port names from the command line. */
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: {0} <port name>...\n",
                                  AppDomain.CurrentDomain.FriendlyName);
                return -1;
            }
            int num_ports = args.Length;
            string[] port_names = new string[num_ports];
            Array.Copy(args, port_names, num_ports);
            Lsp.sp_port*[] ports = new Lsp.sp_port*[num_ports];

            /* The set of events we will wait for. */
            Lsp.sp_event_set* event_set;

            /* Allocate the event set. */
            check(Lsp.sp_new_event_set(&event_set));

            /* Open and configure each port, and then add its RX event
             * to the event set. */
            for (int i = 0; i < num_ports; i++)
            {
                Console.WriteLine("Looking for port {0}.", port_names[i]);
                fixed (Lsp.sp_port** p = &ports[i])
                {
                    check(Lsp.sp_get_port_by_name(port_names[i], p));
                }

                Console.WriteLine("Opening port.");
                check(Lsp.sp_open(ports[i], Lsp.sp_mode.SP_MODE_READ));

                Console.WriteLine("Setting port to 9600 8N1, no flow control.");
                check(Lsp.sp_set_baudrate(ports[i], 9600));
                check(Lsp.sp_set_bits(ports[i], 8));
                check(Lsp.sp_set_parity(ports[i],
                                        Lsp.sp_parity.SP_PARITY_NONE));
                check(Lsp.sp_set_stopbits(ports[i], 1));
                check(Lsp.sp_set_flowcontrol(ports[i],
                                       Lsp.sp_flowcontrol.SP_FLOWCONTROL_NONE));

                Console.WriteLine("Adding port RX event to event set.");
                check(Lsp.sp_add_port_events(event_set, ports[i],
                                               Lsp.sp_event.SP_EVENT_RX_READY));
            }

            /* Now we can call sp_wait() to await any event in the set.
             * It will return when an event occurs, or the timeout elapses. */
            Console.WriteLine("Waiting up to 5 seconds for RX on any port...");
            check(Lsp.sp_wait(event_set, 5000));

            /* Iterate over ports to see which have data waiting. */
            for (int i = 0; i < num_ports; i++)
            {
                /* Get number of bytes waiting. */
                int bytes_waiting = Lsp.sp_input_waiting(ports[i]);
                check((Libserialport.sp_return)bytes_waiting);
                Console.WriteLine("Port {0}: {1} bytes received.",
                        Lsp.sp_get_port_name(ports[i]), bytes_waiting);
            }

            /* Close ports and free resources. */
            Lsp.sp_free_event_set(event_set);
            for (int i = 0; i < num_ports; i++)
            {
                check(Lsp.sp_close(ports[i]));
                Lsp.sp_free_port(ports[i]);
            }

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

    }
}
