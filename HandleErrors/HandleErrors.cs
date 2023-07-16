using System;

using Lsp = libserialport.Libserialport;

namespace libserialport.example
{

    /// <summary>
    /// Example of how to handle errors from libserialport.
    /// </summary>
    /// <para>
    /// This example file is released to the public domain.
    /// </para>
    /// <see>examples/handle_errors.c</see>
    unsafe public class HandleErrors
    {

        public static int Main(string[] args)
        {
            try
            {
                int rc = HandleErrorsMain(args);
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

        private static int check(Lsp.sp_return result)
        {
            switch (result)
            {

                /* Handle each of the four negative error codes that can be returned.
                 *
                 * In this example, we will end the program on any error, using
                 * a different return code for each possible class of error. */

                case Lsp.sp_return.SP_ERR_ARG:
                    /* When SP_ERR_ARG is returned, there was a problem with one
                     * or more of the arguments passed to the function, e.g. a null
                     * pointer or an invalid value. This generally implies a bug in
                     * the calling code. */
                    Console.WriteLine("Error: Invalid argument.");
                    return 1;

                case Lsp.sp_return.SP_ERR_FAIL:
                    /* When SP_ERR_FAIL is returned, there was an error from the OS,
                     * which we can obtain the error code and message for. These
                     * calls must be made in the same thread as the call that
                     * returned SP_ERR_FAIL, and before any other system functions
                     * are called in that thread, or they may not return the
                     * correct results. */
                    int error_code = Lsp.sp_last_error_code();
                    string error_message = Lsp.sp_last_error_message();
                    Console.WriteLine("Error: Failed: OS error code: {0}, message: '{1}'",
                                      error_code, error_message);
                    /* The error message should be freed after use. */
                    #pragma warning disable 0618 // free not needed
                    Lsp.sp_free_error_message(error_message);
                    #pragma warning restore 0618
                    return 2;

                case Lsp.sp_return.SP_ERR_SUPP:
                    /* When SP_ERR_SUPP is returned, the function was asked to do
                     * something that isn't supported by the current OS or device,
                     * or that libserialport doesn't know how to do in the current
                     * version. */
                    Console.WriteLine("Error: Not supported.");
                    return 3;

                case Lsp.sp_return.SP_ERR_MEM:
                    /* When SP_ERR_MEM is returned, libserialport wasn't able to
                     * allocate some memory it needed. Since the library doesn't
                     * normally use any large data structures, this probably means
                     * the system is critically low on memory and recovery will
                     * require very careful handling. The library itself will
                     * always try to handle any allocation failure safely.
                     *
                     * In this example, we'll just try to exit gracefully without
                     * calling printf, which might need to allocate further memory. */
                    return 4;

                case Lsp.sp_return.SP_OK:
                default:
                    /* A return value of SP_OK, defined as zero, means that the
                     * operation succeeded. */
                    Console.WriteLine("Operation succeeded.");

                    /* Some fuctions can also return a value greater than zero to
                     * indicate a numeric result, such as the number of bytes read by
                     * sp_blocking_read(). So when writing an error handling wrapper
                     * function like this one, it's helpful to return the result so
                     * that it can be used. */
                    return (int) result;

            }
        }

        public static int HandleErrorsMain(string[] args)
        {
            Lsp.sp_port** port_list = null;
            Lsp.sp_port_config* config = null;
            Lsp.sp_port* port = null;
            try
            {
                Lsp.sp_return rc = Lsp.sp_return.SP_OK;
                /* Call some functions that should not result in errors. */

                Console.WriteLine("Getting list of ports.");
                rc = Lsp.sp_list_ports(&port_list);
                check(rc);

                Console.WriteLine("Creating a new port configuration.");
                rc = Lsp.sp_new_config(&config);
                check(rc);

                /* Now make a function call that will result in an error. */

                Console.WriteLine("Trying to find a port that doesn't exist.");
                rc = Lsp.sp_get_port_by_name("NON-EXISTENT-PORT", &port);
                // rc still SP_OK (Win7). This is also the case in c excample
                check(rc);
                Lsp.sp_mode flags = Libserialport.sp_mode.SP_MODE_READ_WRITE;
                Console.WriteLine("Trying to open.");
                rc = Lsp.sp_open(port, flags);
                check(rc);
                Console.WriteLine("Trying to close.");
                rc = Lsp.sp_close(port);
                check(rc);
            }
            finally
            {
                if (port_list != null)
                {
                    Lsp.sp_free_port_list(port_list);
                    port_list = null;
                }
                if (config != null)
                {
                    Lsp.sp_free_config(config);
                    config = null;
                }
                if (port != null)
                {
                    Lsp.sp_free_port(port);
                    port = null;
                }
            }
            /* We could now clean up and exit normally if an error hadn't occured. */
            return 0;
        }
    }
}
