# libserialportCS
Binding for C# of libserialport (http://sigrok.org/wiki/Libserialport)

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

See http://sigrok.org/wiki/Libserialport.

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


