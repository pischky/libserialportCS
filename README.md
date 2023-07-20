# libserialportCS
Binding for C# of libserialport 
([http://sigrok.org/wiki/Libserialport](http://sigrok.org/wiki/Libserialport)).

## Usage

### Simple Example

````
using System;
using System.Text;
using libserialport;

namespace Simple
{
  class Program
  {
    static void Main(string[] args)
    {
      using (SerialPortObj port = new SerialPortObj("COM2"))
      {
        port.open();
        port.setBaudrate(9600);
        byte[] buffer1 = Encoding.ASCII.GetBytes("Hello");
        int cnt = port.blockingWrite(buffer1, 0);
        byte[] buffer2 = new byte[50];
        cnt = port.blockingRead(buffer2, 1000);
        String s = Encoding.ASCII.GetString(buffer2, 0, cnt);
        Console.WriteLine(s);
        port.close();
      }
    }
  }
}
````

Add `libserialportCS.dll` to your project. Set "Build Action"
to `Content` and "Copy to Output Directory" to `Copy if newer`.
Add a Reference of this DLL to your project.

Create two folders `x86` and `x86_64` in your project
and add the native DLLs to them. Set "Build Action" 
and "Copy to Output Directory" for all DLLs to values 
`Content` and `Copy if newer`.

![Visual Studio Settings](vs-settings.png "Visual Studio Settings")

### Configuration Example

````
using (SerialPortObj port = new SerialPortObj(portName))
{
  port.open(sp_mode.SP_MODE_READ_WRITE);
  using (PortConfigObj savedConfig = port.getConfig())
  {
    using (PortConfigObj myConfig = new PortConfigObj())
    {
      myConfig.Baudrate = 9600;
	  myConfig.Bits = 8;
      myConfig.Parity = sp_parity.SP_PARITY_NONE;
      myConfig.Stopbits = 1;
      myConfig.Flowcontrol = sp_flowcontrol.SP_FLOWCONTROL_XONXOFF;
      port.setConfig(myConfig);
    }
    transferSomeData(port);
    port.setConfig(savedConfig);
  }
  port.close();
}
````

### Serial Port Enumaration Example

````
const int VID = 0x0403; const int PID = 0xC7D0;
SerialPortObj[] portList = new SerialPortObj[0];
SerialPortObj myPort = null;
try
{
  portList = SerialPortObj.listPorts();
  int i = 0;
  foreach (SerialPortObj spo in portList)
  {
    Console.WriteLine("portList[{0}]: {1} - {2}", i,
                      spo.Name, spo.Description);
    if (spo.Transport == sp_transport.SP_TRANSPORT_USB)
    {
      Console.WriteLine("USB Port: {0}, {1}",
                        spo.UsbManufacturer, spo.UsbProduct);
      if (spo.UsbVendorId == VID && spo.UsbProductId == PID)
      {
        myPort = spo;
      }
    }
    ++i;
  }
}
finally
{
  // We could dispose explicit to free memory asap.
  // Otherwise we can rely on garbage collection.
  foreach (var spo in portList)
  {
    if (!spo.Equals(myPort)) spo.Dispose();
  }
}
if (myPort != null)
{
  try
  {
    myPort.open();
    transferSomeData(myPort);
    myPort.close();
  }
  finally
  {
    myPort.Dispose();
  }
}
````

## Internals

The namespace `libserialport` contains four highlevel 
classes:

 - `SerialPortObj` abstraction of single serial port. Also 
    contains enumeration of port as static methodes.
 - `PortConfigObj` used to save, restore or set a 
    configuration of a port.
 - `EventSetObj` used to configure a set of events
    and wait on one ore more events.
 - `LibSerialPortException` report exeptions in methods
    of above classes.
   
The low level class `Libserialport` is used to communicate 
with the functions written in C.

![Static Class Diagram](static-classes.svg "Classes in namespace libserialport")

## Building

### Windows using MinGW64

Start MSYS2 MinGW 64 bit shell (Start >> MSYS2 64bit >> MSYS2 64bit >> MSYS2 MinGW 64-bit)

````
$ cd /sandbox/libserialport/libserialport/
$ make -f mingw64.mak clean all
$ ls -l libserialport.dll
-rwxr-xr-x 1 martin None 87552 Jul 16 10:39 libserialport.dll

$ cp libserialport.dll ../cs/libserialport/x86_64/
$
````

Start SYS2 MinGW 32 bit shell (Start >> MSYS2 64bit >> MSYS2 64bit >> MSYS2 MinGW 32-bit)

````
$ cd /sandbox/libserialport/libserialport/
$ make -f mingw64.mak clean all
$ $ ls -l libserialport.dll
-rwxr-xr-x 1 martin None 97280 Jul 16 10:43 libserialport.dll

$ cp libserialport.dll ../cs/libserialport/x86/
$ cp /mingw32/bin/libgcc_s_dw2-1.dll ../cs/libserialport/x86/
$ cp /mingw32/bin/libwinpthread-1.dll ../cs/libserialport/x86/
$ ls -l ../cs/libserialport/x86/
total 324
-rwxr-xr-x 1 martin None 157895 Jul 16 10:48 libgcc_s_dw2-1.dll
-rwxr-xr-x 1 martin None  97280 Jul 16 10:48 libserialport.dll
-rwxr-xr-x 1 martin None  71838 Jul 16 10:49 libwinpthread-1.dll

$
````

### Windows using VS2019

See [http://sigrok.org/wiki/Libserialport](http://sigrok.org/wiki/Libserialport).

### Linux

````
$ git clone git://sigrok.org/libserialport
$ cd libserialport/
$ ./autogen.sh
$ ./configure
$ make
$ sudo make install
$ cd /usr/local/lib/
$ sudo rm libserialport.so
$ sudo rm libserialport.so.0
$ sudo ln -s libserialport.so.0.1.0 libserialport.so.0
$ sudo ln -s libserialport.so.0 libserialport.so
$ sudo ldconfig
````

## TODOs

- Add more documentation comments.
- More testing.

