# Adyen-Library testing

## Preface / Background

For one of our projects we need to get Adyen-Lib up to v32.1 (latest at 2025-09-01) under dotnet 8.

This project is being deployed on Raspberry Pis 4 (arm64), running Raspbian 12 (bookworm 64 bit).

## Describe your problem

The Adyen-lib appears to be unable to establish a SSL connection to the local payment-terminal (a Verifone P400-Plus).

The root-CA for the live-fleet is attached to the trust store of the OS, as verified by an openssl command which returns a successful SSL-handshake.

When running the software of this repository the following error occurs:

```
Creating device...
Creating client...
Creating local ApiService...
Performing diagnosis...
Constructing request...
Sending request...
System.Net.Http.HttpRequestException: The SSL connection could not be established, see inner exception.
```

This happens on this line:

```C#
response = await _terminal.RequestEncryptedAsync(diagnosisRequest, _encryptionCredentials, CancellationToken.None);
```
The exception itself comes from inside the Adyen .NET library somewhere.


## What we tried:

### Get the inner exception

The inner exception added no value to the output. It seems to be try-caught somewhere and swallowed.

### Replace HTTP client with "own" client
#### Implement own SSL verification

Did both, the custom client and the verification method did not solve the problem.
We moved the certificate right next to the app and attached it to the trustchain in the SSL verification callback (`ServerCertificateValidationCallback`).


### How we tested the handshake

```bash
openssl s_client -connect <ip_of_terminal>:8443
```

(The adyen root-CA was added to the trust store beforehand).

Response was a successful handshake. With our app on the same system it failed.

## How to reproduce

1. Clone repo
2. add credentials to `Program.cs`
3. build for arm64 and run on a raspberry pi 4/5 with bookworm 12.
