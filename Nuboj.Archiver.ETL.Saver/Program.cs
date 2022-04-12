// See https://aka.ms/new-console-template for more information
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuboj.Archiver.ETL.Saver;
using Nuboj.Archiver.ETL.Saver.Exceptions;
using Nuboj.Archiver.ETL.Saver.Services;
using Serilog;

// ## Set the services
var services = new ServiceCollection();
services.AddDbContext<DataDbContext>();
services.AddLogging(configure => configure.AddSerilog())
    //.AddScoped<DataDbContext>()
    .AddScoped<DbService>()
    .AddScoped<Program>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("/DATA_FOLDER/logs/saver-logging.log")
    .CreateLogger();

var serviceProvider = services.BuildServiceProvider();
var dbContext = serviceProvider.GetRequiredService<DataDbContext>();
var logger = serviceProvider.GetService<ILogger<Program>>();

//Migrate if there are some pending migrations
await dbContext.Database.MigrateAsync();

JArray jarray = new JArray();
DbService populator = new DbService(logger);

Console.Clear();
await Task.Delay(2000);

while (true)
{
    Console.Clear();
    await Task.Delay(5000);
    var saverFile = Directory.GetFiles("/DATA_FOLDER", "start-saver.txt");

    if (saverFile.Any() && File.Exists(saverFile[0]))
    {
        dbContext = new DataDbContext();

        // Get previous files from db
        var fileNames = await populator.ReadJsonFileOnDbAsync(dbContext);
        var fileNamesFromDir = Directory.GetFiles("/DATA_FOLDER", "*.json").ToList();

        if (fileNames.Any() && fileNamesFromDir.Any())
        {
            Console.WriteLine("Starting to compare the files from db with those from dir");

            await Task.Delay(3000);

            List<string> comparerList = new List<string>();

            Parallel.ForEach(fileNamesFromDir, file =>
            {
                file = file.Substring(file.LastIndexOf("/") + 1);
                lock (comparerList)
                {
                    comparerList.AddRange(fileNames.Where(f => f.Contains(file)));
                }
            });

            if (!comparerList.Any())
            {
                foreach (var file in fileNamesFromDir)
                {
                    //if name are different
                    Console.WriteLine($"Starting to parse file: \nfrom dir: {file}");
                    // parse
                    var rawJson = JsonConvert.DeserializeObject(File.ReadAllText(file));
                    var jsonAsString = rawJson.ToString().Replace("][", ",");
                    jarray.Add(JArray.Parse(jsonAsString));

                    // Write new files to db
                    await populator.WriteJsonFilesOnDbAsync(dbContext, file);
                }
                if (jarray.Any()) await PopulateAsync(saverFile);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("The files in the folder are already saved on Database");
                logger.LogInformation($"{DateTime.UtcNow} - Saver: The files in the folder are already saved on Database\n");

                // AN INELEGANT SOLUTION
                string filePath = "/DATA_FOLDER/start-cleaner.txt";
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    File.Create(filePath);
                    // Delete Log
                    //var logFilePath = "/DATA_FOLDER/logs/saver-logging.txt";
                    //if(File.Exists(logFilePath)) File.Delete(logFilePath);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something wrong in calling saver");
                    logger.LogError($"Tranferer: Something wrong in calling saver - \nException: {ex}");
                }
                await Task.Delay(1000);
                continue;
            }
        }
        else
        {
            if (fileNamesFromDir.Any())
            {
                foreach (var fileName in fileNamesFromDir)
                {
                    Console.WriteLine($"Starting to parse file: \nfrom dir: {fileName}");
                    // parse
                    var rawJson = JsonConvert.DeserializeObject(File.ReadAllText(fileName));
                    var jsonAsString = rawJson.ToString().Replace("][", ",");
                    jarray.Add(JArray.Parse(jsonAsString));

                    // Write new files to db
                    // Windows
                    //var fileNameToSave = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                    var fileNameToSave = fileName.Substring(fileName.LastIndexOf("/") + 1);
                    await populator.WriteJsonFilesOnDbAsync(dbContext, fileNameToSave);
                }
                Console.WriteLine(jarray.Count);
                if (jarray.Any()) await PopulateAsync(saverFile);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("There are no JSON Files in the Folder\nWaiting Transferer to populate it...");
                logger.LogWarning($"{DateTime.UtcNow} - Saver: There are no JSON Files in the Folder\nWaiting Transferer to populate it... \n");
                await Task.Delay(3000);
            }
        }
        continue;
    }
    continue;
}


async Task PopulateAsync(string[] saverFile)
{
    if (jarray.Count > 0)
    {
        int tries1 = 0;
        bool success = false;
        while (!success)
        {
            try
            {
                Console.WriteLine("Trying to populate the Database with the populator");
                var result = await populator.PopulateDbWithJsonAsync(dbContext, jarray);
                if (result is true)
                {
                    success = true;
                    Console.WriteLine("Database populated successfully");
                    File.Delete(saverFile[0]);
                    // AN INELEGANT SOLUTION
                    string filePath = "/DATA_FOLDER/start-cleaner.txt";
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        File.Create(filePath);

                        // Delete Log
                        //var logFilePath = "/DATA_FOLDER/logs/saver-logging.txt";
                        //if(File.Exists(logFilePath)) File.Delete(logFilePath);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Something wrong in calling saver");
                        logger.LogError($"Tranferer: Something wrong in calling saver - \nException: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.UtcNow} - Saver Error:\n {ex.Message}  \n{ex.StackTrace} \n");
                Console.WriteLine("Failed to populate the Database");
                tries1++;
                if (tries1 == 10) throw;
            }
        }
    }
}
