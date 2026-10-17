using System;
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

			// ShortRun keeps this to a minute or two; the operations are tiny and stable.
			var baseJob = Job.ShortRun;
			AddJob(baseJob.WithId("local"));

			// Optionally also run against a published package, e.g. RTCOMB_BASELINE=4.0.2. Exclude newer types
			// (Uuid7ComparisonJob) from such runs with a filter such as --filter "*CombProviderJob*".
			var baseline = Environment.GetEnvironmentVariable("RTCOMB_BASELINE");
			if (!string.IsNullOrWhiteSpace(baseline)) {
				AddJob(baseJob.WithMsBuildArguments($"/p:RTCombVersion={baseline}").WithId(baseline));
			}
		}

		private static string GetArtifactsPath() => Path.Combine(RuntimeContext.RootDirectory, "docs", "benchmarks");
	}

}