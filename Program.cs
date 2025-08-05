
using System;
using System.Threading;
using Adyen.ApiSerialization;
using Adyen.Security;
using System.Threading.Tasks;
using Adyen;
using Adyen.Model.TerminalApi;
using Adyen.Model.TerminalApi.Message;
using Adyen.Service;
using Environment = Adyen.Model.Environment;

class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("Creating device...");
        var terminal = new PaymentTerminalAdyen(host: "<ip>", port: 8443, serialNumber: "<serialnumber>"); // TODO: fill

        while (true)
        {
            Console.WriteLine("Performing diagnosis...");
            var testRes = terminal.TryTestAsync().Result;

            Console.WriteLine("Test-Result: " + testRes);

            Console.WriteLine("Waiting 5 seconds...");
            Task.Delay(5000).Wait(); // Wait for 5 seconds before the next test
        }
        return 0;
    }
}

class PaymentTerminalAdyen
{
    private EncryptionCredentialDetails _encryptionCredentials;
    private string _apiKey = "<apiKey>"; // TODO: fill
    private Client _client;
    private ITerminalApiLocalService _terminal;
    private string _serialNumber; 

    public PaymentTerminalAdyen(string host, int port, string serialNumber)
    {
        _serialNumber = serialNumber;
        _encryptionCredentials ??= new EncryptionCredentialDetails
        {
            AdyenCryptoVersion = 1,
            KeyVersion = <keyVersion>, // TODO: fill
            KeyIdentifier = "<KeyIdent>", // TODO: fill
            Password = "<Password>", // TODO: fill
        };
        var config = new Config
        {
            XApiKey = _apiKey,
            LocalTerminalApiEndpoint = $"https://{host}:{port}/nexo/",
            Environment = Environment.Live,
            Timeout = 255_000
        };
        Console.WriteLine("Creating client...");
        _client = new Client(config);
        Console.WriteLine("Creating local ApiService...");
        _terminal = new TerminalApiLocalService(_client, new SaleToPoiMessageSerializer(), new SaleToPoiMessageSecuredEncryptor(), new SaleToPoiMessageSecuredSerializer());
    }

    public async Task<bool> TryTestAsync()
    {
        Console.WriteLine("Constructing request...");

        var diagnosisRequest = new SaleToPOIRequest
        {
            MessageHeader = new MessageHeader
            {
                MessageClass = MessageClassType.Service,
                MessageCategory = MessageCategoryType.Diagnosis,
                SaleID = "THIS_IS_A_TEST_SALEID",
                ServiceID = "D" + "X" + DateTime.Now.ToString("ddHHmmss"),
                POIID = _serialNumber
            },
            MessagePayload = new DiagnosisRequest
            {
                POIID = _serialNumber
            }
        };

        SaleToPOIResponse response;
        DiagnosisResponse diagnosisResponse;

        try
        {
            Console.WriteLine("Sending request...");
            response = await _terminal.RequestEncryptedAsync(diagnosisRequest, _encryptionCredentials, CancellationToken.None);
            Console.WriteLine("Received response...");
            diagnosisResponse = response?.MessagePayload as DiagnosisResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: diagnosis request failed: " + ex.Message);
            return false;
        }

        if (response == null || diagnosisResponse == null)
        {
            Console.WriteLine("ERROR: diagnosis response could not be deserialized");
            return false;
        }

        if (diagnosisResponse.Response.Result == ResultType.Success &&
            (diagnosisResponse.POIStatus.CommunicationOKFlag?.Equals(true) ?? false) &&
            diagnosisResponse.POIStatus.GlobalStatus == GlobalStatusType.OK)
        {
            Console.WriteLine("SUCCESS: diagnosis request ok");
            return true;
        }

        Console.WriteLine("FAILED: diagnosis failed: " + diagnosisResponse.Response.AdditionalResponse);
        return false;
    }
}