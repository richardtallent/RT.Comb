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
		private readonly IConfig defaultConfig = DefaultConfig.Instance;

		public BenchmarkConfig() {
			AddColumnProvider(DefaultColumnProviders.Instance)
				.AddExporter(AsciiDocExporter.Default)
				.AddLogger(ConsoleLogger.Default)
				.AddAnalyser(defaultConfig.GetAnalysers().ToArray())
				.AddValidator(defaultConfig.GetValidators().ToArray())
				.WithSummaryStyle(SummaryStyle.Default)
				.WithArtifactsPath(GetArtifactsPath())
				.WithOption(ConfigOptions.DisableLogFile, true)
				.WithOption(ConfigOptions.DisableOptimizationsValidator, true);

			var baseJob = Job.Default;
			AddJob(
				baseJob.WithNuGet("RT.Comb", "2.5.0").WithId("2.5.0"),
				baseJob.WithNuGet("RT.Comb", "3.0.0").WithId("3.0.0"));
		}

		private static string GetArtifactsPath() {
			return Path.Combine(RuntimeContext.RootDirectory, "docs", "benchmarks");
		}
	}

}