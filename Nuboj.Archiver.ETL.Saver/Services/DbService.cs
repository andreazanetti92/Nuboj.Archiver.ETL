using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuboj.Archiver.ETL.Saver.Exceptions;
using Nuboj.Archiver.ETL.Saver.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Nuboj.Archiver.ETL.Saver.Services
{
    public class DbService
    {
        private ILogger _logger;
        public DbService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> PopulateDbWithJsonAsync(DataDbContext context, JArray jArray)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (jArray is null) throw new ArgumentNullException(nameof(jArray));

            List<Status> statuses = new List<Status>();
            List<Nvm> nvms = new List<Nvm>();
            List<ComponentInfo> componentInfo = new List<ComponentInfo>();

            Console.WriteLine("Started to search for SensorDataType models (status, nvm and component-info)... ");

            Parallel.ForEach(jArray, item =>
            {
                if (item.GetType() == typeof(JToken))
                {
                    if (item["SensorDataType"].ToString() == "status")
                    {
                        lock (statuses)
                        {
                            statuses.Add(JsonConvert.DeserializeObject<Status>(item.ToString()));
                        }
                    }
                    else if (item["SensorDataType"].ToString() == "nvm")
                    {
                        lock (nvms)
                        {
                            nvms.Add(JsonConvert.DeserializeObject<Nvm>(item.ToString()));
                        }
                    }
                    else if (item["SensorDataType"].ToString() == "component-info")
                    {
                        lock (componentInfo)
                        {
                            componentInfo.Add(JsonConvert.DeserializeObject<ComponentInfo>(item.ToString()));
                        }
                    }
                    else
                    {
                        _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to search for SensorDataType models (status, nvm and component-info)");
                        Console.WriteLine("Failed to search for SensorDataType models (status, nvm and component-info)...");
                        throw new NullSensorDataTypeException("Could not retrive any SensorDataType Model of type 'status', 'nvm' or 'component-info");
                    }
                }
                else if (item.GetType() == typeof(JArray))
                {
                    Parallel.ForEach(item, i =>
                    {
                        if (i["SensorDataType"].ToString() == "status")
                        {
                            lock (statuses)
                            {
                                statuses.Add(JsonConvert.DeserializeObject<Status>(i.ToString()));
                            }
                        }
                        else if (i["SensorDataType"].ToString() == "nvm")
                        {
                            lock (nvms)
                            {
                                nvms.Add(JsonConvert.DeserializeObject<Nvm>(i.ToString()));
                            }
                        }
                        else if (i["SensorDataType"].ToString() == "component-info")
                        {
                            lock (componentInfo)
                            {
                                componentInfo.Add(JsonConvert.DeserializeObject<ComponentInfo>(i.ToString()));
                            }
                        }
                        else
                        {
                            _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to search for SensorDataType models (status, nvm and component-info)");
                            Console.WriteLine("Failed to search for SensorDataType models (status, nvm and component-info)...");
                            throw new NullSensorDataTypeException("Could not retrive any SensorDataType Model of type 'status', 'nvm' or 'component-info");
                        }
                    });
                }
                else
                {
                    _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to search for SensorDataType models (status, nvm and component-info)");
                    Console.WriteLine("Failed to search for SensorDataType models (status, nvm and component-info)...");
                    throw new NullSensorDataTypeException("Could not retrive any SensorDataType Model of type 'status', 'nvm' or 'component-info");
                }
            });

            if (statuses.Any() && nvms.Any() && componentInfo.Any())
            {
                Console.WriteLine("All SensorDataType were found ...");
                Console.WriteLine("Starting bulk operations ...");

                bool success = false;

                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        int count = 0;
                        int commitCount = 1000;
                        foreach(var status in statuses)
                        {
                            ++count;
                            if (statuses.Count % commitCount == statuses.Count - count)
                            {
                                count = statuses.Count - count;
                                commitCount = count;
                            }
                            context = AddToContext<Status>(context, status, count, commitCount, false);
                        };
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to insert status into Database");
                        Console.WriteLine("Saver Error: Failed to insert status into Database");
                    }
                    scope.Complete();
                }

                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        int count = 0;
                        int commitCount = 100;

                        foreach(var nvm in nvms)
                        {
                            ++count;
                            if (nvms.Count % commitCount == nvms.Count - count)
                            {
                                count = nvms.Count - count;
                                commitCount = count;
                            }

                            context = AddToContext<Nvm>(context, nvm, count, commitCount, false);
                        };
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to insert nvm into Database");
                        Console.WriteLine("Saver Error: Failed to insert nvm into Database");
                    }
                    scope.Complete();
                }

                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        int count = 0;
                        int commitCount = 100;
                        foreach (var component in componentInfo)
                        {
                            ++count;
                            if (componentInfo.Count % commitCount == componentInfo.Count - count)
                            {
                                count = componentInfo.Count - count;
                                commitCount = count;
                            }
                            context = AddToContext<ComponentInfo>(context, component, count, commitCount, false);   
                        }
                        success=true;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to insert ComponentInfo into Database\n Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n{ex.InnerException}");
                        Console.WriteLine("Saver Error: Failed to insert ComponentInfo into Database");
                    }
                    scope.Complete();
                }

                if (success is true) return success;
            }
            // Da aggiornare con la Bulk Operation come nel metodo sopra
            else if (statuses.Any())
            {
                Console.WriteLine("Only 'Status' SensorDataType were found");
                Console.WriteLine("Starting bulk operations ...");


                bool success = false;
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        int count = 0;
                        int commitCount = 1000;
                        foreach (var status in statuses)
                        {
                            ++count;
                            if (statuses.Count % commitCount == statuses.Count - count)
                            {
                                count = statuses.Count - count;
                                commitCount = count;
                            }
                            context = AddToContext<Status>(context, status, count, commitCount, false);
                        };
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to insert status into Database");
                        Console.WriteLine("Saver Error: Failed to insert status into Database");
                    }
                    scope.Complete();
                }

                if (success is true) return success;
            }
            else if (componentInfo.Any())
            {
                Console.WriteLine("Only 'Component-Info' SensorDataType were found");
                Console.WriteLine("Starting bulk operations ...");

                bool success = false;
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        int count = 0;
                        int commitCount = 100;
                        foreach (var component in componentInfo)
                        {
                            ++count;
                            if (componentInfo.Count % commitCount == componentInfo.Count - count)
                            {
                                count = componentInfo.Count - count;
                                commitCount = count;
                            }
                            context = AddToContext<ComponentInfo>(context, component, count, commitCount, false);
                        }
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to insert ComponentInfo into Database\n Message: {ex.Message} \nStack Trace: {ex.StackTrace}\n{ex.InnerException}");
                        Console.WriteLine("Saver Error: Failed to insert ComponentInfo into Database");
                    }
                    scope.Complete();
                }
                if (success is true) return success;
            }
            else if (nvms.Any())
            {
                Console.WriteLine("Only 'Nvms' SensorDataType were found");
                Console.WriteLine("Starting bulk operations ...");

                bool success = false;
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        int count = 0;
                        int commitCount = 100;

                        foreach (var nvm in nvms)
                        {
                            ++count;
                            if (nvms.Count % commitCount == nvms.Count - count)
                            {
                                count = nvms.Count - count;
                                commitCount = count;
                            }

                            context = AddToContext<Nvm>(context, nvm, count, commitCount, false);
                        };
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to insert nvm into Database");
                        Console.WriteLine("Saver Error: Failed to insert nvm into Database");
                    }
                    scope.Complete();
                }

                if (success is true) return success;
            }
            else
            {
                _logger.LogError($"{DateTime.UtcNow} - Saver Error: Failed to search for SensorDataType models (status, nvm and component - info)");
                Console.WriteLine("Failed to search for SensorDataType models (status, nvm and component-info)...");
                throw new NullSensorDataTypeException("Could not retrive any SensorDataType Data of type 'status', 'nvm' or 'component-info");
            }

            return false;
        }

        private DataDbContext AddToContext<T>(DataDbContext context, T entity, int count, int commitCount, bool recreateContext) where T : class
        {
            context.Set<T>().Add(entity);
            if(count % commitCount == 0)
            {
                context.SaveChanges();
                if (recreateContext)
                {
                    context.Dispose();
                    context = new DataDbContext();
                    context.ChangeTracker.AutoDetectChangesEnabled = false;
                }
            }
            return context;
        }

        public async Task WriteJsonFilesOnDbAsync(DataDbContext context, string fileName)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (fileName is null) throw new ArgumentNullException("DbService - WriteJsonFileOnDbAsync: fileName could not be null");

            await context.AddAsync(new JsonFilename
            {
                fileName = fileName
            });

            await context.SaveChangesAsync();
        }

        public async Task<List<string>> ReadJsonFileOnDbAsync(DataDbContext context)
        {
            return context.JsonFilenames.Select(x => x.fileName).ToList();
        }

        public async Task DeleteJsonFileAsync(DataDbContext context)
        {
            var entities = context.JsonFilenames
                .Select(x => new JsonFilename { fileName = x.fileName });
            
            context.RemoveRange(entities);

            await context.SaveChangesAsync();
        }
    }
}
