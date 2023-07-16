using System;
using System.Text;
using System.Threading;

using SerialPortObj = libserialport.SerialPortObj;
using sp_mode = libserialport.Libserialport.sp_mode;
using sp_transport = libserialport.Libserialport.sp_transport;
using sp_signal = libserialport.Libserialport.sp_signal;
using sp_parity = libserialport.Libserialport.sp_parity;
using sp_flowcontrol = libserialport.Libserialport.sp_flowcontrol;
using sp_rts = libserialport.Libserialport.sp_rts;
using sp_cts = libserialport.Libserialport.sp_cts;
using sp_dtr = libserialport.Libserialport.sp_dtr;
using sp_dsr = libserialport.Libserialport.sp_dsr;
using sp_xonxoff = libserialport.Libserialport.sp_xonxoff;
using sp_event = libserialport.Libserialport.sp_event;

namespace libserialport.example
{
    public class CSharpTest
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("running on {0}", Environment.OSVersion.Platform);
                string COM1 = "", COM2 = "", COM6 = "";
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    COM1 = "COM1";
                    COM2 = "COM2";
                    COM6 = "COM6";
                }
                else
                {
                    COM1 = "/dev/ttyS0";
                    COM2 = "/dev/ttyS1";
                    COM6 = "/dev/ttyUSB0";
                }
                Console.WriteLine("Running as {0} bit app.",
                                                  (IntPtr.Size == 4) ? 32 : 64);
                String pkgVersion = Libserialport.sp_get_package_version_string();
                Console.WriteLine("Package Version: {0}", pkgVersion);
                Console.WriteLine("Library Version: {0}", Libserialport.sp_get_lib_version_string());
                if (pkgVersion != "0.1.1")
                {
                    Console.WriteLine("expected version 0.1.1 !!!!!!!!!!!!");
                }
                AwaitEvents(COM1, COM2, COM6);
                HandleErrors();
                ListPorts();
                OpenClose(COM6);
                PortConfig(COM2);
                PortInfo(COM6);
                SendReceive(COM2);
                //SendReceive(COM2, COM1);
                Signals(COM6);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("----------------------------------------"
                              + "---------------------------------");
            Console.Write("press any key");
            Console.ReadKey();
        }

        private static void AwaitEvents(params String[] args)
        {
            Console.WriteLine("----------------------------------------"
                              + "--------------- AwaitEvents -----");
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: {0} <port name>...\n",
                                  AppDomain.CurrentDomain.FriendlyName);
                return;
            }
            int num_ports = args.Length;
            string[] port_names = new string[num_ports];
            Array.Copy(args, port_names, num_ports);
            SerialPortObj[] ports = new SerialPortObj[num_ports];

            /* The set of events we will wait for. */
            using (EventSetObj eventSet = new EventSetObj())
            {
                /* Open and configure each port, and then add its RX event
                 * to the event set. */
                for (int i = 0; i < num_ports; i++)
                {
                    Console.WriteLine("Looking for port {0}.", port_names[i]);
                    ports[i] = new SerialPortObj(port_names[i]);

                    Console.WriteLine("Opening port.");
                    ports[i].open(sp_mode.SP_MODE_READ_WRITE);

                    Console.WriteLine("Setting port to 9600 8N1, no flow control.");
                    ports[i].setBaudrate(9600);
                    ports[i].setBits(8);
                    ports[i].setParity(sp_parity.SP_PARITY_NONE);
                    ports[i].setStopbits(1);
                    ports[i].setFlowcontrol(sp_flowcontrol.SP_FLOWCONTROL_NONE);

                    Console.WriteLine("Adding port RX event to event set.");
                    eventSet.addPortEvents(ports[i], sp_event.SP_EVENT_RX_READY);
                }

                /* Now we can call sp_wait() to await any event in the set.
                 * It will return when an event occurs, or the timeout elapses. */
                Console.WriteLine("Waiting up to 3 seconds for RX on any port...");
                eventSet.wait(3000);

                showRxBytes(ports);

                if (num_ports >= 2)
                {
                    // we assume that TX of port ports[1] is connected to RX.
                    Thread t = new Thread(
                        () =>
                        {
                            Thread.Sleep(500);
                            Console.WriteLine("writing 'hello' to port {0}",
                                              ports[1].Name);
                            ports[1].blockingWrite(
                                           Encoding.ASCII.GetBytes("hello"), 0);
                            Console.WriteLine("writing done");
                        });
                    t.Start();
                }

                /* Now we can call sp_wait() to await any event in the set.
                 * It will return when an event occurs, or the timeout elapses. */
                Console.WriteLine("Waiting up to 5 seconds for RX on any port...");
                eventSet.wait(5000);

                showRxBytes(ports);
            } // eventSet.Dispose() called here.

            /* Close ports and free resources. */
            for (int i = 0; i < ports.Length; ++i)
            {
                ports[i].close(); 
                ports[i].Dispose();
                ports[i] = null;
            }
        }

        private static void showRxBytes(SerialPortObj[] ports)
        {
            /* Iterate over ports to see which have data waiting. */
            for (int i = 0; i < ports.Length; i++)
            {
                /* Get number of bytes waiting. */
                int bytes_waiting = ports[i].inputWaiting();
                Console.Write("Port {0}: {1} bytes received.",
                                  ports[i].Name, bytes_waiting);
                if (bytes_waiting > 0)
                {
                    byte[] buffer = new byte[bytes_waiting];
                    int count = ports[i].nonblockingRead(buffer);
                    if (count != bytes_waiting) Console.Write(" => missmatch");
                    Console.Write(" => '" + Encoding.ASCII.GetString(buffer) + "'");
                }
                Console.WriteLine();
            }
        }

        private static void HandleErrors()
        {
            Console.WriteLine("----------------------------------------"
                              + "------------ HandleErrors() -----");
            SerialPortObj port = null;
            try
            {
                try
                {
                    Console.WriteLine("Trying to find a port that doesn't exist.");
                    port = new SerialPortObj("NON-EXISTENT-PORT");
                }
                catch (ArgumentException ex)
                {
                    // On linux this is thrown. On Windows not.
                    Console.WriteLine("SerialPortObj throwed exception: ArgumentException");
                    Console.WriteLine("ex.Message={0}", ex.Message);
                }
                if (port == null) return;
                Console.WriteLine("Trying to open.");
                try
                {
                    port.open(sp_mode.SP_MODE_READ_WRITE); // should throw
                }
                catch (LibSerialPortException ex)
                {
                    Console.WriteLine("open throwed exception: LibSerialPortException");
                    Console.WriteLine("ex.HResult={0}", ex.HResult);
                    Console.WriteLine("ex.Message={0}", ex.Message);
                }
                Console.WriteLine("Trying to close.");
                try
                {
                    port.close();
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("close throwed exception: ArgumentException");
                    Console.WriteLine("ex.Message={0}", ex.Message);
                }
            }
            finally
            {
                if (port != null)
                {
                    port.Dispose(); 
                    port = null;
                }
            }
        }

        private static void ListPorts()
        {
            Console.WriteLine("----------------------------------------"
                              + "----------------- ListPorts -----");
            SerialPortObj[] portList = new SerialPortObj[0];
            try
            {
                portList = SerialPortObj.listPorts();
                int i = 0;
                foreach (SerialPortObj spo in portList)
                {
                    Console.WriteLine("portList[{0}]: {1} - {2}", i,
                                      spo.Name, spo.Description);
                    ++i;
                }
            }
            finally
            {
                // We could dispose explicit to free memory asap.
                // Otherwise we can rely on garbage collection.
                foreach (var spo in portList) spo.Dispose();
            }
        }

        private static void OpenClose(String portName)
        {
            Console.WriteLine("----------------------------------------"
                              + "--------- OpenClose(\"{0}\") -----", portName);
            using (SerialPortObj port = new SerialPortObj(portName))
            {
                Console.WriteLine("handle={0}", port.Handle.ToInt64().ToString("X08"));
                Console.WriteLine("open on {0}", portName);
                port.open(sp_mode.SP_MODE_READ_WRITE);
                Console.WriteLine("handle={0}", port.Handle.ToInt64().ToString("X08"));
                Console.WriteLine("close on {0}", portName);
                port.close();
                Console.WriteLine("handle={0}", port.Handle.ToInt64().ToString("X08"));
            }
        }

        private static void PortConfig(String portName)
        {
            Console.WriteLine("----------------------------------------"
                              + "-------- PortConfig(\"{0}\") -----", portName);
            Console.WriteLine("Looking for port {0}.", portName);
            using (SerialPortObj port = new SerialPortObj(portName))
            {
                /* Display some basic information about the port. */
                Console.WriteLine("Port name: {0}", port.Name);
                Console.WriteLine("Description: {0}", port.Description);
                Console.WriteLine("Opening port.");
                port.open(sp_mode.SP_MODE_READ_WRITE);
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
                port.setBaudrate(115200);
                port.setBits(8);
                port.setParity(sp_parity.SP_PARITY_NONE);
                port.setStopbits(1);
                port.setFlowcontrol(sp_flowcontrol.SP_FLOWCONTROL_NONE);
                /* A pointer to a struct sp_port_config, which we'll use for the
                 * config read back from the port. */
                using (PortConfigObj initialConfig = port.getConfig())
                {
                    int baudrate = initialConfig.Baudrate;
                    int bits = initialConfig.Bits;
                    int stopbits = initialConfig.Stopbits;
                    sp_parity parity = initialConfig.Parity;
                    Console.WriteLine("Baudrate: {0}, data bits: {1}, parity: {2}, "
                                      + "stop bits: {3}", baudrate, bits,
                                      parity, stopbits);
                    sp_rts rts = initialConfig.Rts;
                    sp_cts cts = initialConfig.Cts;
                    sp_dtr dtr = initialConfig.Dtr;
                    sp_dsr dsr = initialConfig.Dsr;
                    sp_xonxoff xonxoff = initialConfig.XonXoff;
                    /* Create a different configuration to have ready for use. */
                    Console.WriteLine("Creating new config for 9600 7E2, XON/XOFF flow control.");
                    using (PortConfigObj otherConfig = new PortConfigObj())
                    {
                        otherConfig.Baudrate = 9600;
                        otherConfig.Bits = 7;
                        otherConfig.Baudrate = 9600;
                        otherConfig.Parity = sp_parity.SP_PARITY_EVEN;
                        otherConfig.Stopbits = 2;
                        otherConfig.Flowcontrol = sp_flowcontrol.SP_FLOWCONTROL_XONXOFF;
                        /* We can apply the new config to the port in one call. */
                        Console.WriteLine("Applying new configuration.");
                        port.setConfig(otherConfig);
                    }
                    /* And now switch back to our original config. */
                    Console.WriteLine("Setting port back to previous config.");
                    port.setConfig(initialConfig);
                }
                /* Now clean up by closing the port and freeing structures. */
                port.close();
                // initialConfig and otherConfig are cleand up by leaving using block.
            }
        }

        private static void PortInfo(String portName)
        {
            Console.WriteLine("----------------------------------------"
                              + "---------- PortInfo(\"{0}\") -----", portName);
            using (SerialPortObj port = new SerialPortObj(portName))
            {
                WriteInfo(port);
            }
        }

        private static void SendReceive(params String[] args)
        {
            Console.WriteLine("----------------------------------------"
                              + "------- SendReceive(\"{0}\") -----", args[0]);
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Usage: <port 1> [<port 2>]");
                return;
            }
            int num_ports = args.Length;
            string[] port_names = new string[num_ports];
            Array.Copy(args, port_names, num_ports);
            SerialPortObj[] ports = new SerialPortObj[num_ports];
            for (int i = 0; i < num_ports; i++)
            {
                Console.WriteLine("Looking for port {0}.", port_names[i]);
                ports[i] = new SerialPortObj(port_names[i]);

                Console.WriteLine("Opening port.");
                ports[i].open(sp_mode.SP_MODE_READ_WRITE);

                Console.WriteLine("Setting port to 9600 8N1, no flow control.");
                ports[i].setBaudrate(9600);
                ports[i].setBits(8);
                ports[i].setParity(sp_parity.SP_PARITY_NONE);
                ports[i].setStopbits(1);
                ports[i].setFlowcontrol(sp_flowcontrol.SP_FLOWCONTROL_NONE);
            }
                        /* Now send some data on each port and receive it back. */
            for (int tx = 0; tx < num_ports; tx++)
            {
                /* Get the ports to send and receive on. */
                int rx = num_ports == 1 ? 0 : ((tx == 0) ? 1 : 0);
                SerialPortObj tx_port = ports[tx];
                SerialPortObj rx_port = ports[rx];

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
                                  data, size, tx_port.Name);
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                result = tx_port.blockingWrite(buffer, size, timeout);

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
                                  size, rx_port.Name);
                result = rx_port.blockingRead(buf, size, timeout);

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
            for (int i = 0; i < num_ports; i++)
            {
                ports[i].close();
                ports[i].Dispose();
            }

            return;
        }

        private static void Signals(string portName)
        {
            Console.WriteLine("----------------------------------------"
                              + "----------- Signals(\"{0}\") -----", portName);
            using (SerialPortObj port = new SerialPortObj(portName))
            {
                port.open();
                sp_signal mask = port.getSignals();
                Console.WriteLine("mask={0}", mask);
                Console.WriteLine("sent break");
                port.startBreak();
                Console.WriteLine("sleep 2000ms");
                Thread.Sleep(2000);
                Console.WriteLine("end break");
                port.endBreak();
                port.close();
            }
        }

        private static void WriteInfo(SerialPortObj port)
        {
            Console.WriteLine("Name: {0}", port.Name);
            Console.WriteLine("Description: {0}", port.Description);
            Console.WriteLine("Transport: {0}", port.Transport);
            if (port.Transport == sp_transport.SP_TRANSPORT_USB)
            {
                Console.WriteLine("UsbBus: {0}",
                                        port.UsbBus);
                Console.WriteLine("UsbAddress: {0}",
                                        port.UsbAddress);
                Console.WriteLine("UsbVendorId: {0}",
                                        port.UsbVendorId.ToString("X04"));
                Console.WriteLine("UsbProductId: {0}",
                                        port.UsbProductId.ToString("X04"));
                Console.WriteLine("UsbSerial: {0}",
                                        port.UsbSerial == null
                                            ? "null"
                                            : port.UsbSerial);

                Console.WriteLine("UsbManufacturer: {0}",
                                        port.UsbManufacturer == null
                                            ? "null"
                                            : port.UsbManufacturer);
                Console.WriteLine("UsbProduct: {0}",
                                        port.UsbProduct == null
                                            ? "null"
                                            : port.UsbProduct);
            }
            if (port.Transport == sp_transport.SP_TRANSPORT_BLUETOOTH)
            {

                Console.WriteLine("BluetoothAddress: {0}",
                                        port.BluetoothAddress == null
                                                   ? "null"
                                                   : port.BluetoothAddress);
            }
            Console.WriteLine("Handle: " + port.Handle.ToInt64().ToString("X08"));
        }
    }
}
