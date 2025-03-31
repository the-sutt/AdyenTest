FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# download adyen certificates
RUN apt-get install -y wget
WORKDIR /home/certs
RUN wget https://docs.adyen.com/point-of-sale/design-your-integration/choose-your-architecture/local/adyen-terminalfleet-live.pem -O adyen-terminalfleet-live.pem
RUN openssl x509 -outform der -in adyen-terminalfleet-live.pem -out adyen-terminalfleet-live.cer

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# copy and install adyen certificates
COPY --from=build-env /home/certs/* /usr/local/share/ca-certificates/
RUN update-ca-certificates

WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT [ "dotnet", "AdyenTest.dll" ]