using System.IO;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

namespace RT.Comb.Benchmark {

	internal class BenchmarkConfig : ManualConfig {
		private readonly IConfig _defaultConfig = DefaultConfig.Instance;

		public BenchmarkConfig() {
			AddColumnProvider(DefaultColumnProviders.Instance)
				.AddExporter(AsciiDocExporter.Default)
				.AddLogger(ConsoleLogger.Default)
				.AddAnalyser(_defaultConfig.GetAnalysers().ToArray())
				.AddValidator(_defaultConfig.GetValidators().ToArray())
				.WithSummaryStyle(SummaryStyle.Default)
				.WithArtifactsPath(GetArtifactsPath())
				.WithOption(ConfigOptions.DisableLogFile, true)
				.WithOption(ConfigOptions.DisableOptimizationsValidator, true);

			var baseJob = Job.Default;
			AddJob(
				// baseJob.WithNuGet("RT.Comb", "3.0.0").WithId("3.0.0"),
				baseJob.WithNuGet("RT.Comb", "4.0.2").WithId("4.0.2"));
		}

		private static string GetArtifactsPath() => Path.Combine(RuntimeContext.RootDirectory, "docs", "benchmarks");
	}

}