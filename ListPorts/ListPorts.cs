using System;

using Lsp = libserialport.Libserialport;

namespace libserialport.example
{
    /// <summary>
    /// Example of how to get a list of serial ports on the system.
    /// </summary>
    /// <para>
    /// This example file is released to the public domain.
    /// </para>
    /// <see>examples/list_ports.c</see>
    unsafe public class ListPorts
    {
        public static int Main(string[] args)
        {
            try
            {
                int rc = ListPortsMain(args);
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

        public static int ListPortsMain(string[] args)
        {
            /* A pointer to a null-terminated array of pointers to
             * struct sp_port, which will contain the ports found.*/
            Lsp.sp_port** port_list;

            Console.WriteLine("Getting port list.");

            /* Call sp_list_ports() to get the ports. The port_list
             * pointer will be updated to refer to the array created. */
            Lsp.sp_return result = Lsp.sp_list_ports(&port_list);

            if (result != Lsp.sp_return.SP_OK) {
                Console.WriteLine("sp_list_ports() failed!");
                return -1;
            }

            /* Iterate through the ports. When port_list[i] is NULL
             * this indicates the end of the list. */
            int i;
            Lsp.sp_port* first_port = null;
            for (i = 0; port_list[i] != null; i++)
            {
                Lsp.sp_port* port = port_list[i];
                if (i == 0)
                {
                    result = Lsp.sp_copy_port(port, &first_port);
                    if (result != Lsp.sp_return.SP_OK)
                    {
                        Console.WriteLine("sp_copy_port() failed!");
                        return -1;
                    }
                }

                /* Get the name of the port. */
                string port_name = Lsp.sp_get_port_name(port);

                Console.WriteLine("Found port: " + port_name);
            }

            Console.WriteLine("Found " + i + " ports.");

            Console.WriteLine("Freeing port list.");

            /* Free the array created by sp_list_ports(). */
            Lsp.sp_free_port_list(port_list);

            /* Note that this will also free all the sp_port structures
             * it points to. If you want to keep one of them (e.g. to
             * use that port in the rest of your program), take a copy
             * of it first using sp_copy_port(). */

            if (first_port != null)
            {
                Console.WriteLine("First Port name: "
                                  + Lsp.sp_get_port_name(first_port));
                Console.WriteLine("First Description: "
                                  + Lsp.sp_get_port_description(first_port));
                IntPtr handle;
                Lsp.sp_get_port_handle(first_port, out handle);
                Console.WriteLine("First Handle: "
                                  + handle.ToString("X08") );
            }

            Lsp.sp_free_port(first_port);

            return 0;
        }

    }
}
