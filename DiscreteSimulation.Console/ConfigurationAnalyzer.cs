using Simulation;

namespace DiscreteSimulation.Console;

public class ConfigurationAnalyzer
{
    private SimulationObject[] _simulations;
    
    private int _replications;

    private readonly string _basePath;
    
    private readonly Lock _lockSimulations = new();
    
    private readonly Lock _lockConfigurations = new();
    
    private SemaphoreSlim _semaphoreAvailableSimulations;
    
    public Queue<int[]> Configurations { get; set; } = new();

    public List<ResultObject> Results { get; set; } = new();

    public ConfigurationAnalyzer(int countOfParallelSimulations = 1)
    {
        _basePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        Initialize(countOfParallelSimulations);
    }
    
    public ConfigurationAnalyzer(string basePath, int countOfParallelSimulations = 1)
    {
        _basePath = basePath;
        Initialize(countOfParallelSimulations);
    }

    private void Initialize(int countOfParallelSimulations)
    {
        _simulations = new SimulationObject[countOfParallelSimulations];
        
        for (int i = 0; i < countOfParallelSimulations; i++)
        {
            _simulations[i] = new(new MySimulation(), true);
            _simulations[i].Simulation.OnSimulationDidFinish(SimulationFinished);
        }
        
        _semaphoreAvailableSimulations = new SemaphoreSlim(_simulations.Length);
    }
    
    public async Task AnalyzeConfigurationsAsync(int replications)
    {
        _replications = replications;

        if (Configurations.Count == 0)
        {
            LoadConfiguration();
        }

        var tasks = new List<Task>();

        while (true)
        {
            int[] configuration;
            int configurationNumber;
            
            lock (_lockConfigurations)
            {
                if (!Configurations.TryDequeue(out configuration))
                {
                    break;
                }
                
                configurationNumber = Configurations.Count + 1;
            }
            
            // Vyžiadanie voľného objektu simulácie
            await _semaphoreAvailableSimulations.WaitAsync();
            
            MySimulation? simulation = null;
            
            // Tu sa treba zamknúť, aby dve vlákna nezískali tú istú voľnú simuláciu
            lock (_lockSimulations)
            {
                var simulationWithInfo = _simulations.FirstOrDefault(s => s.IsFree);

                if (simulationWithInfo == default)
                {
                    throw new InvalidOperationException("No available simulation instance.");
                }
                
                simulationWithInfo.IsFree = false;
                simulationWithInfo.CurrentConfigurationNumber = configurationNumber;
                simulationWithInfo.StartTime = DateTime.Now;
                simulation = simulationWithInfo.Simulation;
            }
            
            tasks.Add(Task.Run(async () =>
            {
                await AnalyzeConfigurationAsync(simulation, configuration);
            }));
        }

        await Task.WhenAll(tasks);
        
        PrintResultsToFile();
    }
    
    private async Task AnalyzeConfigurationAsync(MySimulation simulation, int[] configuration)
    {
        simulation.CountOfWorkersGroupA = configuration[0];
        simulation.CountOfWorkersGroupB = configuration[1];
        simulation.CountOfWorkersGroupC = configuration[2];
        simulation.CountOfAssemblyLines = configuration[3];

        await Task.Run(() => simulation.Simulate(_replications, 7_171_200));
    }
    
    private void SimulationFinished(OSPABA.Simulation simulation)
    {
        var mySimulation = (MySimulation)simulation;
        
        // Free the simulation object
        for (int i = 0; i < _simulations.Length; i++)
        {
            if (_simulations[i].Simulation == mySimulation)
            {
                _simulations[i].IsFree = true;
                
                SaveSimulationRunResults(_simulations[i]);
        
                System.Console.WriteLine($"Simulation finished for configuration: <{mySimulation.CountOfWorkersGroupA}, " +
                                         $"{mySimulation.CountOfWorkersGroupB}, {mySimulation.CountOfWorkersGroupC}, " +
                                         $"{mySimulation.CountOfAssemblyLines}> ProcessingOrderTime={mySimulation.AverageProcessingOrderTime.Mean / 3600.0}");
                
                // Release the semaphore
                _semaphoreAvailableSimulations.Release();
                
                break;
            }
        }
    }

    private void LoadConfiguration()
    {
        var configFilePath = Path.Combine(_basePath, "simulation.config");
        
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException("Configuration file not found.", configFilePath);
        }
        
        var lines = File.ReadAllLines(configFilePath);
        
        foreach (var line in lines)
        {
            var parts = line.Split(' ');
            if (parts.Length != 4)
            {
                throw new FormatException($"Invalid configuration format: {line}");
            }

            var a = int.Parse(parts[0]);
            var b = int.Parse(parts[1]);
            var c = int.Parse(parts[2]);
            var al = int.Parse(parts[3]);

            Configurations.Enqueue(new[] { a, b, c, al });
        }
    }

    private void PrintResultsToFile()
    {
        var csvFilePath = Path.Combine(_basePath, "SimulationResults_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv");
        
        // CSV file header
        File.WriteAllText(csvFilePath, "WorkersA;WorkersB;WorkersC;AssemblyLines;" +
                                       "ProcessingOrderTimeFormatted;ProcessingOrderTimeCIFormatted;ProcessingOrderTime;ProcessingOrderTimeCI_Lower;ProcessingOrderTimeCI_Upper;" +
                                       "ProcessingFurnitureTimeFormatted;ProcessingFurnitureTimeCIFormatted;ProcessingFurnitureTime;ProcessingFurnitureTimeCI_Lower;ProcessingFurnitureTimeCI_Upper;" +
                                       "PendingOrdersCountFormatted;PendingOrdersCountCIFormatted;PendingOrdersCount;PendingOrdersCountCI_Lower;PendingOrdersCountCI_Upper;" +
                                       "PendingItemsForLineCountFormatted;PendingItemsForLineCountCIFormatted;PendingItemsForLineCount;PendingItemsForLineCountCI_Lower;PendingItemsForLineCountCI_Upper;" +
                                       "PendingItemsForWorkerACountFormatted;PendingItemsForWorkerACountCIFormatted;PendingItemsForWorkerACount;PendingItemsForWorkerACountCI_Lower;PendingItemsForWorkerACountCI_Upper;" +
                                       "PendingItemsForWorkerBCountFormatted;PendingItemsForWorkerBCountCIFormatted;PendingItemsForWorkerBCount;PendingItemsForWorkerBCountCI_Lower;PendingItemsForWorkerBCountCI_Upper;" +
                                       "PendingItemsForWorkerCCountFormatted;PendingItemsForWorkerCCountCIFormatted;PendingItemsForWorkerCCount;PendingItemsForWorkerCCountCI_Lower;PendingItemsForWorkerCCountCI_Upper;" +
                                       "PendingItemsForWorkerMixCountFormatted;PendingItemsForWorkerMixCountCIFormatted;PendingItemsForWorkerMixCount;PendingItemsForWorkerMixCount_Lower;PendingItemsForWorkerMixCountCI_Upper;" +
                                       "PendingOrdersWaitingTimeFormatted;PendingOrdersWaitingTimeCIFormatted;PendingOrdersWaitingTime;PendingOrdersWaitingTimeCI_Lower;PendingOrdersWaitingTimeCI_Upper;" +
                                       "PendingItemsForLineWaitingTimeFormatted;PendingItemsForLineWaitingTimeCIFormatted;PendingItemsForLineWaitingTime;PendingItemsForLineWaitingTimeCI_Lower;PendingItemsForLineWaitingTimeCI_Upper;" +
                                       "PendingItemsForWorkerAWaitingTimeFormatted;PendingItemsForWorkerAWaitingTimeCIFormatted;PendingItemsForWorkerAWaitingTime;PendingItemsForWorkerAWaitingTimeCI_Lower;PendingItemsForWorkerAWaitingTimeCI_Upper;" +
                                       "PendingItemsForWorkerBWaitingTimeFormatted;PendingItemsForWorkerBWaitingTimeCIFormatted;PendingItemsForWorkerBWaitingTime;PendingItemsForWorkerBWaitingTimeCI_Lower;PendingItemsForWorkerBWaitingTimeCI_Upper;" +
                                       "PendingItemsForWorkerCWaitingTimeFormatted;PendingItemsForWorkerCWaitingTimeCIFormatted;PendingItemsForWorkerCWaitingTime;PendingItemsForWorkerCWaitingTimeCI_Lower;PendingItemsForWorkerCWaitingTimeCI_Upper;" +
                                       "PendingItemsForWorkerMixWaitingTimeFormatted;PendingItemsForWorkerMixWaitingTimeCIFormatted;PendingItemsForWorkerMixWaitingTime;PendingItemsForWorkerMixWaitingTimeCI_Lower;PendingItemsForWorkerMixWaitingTimeCI_Upper;" +
                                       "AssemblyLinesUtilizationFormatted;AssemblyLinesUtilizationCIFormatted;AssemblyLinesUtilization;AssemblyLinesUtilizationCI_Lower;AssemblyLinesUtilizationCI_Upper;" +
                                       "WorkersGroupAUtilizationFormatted;WorkersGroupAUtilizationCIFormatted;WorkersGroupAUtilization;WorkersGroupAUtilizationCI_Lower;WorkersGroupAUtilizationCI_Upper;" +
                                       "WorkersGroupBUtilizationFormatted;WorkersGroupBUtilizationCIFormatted;WorkersGroupBUtilization;WorkersGroupBUtilizationCI_Lower;WorkersGroupBUtilizationCI_Upper;" +
                                       "WorkersGroupCUtilizationFormatted;WorkersGroupCUtilizationCIFormatted;WorkersGroupCUtilization;WorkersGroupCUtilizationCI_Lower;WorkersGroupCUtilizationCI_Upper;" +
                                       "DurationOfSimulation\n");
        
        // Order results by configuration number descending
        Results = Results.OrderByDescending(r => r.ConfigurationNumber).ToList();
        
        foreach (var result in Results)
        {
            File.AppendAllText(csvFilePath, result.Result);
        }
    }
    
    private void SaveSimulationRunResults(SimulationObject simulationObject)
    {
        var mySimulation = simulationObject.Simulation;

        var averageProcessingOrderTime = mySimulation.AverageProcessingOrderTime.Mean / 3600.0;
        var averageProcessingOrderTimeFormatted = Math.Round(averageProcessingOrderTime, 2).ToString("F2");
        var averageProcessingOrderTimeCI = mySimulation.AverageProcessingOrderTime.ConfidenceInterval95().Divide(3600.0);
        var averageProcessingOrderTimeCIFormatted = $"\"<{Math.Round(averageProcessingOrderTimeCI.Item1, 2)} ; {Math.Round(averageProcessingOrderTimeCI.Item2, 2)}>\"";
        var averageProcessingFurnitureTime = mySimulation.AverageProcessingFurnitureTime.Mean / 3600.0;
        var averageProcessingFurnitureTimeFormatted = Math.Round(averageProcessingFurnitureTime, 2).ToString("F2");
        var averageProcessingFurnitureTimeCI = mySimulation.AverageProcessingFurnitureTime.ConfidenceInterval95().Divide(3600.0);
        var averageProcessingFurnitureTimeCIFormatted = $"\"<{Math.Round(averageProcessingFurnitureTimeCI.Item1, 2)} ; {Math.Round(averageProcessingFurnitureTimeCI.Item2, 2)}>\"";
        
        var averagePendingOrdersCount = mySimulation.AveragePendingOrdersCount.Mean;
        var averagePendingOrdersCountFormatted = Math.Round(averagePendingOrdersCount, 2).ToString("F2");
        var averagePendingOrdersCountCI = mySimulation.AveragePendingOrdersCount.ConfidenceInterval95();
        var averagePendingOrdersCountCIFormatted = $"\"<{Math.Round(averagePendingOrdersCountCI.Item1, 2)} ; {Math.Round(averagePendingOrdersCountCI.Item2, 2)}>\"";
        var averagePendingItemsForLineCount = mySimulation.AveragePendingItemsForLineCount.Mean;
        var averagePendingItemsForLineCountFormatted = Math.Round(averagePendingItemsForLineCount, 2).ToString("F2");
        var averagePendingItemsForLineCountCI = mySimulation.AveragePendingItemsForLineCount.ConfidenceInterval95();
        var averagePendingItemsForLineCountCIFormatted = $"\"<{Math.Round(averagePendingItemsForLineCountCI.Item1, 2)} ; {Math.Round(averagePendingItemsForLineCountCI.Item2, 2)}>\"";
        var averagePendingItemsForWorkerACount = mySimulation.AveragePendingItemsForWorkerACount.Mean;
        var averagePendingItemsForWorkerACountFormatted = Math.Round(averagePendingItemsForWorkerACount, 2).ToString("F2");
        var averagePendingItemsForWorkerACountCI = mySimulation.AveragePendingItemsForWorkerACount.ConfidenceInterval95();
        var averagePendingItemsForWorkerACountCIFormatted = $"\"<{Math.Round(averagePendingItemsForWorkerACountCI.Item1, 2)} ; {Math.Round(averagePendingItemsForWorkerACountCI.Item2, 2)}>\"";
        var averagePendingItemsForWorkerBCount = mySimulation.AveragePendingItemsForWorkerBCount.Mean;
        var averagePendingItemsForWorkerBCountFormatted = Math.Round(averagePendingItemsForWorkerBCount, 2).ToString("F2");
        var averagePendingItemsForWorkerBCountCI = mySimulation.AveragePendingItemsForWorkerBCount.ConfidenceInterval95();
        var averagePendingItemsForWorkerBCountCIFormatted = $"\"<{Math.Round(averagePendingItemsForWorkerBCountCI.Item1, 2)} ; {Math.Round(averagePendingItemsForWorkerBCountCI.Item2, 2)}>\"";
        var averagePendingItemsForWorkerCCount = mySimulation.AveragePendingItemsForWorkerCCount.Mean;
        var averagePendingItemsForWorkerCCountFormatted = Math.Round(averagePendingItemsForWorkerCCount, 2).ToString("F2");
        var averagePendingItemsForWorkerCCountCI = mySimulation.AveragePendingItemsForWorkerCCount.ConfidenceInterval95();
        var averagePendingItemsForWorkerCCountCIFormatted = $"\"<{Math.Round(averagePendingItemsForWorkerCCountCI.Item1, 2)} ; {Math.Round(averagePendingItemsForWorkerCCountCI.Item2, 2)}>\"";
        var averagePendingItemsForWorkerMixCount = mySimulation.AveragePendingItemsForWorkerMixCount.Mean;
        var averagePendingItemsForWorkerMixCountFormatted = Math.Round(averagePendingItemsForWorkerMixCount, 2).ToString("F2");
        var averagePendingItemsForWorkerMixCountCI = mySimulation.AveragePendingItemsForWorkerMixCount.ConfidenceInterval95();
        var averagePendingItemsForWorkerMixCountCIFormatted = $"\"<{Math.Round(averagePendingItemsForWorkerMixCountCI.Item1, 2)} ; {Math.Round(averagePendingItemsForWorkerMixCountCI.Item2, 2)}>\"";
        
        var averagePendingOrdersWaitingTime = mySimulation.AveragePendingOrdersWaitingTime.Mean / 3600.0;
        var averagePendingOrdersWaitingTimeFormatted = Math.Round(averagePendingOrdersWaitingTime, 2).ToString("F2");
        var averagePendingOrdersWaitingTimeCI = mySimulation.AveragePendingOrdersWaitingTime.ConfidenceInterval95().Divide(3600.0);
        var averagePendingOrdersWaitingTimeCIFormatted = $"\"<{Math.Round(averagePendingOrdersWaitingTimeCI.Item1, 2)} ; {Math.Round(averagePendingOrdersWaitingTimeCI.Item2, 2)}>\"";
        var averagePendingItemsForLineWaitingTime = mySimulation.AveragePendingItemsForLineWaitingTime.Mean / 3600.0;
        var averagePendingItemsForLineWaitingTimeFormatted = Math.Round(averagePendingItemsForLineWaitingTime, 2).ToString("F2");
        var averagePendingItemsForLineWaitingTimeCI =  mySimulation.AveragePendingItemsForLineWaitingTime.ConfidenceInterval95().Divide(3600.0);
        var averagePendingItemsForLineWaitingTimeCIFormatted = $"\"<{Math.Round(averagePendingItemsForLineWaitingTimeCI.Item1, 2)} ; {Math.Round(averagePendingItemsForLineWaitingTimeCI.Item2, 2)}>\"";
        var averagePendingItemsForWorkerAWaitingTime = mySimulation.AveragePendingItemsForWorkerAWaitingTime.Mean / 3600.0;
        var averagePendingItemsForWorkerAWaitingTimeFormatted = Math.Round(averagePendingItemsForWorkerAWaitingTime, 2).ToString("F2");
        var averagePendingItemsForWorkerAWaitingTimeCI = mySimulation.AveragePendingItemsForWorkerAWaitingTime.ConfidenceInterval95().Divide(3600.0);
        var averagePendingItemsForWorkerAWaitingTimeCIFormatted = $"\"<{Math.Round(averagePendingItemsForWorkerAWaitingTimeCI.Item1, 2)} ; {Math.Round(averagePendingItemsForWorkerAWaitingTimeCI.Item2, 2)}>\"";
        var averagePendingItemsForWorkerBWaitingTime = mySimulation.AveragePendingItemsForWorkerBWaitingTime.Mean / 3600.0;
        var averagePendingItemsForWorkerBWaitingTimeFormatted = Math.Round(averagePendingItemsForWorkerBWaitingTime, 2).ToString("F2");
        var averagePendingItemsForWorkerBWaitingTimeCI = mySimulation.AveragePendingItemsForWorkerBWaitingTime.ConfidenceInterval95().Divide(3600.0);
        var averagePendingItemsForWorkerBWaitingTimeCIFormatted = $"\"<{Math.Round(averagePendingItemsForWorkerBWaitingTimeCI.Item1, 2)} ; {Math.Round(averagePendingItemsForWorkerBWaitingTimeCI.Item2, 2)}>\"";
        var averagePendingItemsForWorkerCWaitingTime = mySimulation.AveragePendingItemsForWorkerCWaitingTime.Mean / 3600.0;
        var averagePendingItemsForWorkerCWaitingTimeFormatted = Math.Round(averagePendingItemsForWorkerCWaitingTime, 2).ToString("F2");
        var averagePendingItemsForWorkerCWaitingTimeCI = mySimulation.AveragePendingItemsForWorkerCWaitingTime.ConfidenceInterval95().Divide(3600.0);
        var averagePendingItemsForWorkerCWaitingTimeCIFormatted = $"\"<{Math.Round(averagePendingItemsForWorkerCWaitingTimeCI.Item1, 2)} ; {Math.Round(averagePendingItemsForWorkerCWaitingTimeCI.Item2, 2)}>\"";
        var averagePendingItemsForWorkerMixWaitingTime = mySimulation.AveragePendingItemsForWorkerMixWaitingTime.Mean / 3600.0;
        var averagePendingItemsForWorkerMixWaitingTimeFormatted = Math.Round(averagePendingItemsForWorkerMixWaitingTime, 2).ToString("F2");
        var averagePendingItemsForWorkerMixWaitingTimeCI = mySimulation.AveragePendingItemsForWorkerMixWaitingTime.ConfidenceInterval95().Divide(3600.0);
        var averagePendingItemsForWorkerMixWaitingTimeCIFormatted = $"\"<{Math.Round(averagePendingItemsForWorkerMixWaitingTimeCI.Item1, 2)} ; {Math.Round(averagePendingItemsForWorkerMixWaitingTimeCI.Item2, 2)}>\"";
        
        var averageAssemblyLinesUtilization = mySimulation.AverageAssemblyLinesUtilization.Mean;
        var averageAssemblyLinesUtilizationFormatted = Math.Round(averageAssemblyLinesUtilization, 2).ToString("F2");
        var averageAssemblyLinesUtilizationCI = mySimulation.AverageAssemblyLinesUtilization.ConfidenceInterval95();
        var averageAssemblyLinesUtilizationCIFormatted = $"\"<{Math.Round(averageAssemblyLinesUtilizationCI.Item1, 2)} ; {Math.Round(averageAssemblyLinesUtilizationCI.Item2, 2)}>\"";
        
        var averageWorkersGroupAUtilization = mySimulation.AverageWorkersGroupAUtilization.Mean;
        var averageWorkersGroupAUtilizationFormatted = Math.Round(averageWorkersGroupAUtilization, 2).ToString("F2");
        var averageWorkersGroupAUtilizationCI = mySimulation.AverageWorkersGroupAUtilization.ConfidenceInterval95();
        var averageWorkersGroupAUtilizationCIFormatted = $"\"<{Math.Round(averageWorkersGroupAUtilizationCI.Item1, 2)} ; {Math.Round(averageWorkersGroupAUtilizationCI.Item2, 2)}>\"";
        var averageWorkersGroupBUtilization = mySimulation.AverageWorkersGroupBUtilization.Mean;
        var averageWorkersGroupBUtilizationFormatted = Math.Round(averageWorkersGroupBUtilization, 2).ToString("F2");
        var averageWorkersGroupBUtilizationCI = mySimulation.AverageWorkersGroupBUtilization.ConfidenceInterval95();
        var averageWorkersGroupBUtilizationCIFormatted = $"\"<{Math.Round(averageWorkersGroupBUtilizationCI.Item1, 2)} ; {Math.Round(averageWorkersGroupBUtilizationCI.Item2, 2)}>\"";
        var averageWorkersGroupCUtilization = mySimulation.AverageWorkersGroupCUtilization.Mean;
        var averageWorkersGroupCUtilizationFormatted = Math.Round(averageWorkersGroupCUtilization, 2).ToString("F2");
        var averageWorkersGroupCUtilizationCI = mySimulation.AverageWorkersGroupCUtilization.ConfidenceInterval95();
        var averageWorkersGroupCUtilizationCIFormatted = $"\"<{Math.Round(averageWorkersGroupCUtilizationCI.Item1, 2)} ; {Math.Round(averageWorkersGroupCUtilizationCI.Item2, 2)}>\"";

        var durationOfSimulation = (DateTime.Now - simulationObject.StartTime).TotalSeconds;

        var resultItem = $"{mySimulation.CountOfWorkersGroupA};{mySimulation.CountOfWorkersGroupB};{mySimulation.CountOfWorkersGroupC};{mySimulation.CountOfAssemblyLines};" +
                         $"{averageProcessingOrderTimeFormatted};{averageProcessingOrderTimeCIFormatted};{averageProcessingOrderTime};{averageProcessingOrderTimeCI.Item1};{averageProcessingOrderTimeCI.Item2};" +
                         $"{averageProcessingFurnitureTimeFormatted};{averageProcessingFurnitureTimeCIFormatted};{averageProcessingFurnitureTime};{averageProcessingFurnitureTimeCI.Item1};{averageProcessingFurnitureTimeCI.Item2};" +
                         $"{averagePendingOrdersCountFormatted};{averagePendingOrdersCountCIFormatted};{averagePendingOrdersCount};{averagePendingOrdersCountCI.Item1};{averagePendingOrdersCountCI.Item2};" +
                         $"{averagePendingItemsForLineCountFormatted};{averagePendingItemsForLineCountCIFormatted};{averagePendingItemsForLineCount};{averagePendingItemsForLineCountCI.Item1};{averagePendingItemsForLineCountCI.Item2};" +
                         $"{averagePendingItemsForWorkerACountFormatted};{averagePendingItemsForWorkerACountCIFormatted};{averagePendingItemsForWorkerACount};{averagePendingItemsForWorkerACountCI.Item1};{averagePendingItemsForWorkerACountCI.Item2};" +
                         $"{averagePendingItemsForWorkerBCountFormatted};{averagePendingItemsForWorkerBCountCIFormatted};{averagePendingItemsForWorkerBCount};{averagePendingItemsForWorkerBCountCI.Item1};{averagePendingItemsForWorkerBCountCI.Item2};" +
                         $"{averagePendingItemsForWorkerCCountFormatted};{averagePendingItemsForWorkerCCountCIFormatted};{averagePendingItemsForWorkerCCount};{averagePendingItemsForWorkerCCountCI.Item1};{averagePendingItemsForWorkerCCountCI.Item2};" +
                         $"{averagePendingItemsForWorkerMixCountFormatted};{averagePendingItemsForWorkerMixCountCIFormatted};{averagePendingItemsForWorkerMixCount};{averagePendingItemsForWorkerMixCountCI.Item1};{averagePendingItemsForWorkerMixCountCI.Item2};" +
                         $"{averagePendingOrdersWaitingTimeFormatted};{averagePendingOrdersWaitingTimeCIFormatted};{averagePendingOrdersWaitingTime};{averagePendingOrdersWaitingTimeCI.Item1};{averagePendingOrdersWaitingTimeCI.Item2};" +
                         $"{averagePendingItemsForLineWaitingTimeFormatted};{averagePendingItemsForLineWaitingTimeCIFormatted};{averagePendingItemsForLineWaitingTime};{averagePendingItemsForLineWaitingTimeCI.Item1};{averagePendingItemsForLineWaitingTimeCI.Item2};" +
                         $"{averagePendingItemsForWorkerAWaitingTimeFormatted};{averagePendingItemsForWorkerAWaitingTimeCIFormatted};{averagePendingItemsForWorkerAWaitingTime};{averagePendingItemsForWorkerAWaitingTimeCI.Item1};{averagePendingItemsForWorkerAWaitingTimeCI.Item2};" +
                         $"{averagePendingItemsForWorkerBWaitingTimeFormatted};{averagePendingItemsForWorkerBWaitingTimeCIFormatted};{averagePendingItemsForWorkerBWaitingTime};{averagePendingItemsForWorkerBWaitingTimeCI.Item1};{averagePendingItemsForWorkerBWaitingTimeCI.Item2};" +
                         $"{averagePendingItemsForWorkerCWaitingTimeFormatted};{averagePendingItemsForWorkerCWaitingTimeCIFormatted};{averagePendingItemsForWorkerCWaitingTime};{averagePendingItemsForWorkerCWaitingTimeCI.Item1};{averagePendingItemsForWorkerCWaitingTimeCI.Item2};" +
                         $"{averagePendingItemsForWorkerMixWaitingTimeFormatted};{averagePendingItemsForWorkerMixWaitingTimeCIFormatted};{averagePendingItemsForWorkerMixWaitingTime};{averagePendingItemsForWorkerMixWaitingTimeCI.Item1};{averagePendingItemsForWorkerMixWaitingTimeCI.Item2};" +
                         $"{averageAssemblyLinesUtilizationFormatted};{averageAssemblyLinesUtilizationCIFormatted};{averageAssemblyLinesUtilization};{averageAssemblyLinesUtilizationCI.Item1};{averageAssemblyLinesUtilizationCI.Item2};" +
                         $"{averageWorkersGroupAUtilizationFormatted};{averageWorkersGroupAUtilizationCIFormatted};{averageWorkersGroupAUtilization};{averageWorkersGroupAUtilizationCI.Item1};{averageWorkersGroupAUtilizationCI.Item2};" +
                         $"{averageWorkersGroupBUtilizationFormatted};{averageWorkersGroupBUtilizationCIFormatted};{averageWorkersGroupBUtilization};{averageWorkersGroupBUtilizationCI.Item1};{averageWorkersGroupBUtilizationCI.Item2};" +
                         $"{averageWorkersGroupCUtilizationFormatted};{averageWorkersGroupCUtilizationCIFormatted};{averageWorkersGroupCUtilization};{averageWorkersGroupCUtilizationCI.Item1};{averageWorkersGroupCUtilizationCI.Item2};" +
                         $"{durationOfSimulation}\n";
        
        Results.Add(new(
            resultItem,
            simulationObject.CurrentConfigurationNumber,
            [
                mySimulation.CountOfWorkersGroupA,
                mySimulation.CountOfWorkersGroupB,
                mySimulation.CountOfWorkersGroupC,
                mySimulation.CountOfAssemblyLines
        ]));
    }
}

public class SimulationObject
{
    public MySimulation Simulation { get; set; }
    
    public int CurrentConfigurationNumber { get; set; }
    
    public bool IsFree { get; set; }
    
    public DateTime StartTime { get; set; }

    public SimulationObject(MySimulation simulation, bool isFree)
    {
        Simulation = simulation;
        IsFree = isFree;
    }
}

public class ResultObject
{
    public string Result { get; set; }
    
    public int[] Configuration { get; set; }
    
    public int ConfigurationNumber { get; set; }
    
    public ResultObject(string result, int configurationNumber, int[] configuration)
    {
        Result = result;
        ConfigurationNumber = configurationNumber;
        Configuration = configuration;
    }
}

public static class DoubleTupleExtensions
{
    public static (double, double) Divide(this (double, double) tuple, double divider)
    {
        return (tuple.Item1 / divider, tuple.Item2 / divider);
    }
}
