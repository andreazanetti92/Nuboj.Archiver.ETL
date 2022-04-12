using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuboj.Archiver.ETL.Saver;
using Nuboj.Archiver.ETL.Saver.Services;
using Serilog;

// ## Set the services
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddSerilog())
    .AddScoped<Program>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("/DATA_FOLDER/logs/cleaner-logging.log")
    .CreateLogger();

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetService<ILogger<Program>>();

// ## Used by other projects

string filePath = "/DATA_FOLDER/start-cleaner.txt";
string saverFilePath = "/DATA_FOLDER/start-saver.txt";

Console.Clear();
await Task.Delay(3000);

//DataDbContext context = new DataDbContext();
//DbService populator = new DbService(logger);

while (true)
{
    Console.Clear();
    try
    {
        if (File.Exists(filePath))
        {
            if(File.Exists(saverFilePath)) File.Delete(saverFilePath);

            foreach (var file in Directory.EnumerateFiles("/DATA_FOLDER", "*.json"))
            {
                Console.Clear();
                Console.WriteLine($"Deleting file from dir: {file}");
                File.Delete(file);
                await Task.Delay(1000);
            }
            Console.WriteLine("All files were deleted ...");

            //Console.WriteLine("DeleteJson");
            //try
            //{
            //    await populator.DeleteJsonFileAsync(context);
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("There are no JSON Files in the Folder\nWaiting Transferer to populate it...");
            //    logger.LogWarning($"{DateTime.UtcNow} - Saver: There are no JSON Files in the Folder\nWaiting Transferer to populate it... \n");
            //}
            File.Delete(filePath);
            continue;
        }

        await Task.Delay(3000);
        Console.Clear();
        Console.WriteLine("Waiting for Saver ...");
        continue;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Something wrong in calling saver");
        logger.LogError($"Tranferes: Something wrong in calling saver - \nException: {ex}");
    }
}
