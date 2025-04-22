using DiscreteSimulation.Console;

var analyzer = new ConfigurationAnalyzer(8);

await analyzer.AnalyzeConfigurationsAsync(100);
