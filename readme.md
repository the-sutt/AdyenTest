# Adyen-Library testing

## Preface / Background

For one of our projects we need to get Adyen-Lib up to v31 (latest at 2025-03-31) under dotnet 8.

This project is being deployed on Raspberry Pis 4, running Raspbian 10 (buster 32 bit).

At this moment we can't change that - we are going to migrate to a later Raspbian image but for now we are forced to use buster 32 bit.

Note: The software in general is working and we are running multiple terminals with an old build under .NET 3.1 on arm32v7 and it works fine. But that Adyen lib there has a major version still in the single digits.  
That's why we were tasked to move to dotnet 8 and also a later Adyen lib.

## Describe your problem

My image builds with
- `sdk:8.0-bookworm-slim` for platform `linux/arm/v7`
- `sdk:8.0-bookworm-slim` for platform `linux/arm64`
- `sdk:8.0` for platform `linux/arm/v7`
- `sdk:8.0` for platform `linux/arm64`

The build command is

```bash
docker buildx build --platform <platform> -t adyentest:latest .
```

Note: Built and pushed on a Windows 11 machine, pulled on the pi.

Also note: I know I can build for multiple platforms at once, this is just for testing. :)

## Results

### on a 64 bit OS

- Raspberry Pi 4
- Raspberry Pi OS 12 (bookworm)
- `uname -a`
```
Linux xxxxx 6.6.20+rpt-rpi-v8 #1 SMP PREEMPT Debian 1:6.6.20-1+rpt1 (2024-03-07) aarch64 GNU/Linux
```

**✅ all of the above builds run and execute as desired**


### on a 32 bit OS

- Raspberry Pi 4
- Raspberry Pi OS 10 (buster)
- `Linux xxxxx 5.10.103-v7l+ #1529 SMP Tue Mar 8 12:24:00 GMT 2022 armv7l GNU/Linux`

☑️ expected: can't download the arm64 image.
- `no matching manifest for linux/arm/v7 in the manifest list entries`

But when building for `arm32v7`:

#### 🛑 ERROR

With any 32 bit build created above the program starts, but crashes midway:

```
Creating device...
Creating client...
Creating local ApiService...
Performing diagnosis...
Constructing request...
Sending request...
Unhandled exception. System.Runtime.InteropServices.COMException (0x8007054F): An internal error occurred.
 (0x8007054F)
   at System.Threading.WaitHandle.WaitOneCore(IntPtr waitHandle, Int32 millisecondsTimeout)
   at System.Threading.WaitHandle.WaitOneNoCheck(Int32 millisecondsTimeout)
   at System.Threading.TimerQueue.TimerThread()
   at System.Threading.Thread.StartCallback()
Fatal error. Internal CLR error. (0x80131506)
   at System.Threading.WaitHandle.WaitOneCore(IntPtr, Int32)
   at System.Threading.WaitHandle.WaitOneNoCheck(Int32)
   at System.Threading.TimerQueue.TimerThread()
   at System.Threading.Thread.StartCallback()
```

This happens on this line:

```C#
response = await _terminal.RequestEncryptedAsync(diagnosisRequest, _encryptionCredentials, CancellationToken.None);
```
The exception itself comes from inside the Adyen .NET library somewhere.

I hope you can point me in the right direction or confirm that the Adyen Lib v31 is incompatible with `arm32v7`.