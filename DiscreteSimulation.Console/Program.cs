using DiscreteSimulation.Console;

var analyzer = new ConfigurationAnalyzer(4);

await analyzer.AnalyzeConfigurationsAsync(1000);
