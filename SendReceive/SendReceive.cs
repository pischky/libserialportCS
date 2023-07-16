using System;
using System.Text;

using Lsp = libserialport.Libserialport;

namespace libserialport.example
{
    /// <summary>
    /// Example of how to send and receive data.
    /// </summary>
    /// <para>
    /// This example file is released to the public domain.
    /// </para>
    /// <see>examples/send_receive.c</see>
    /// </summary>
    unsafe public class SendReceive
    {
        public static int Main(string[] args)
        {
            try
            {
                int rc = SendReceiveMain(args);
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

        public static int SendReceiveMain(string[] args)
        {
            /* This example can be used with one or two ports. With one port, it
             * will send data and try to receive it on the same port. This can be
             * done by connecting a single wire between the TX and RX pins of the
             * port.
             *
             * Alternatively it can be used with two serial ports connected to each
             * other, so that data can be sent on one and received on the other.
             * This can be done with two ports with TX/RX cross-connected, e.g. by
             * a "null modem" cable, or with a pair of interconnected virtual ports,
             * such as those created by com0com on Windows or tty0tty on Linux. */

            /* Get the port names from the command line. */
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Usage: {0} <port 1> [<port 2>]",
                                  AppDomain.CurrentDomain.FriendlyName);
                return -1;
            }
            int num_ports = args.Length;
            string[] port_names = new string[num_ports];
            Array.Copy(args, port_names, num_ports);
            Lsp.sp_port*[] ports = new Lsp.sp_port*[2];

            /* Open and configure each port. */
            for (int i = 0; i < num_ports; i++)
            {
                Console.WriteLine("Looking for port {0}.", port_names[i]);
                fixed (Lsp.sp_port** p = &ports[i])
                {
                    check(Lsp.sp_get_port_by_name(port_names[i], p));
                }
                // check(Lsp.sp_get_port_by_name(port_names[i], &ports[i]));
                // ==> error CS0212: You can only take the address of an unfixed
                //     expression inside of a fixed statement initializer
                // I really do not known whats the different here. See PortConfig.

                Console.WriteLine("Opening port.");
                check(Lsp.sp_open(ports[i], Lsp.sp_mode.SP_MODE_READ_WRITE));

                Console.WriteLine("Setting port to 9600 8N1, no flow control.");
                check(Lsp.sp_set_baudrate(ports[i], 9600));
                check(Lsp.sp_set_bits(ports[i], 8));
                check(Lsp.sp_set_parity(ports[i],
                                        Lsp.sp_parity.SP_PARITY_NONE));
                check(Lsp.sp_set_stopbits(ports[i], 1));
                check(Lsp.sp_set_flowcontrol(ports[i],
                                       Lsp.sp_flowcontrol.SP_FLOWCONTROL_NONE));
            }

            /* Now send some data on each port and receive it back. */
            for (int tx = 0; tx < num_ports; tx++)
            {
                /* Get the ports to send and receive on. */
                int rx = num_ports == 1 ? 0 : ((tx == 0) ? 1 : 0);
                Lsp.sp_port* tx_port = ports[tx];
                Lsp.sp_port* rx_port = ports[rx];

                /* The data we will send. */
                string data = "Hello!";
                int size = data.Length;

                /* We'll allow a 1 second timeout for send and receive. */
                uint timeout = 1000;

                /* On success, sp_blocking_write() and sp_blocking_read()
                 * return the number of bytes sent/received before the
                 * timeout expired. We'll store that result here. */
                int result;

                /* Send data. */
                Console.WriteLine("Sending '{0}' ({1} bytes) on port {2}.",
                                  data, size, Lsp.sp_get_port_name(tx_port));
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                result = Lsp.sp_blocking_write(tx_port, buffer, size, timeout);
                if (result <= 0) check((Libserialport.sp_return)result);

                /* Check whether we sent all of the data. */
                if (result == size)
                {
                    Console.WriteLine("Sent {0} bytes successfully.", size);
                }
                else
                {
                    Console.WriteLine("Timed out, {0}/{1} bytes sent.", result, size);
                }

                /* Allocate a buffer to receive data. */
                byte[] buf = new byte[size + 1];
                for (int j = 0; j < buf.Length; ++j) buf[j] = (byte)'@';

                /* Try to receive the data on the other port. */
                Console.WriteLine("Receiving {0} bytes on port {1}.",
                        size, Lsp.sp_get_port_name(rx_port));
                result = Lsp.sp_blocking_read(rx_port, buf, size, timeout);
                if (result <= 0) check((Libserialport.sp_return)result);

                /* Check whether we received the number of bytes we wanted. */
                if (result == size)
                {
                    Console.WriteLine("Received {0} bytes successfully.", size);
                }
                else
                {
                    Console.WriteLine("Timed out, {0}/{1} bytes received.", result, size);
                }

                /* Check if we received the same data we sent. */
                string str = Encoding.ASCII.GetString(buf, 0, result);
                Console.WriteLine("Received '{0}'.", str);

            }

            /* Close ports and free resources. */
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
