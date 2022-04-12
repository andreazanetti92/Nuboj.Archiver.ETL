using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Diagnostics;
using System.IO.Compression;

var services = new ServiceCollection();
services.AddLogging(configure => configure.AddSerilog())
    .AddScoped<Program>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("/DATA_FOLDER/logs/transferer-logging.log")
    .CreateLogger();

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetService<ILogger<Program>>();

Console.Clear();
await Task.Delay(3000);

while (true)
{
    Console.Clear();
    await Task.Delay(3000);

    Console.WriteLine("Reading args ...");
    string startdatetime = String.Empty;
    string enddatetime = String.Empty;
    string envStartdatetime = Environment.GetEnvironmentVariable("STARTDATETIME");
    string envEnddatetime = Environment.GetEnvironmentVariable("ENDDATETIME");

    // ## TAKE ARGUMENTS
    if (args.Any())
    {
        // suggested datetime format: yyyy-MM-ddThh
        //var myargs = Environment.GetCommandLineArgs();

        startdatetime = args[0];
        enddatetime = args[1];

        Console.WriteLine($"{startdatetime} - {enddatetime}");
    }
    else if(DateTimeOffset.TryParse(envStartdatetime, out DateTimeOffset startdate) && DateTimeOffset.TryParse(envEnddatetime, out DateTimeOffset enddate))
    {
        startdatetime = startdate.ToString();
        enddatetime = enddate.ToString();

        Console.WriteLine($"{startdatetime} - {enddatetime}");
    }
    else if (!args.Any() || !DateTimeOffset.TryParse(envStartdatetime, out DateTimeOffset date) || string.IsNullOrEmpty(envStartdatetime))
    {
        startdatetime = DateTimeOffset.UtcNow.AddDays(-1).ToString("yyyy-MM-ddT00:00:00");
        enddatetime = DateTimeOffset.UtcNow.AddDays(-1).ToString("yyyy-MM-ddT23:00:00");

        Console.WriteLine($"{startdatetime} - {enddatetime}");
    }
    else
    {
        Console.WriteLine("Transferer: You must provide a Start Datetime and an End DateTime separated by space. \nEx: \"2022-04-01T12:00:00\" \"2022-04-01T15:00:00\"");
        logger.LogError($"{DateTime.UtcNow} - Transferer: You must provide a Start Datetime and End DateTime separated by space.");
        continue;
    }

    Console.WriteLine("Reading accounts ...");
    // ## READ CONNECTION STRINGS FROM STORAGE ACCOUNT IN ACCOUNT_LIST FILE)
    var account_list = "/ACCOUNT_FOLDER/storage-accounts.cfg";
    List<BlobServiceClient> storageAccounts = new List<BlobServiceClient>();
    using (var stream = File.Open(account_list, FileMode.Open))
    using (StreamReader reader = new StreamReader(stream))
    {
        string connectionString = await reader.ReadLineAsync();
        while (!string.IsNullOrWhiteSpace(connectionString))
        {
            BlobServiceClient client = new BlobServiceClient(connectionString);
            storageAccounts.Add(client);

            connectionString = await reader.ReadLineAsync();
        }
    }

    // ## BUILD BLOB PATH - "{tenantId}/{anno:yyyy}/{mese:MM}/{giorno:dd}/{ora:HH}/{filename}.json.gzip"
    // Tentant_Id comes from ENV VAR
    var TENANT_ID = Environment.GetEnvironmentVariable("TENANT_ID");

    if (!string.IsNullOrEmpty(startdatetime) || !string.IsNullOrEmpty(enddatetime))
    {
        // SECTION OFF DATETIME
        var startdate = startdatetime.Split('T')[0];
        var starttime = startdatetime.Split('T')[1];

        var enddate = enddatetime.Split('T')[0];
        var endtime = enddatetime.Split('T')[1];

        // TODO: Files are uploaded every hour, if as arguments pass a rangetime of 6 hours 
        // we have to download 6 files
        Console.WriteLine("Creating path to search into blob ...");
        List<string> blob_paths = new List<string>();
        for (int i = int.Parse(starttime.Split(':')[0]); i <= int.Parse(endtime.Split(':')[0]); i++)
        {
            // Check day between start and end
            if (startdate.Split('-')[2] == enddate.Split('-')[2])
            {
                var blob_path_startdate = TENANT_ID + '/' + startdate.Split('-')[0] + '/' + startdate.Split('-')[1] + '/' + startdate.Split('-')[2] + '/' + i;
                blob_paths.Add(blob_path_startdate);
            }
            else
            {
                var blob_path_startdate = TENANT_ID + '/' + startdate.Split('-')[0] + '/' + startdate.Split('-')[1] + '/' + startdate.Split('-')[2] + '/' + i;
                var blob_path_enddate = TENANT_ID + '/' + enddate.Split('-')[0] + '/' + enddate.Split('-')[1] + '/' + enddate.Split('-')[2] + '/' + i;
                blob_paths.Add(blob_path_startdate);
                blob_paths.Add(blob_path_enddate);
            }
        }

        // Waiting saver to delete files when it has finish to populate the DB
        await Task.Delay(3000);
        // ## if there're json files into the folder
        var fileNamesFromDir = Directory.GetFiles("/DATA_FOLDER", "*.json");
        if (!fileNamesFromDir.Any())
        {
            await QueryBlobStorageAccountAsync(storageAccounts, blob_paths);
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Waiting for Saver to save the files and then delete them");
            logger.LogInformation("Waiting for Saver to save the files and then delete them");
            await Task.Delay(3000);
        }
    }
}

async Task QueryBlobStorageAccountAsync(List<BlobServiceClient> storageAccounts, List<string> blob_paths)
{
    // ## DOWNLOAD FILE/FILES - PATH "{tenantId}/{anno:yyyy}/{mese:MM}/{giorno:dd}/{ora:HH}/{filename}.json.gzip"
    // Loop through all the storage accounts, searching for our container (name: tenant_id)
    //BlobContainerClient? containerClient = null;
    List<BlobContainerClient> containerClients = new List<BlobContainerClient>();

    Console.WriteLine("Loop through the storage accounts list and populate BlobContainerClients ...");
    if (storageAccounts.Count > 0)
    {
        Parallel.ForEach(storageAccounts, account =>
        {
            Console.WriteLine(account.AccountName);
            try
            {
                var containerItems = account.GetBlobContainers().ToList();
                if (containerItems.Count > 0)
                {
                    Parallel.ForEach(containerItems, item =>
                    {
                        lock (containerClients)
                        {
                            containerClients.Add(account.GetBlobContainerClient(item.Name));
                        };
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.LogWarning($"{DateTime.UtcNow} - Transferer: Message:{ex.Message}\n Stack Trace: {ex.StackTrace} \n");
            }
        });
    }

    List<BlobClient> blobsClient = new List<BlobClient>();

    Console.WriteLine("Loop through BlobContainerClients and Blob Paths and populate blobsClient");
    if (containerClients.Count > 0 && blob_paths.Count > 0)
    {
        Parallel.ForEach(containerClients, client =>
        {
            Parallel.ForEach(blob_paths, path =>
            {
                var blobsItems = client.GetBlobs(prefix: path).ToList();

                Parallel.ForEach(blobsItems, blob =>
                {
                    lock (blobsClient)
                    {
                        blobsClient.Add(client.GetBlobClient(blob.Name));
                    }
                });
            });
        });
    }

    // ## UNZIP AND CONVERT TO JARRAY THE FILE
    using (MemoryStream compressedStream = new MemoryStream())
    using (GZipStream decompressorStream = new GZipStream(compressedStream, CompressionMode.Decompress))
    using (StreamReader reader = new StreamReader(decompressorStream))
    {
        //Download data from blob to stream
        foreach (var blob in blobsClient)
        {
            Console.Clear();
            Console.WriteLine("Downloading blob ...");
            await blob.DownloadToAsync(compressedStream);
            compressedStream.Seek(0, SeekOrigin.Begin);

            //Read, serialize and write decompressed data to DATA_FOLDER
            string jsonString = reader.ReadToEnd();
            var jsonFormated = JsonConvert.SerializeObject(jsonString, Formatting.Indented);
            var temp = blob.Name.Substring(blob.Name.LastIndexOf('/') + 1);
            var fileName = temp.Split('.')[0];
            Console.WriteLine("Saving data into file ...");
            File.WriteAllText($"/DATA_FOLDER/{fileName}.json", jsonFormated);
            await Task.Delay(500);
        }
    }

    var fileNamesFromDir = Directory.GetFiles("/DATA_FOLDER", "*.json");
    if (fileNamesFromDir.Any())
    { 
        Console.WriteLine("Calling Saver ...");
        
        // AN INELEGANT SOLUTION
        string filePath = "/DATA_FOLDER/start-saver.txt";
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Create(filePath);

            // Delete Log
            var logFilePath = "/DATA_FOLDER/logs/transferer-logging.txt";
            if(File.Exists(logFilePath)) File.Delete(logFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something wrong in calling saver");
            logger.LogError($"Tranferes: Something wrong in calling saver - \nException: {ex}");
        }

        // To start the process on windows
        //await Task.Delay(3000);
        //ProcessStartInfo startInfo = new ProcessStartInfo();
        //startInfo.UseShellExecute = false;
        //startInfo.CreateNoWindow = false;
        //startInfo.FileName = @"C:\Users\ext-azanetti\source\repos\Nuboj.Archiver.ETL\Nuboj.Archiver.ETL.Saver\bin\Debug\net6.0\Nuboj.Archiver.ETL.Saver.exe";
        //await Process.Start(startInfo)
        //    .WaitForExitAsync();

        // Using Docker.Net and Docker for Windows
        //DockerClient dockerClient = new DockerClientConfiguration(
        //    new Uri("npipe://./pipe/docker_engine"))
        //    .CreateClient();

        //// saver container id: "36598771e257"
        //Console.WriteLine("Start container saver");
        //await dockerClient.Containers.StartContainerAsync(
        //    "saver",
        //    new ContainerStartParameters()
        //    );

        return;
    }
}
