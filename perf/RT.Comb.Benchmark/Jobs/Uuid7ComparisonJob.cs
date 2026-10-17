#if !RTCOMB_NUGET
using System;
using BenchmarkDotNet.Attributes;

namespace RT.Comb.Benchmark.Jobs {

	/// <summary>
	/// Compares Uuid7CombProvider with the framework's Guid.CreateVersion7() (.NET 9+), with Guid.NewGuid() as the baseline.
	/// </summary>
	[AsciiDocExporter]
	[MemoryDiagnoser]
	public class Uuid7ComparisonJob {
		private readonly Uuid7CombProvider _provider = new();
		private readonly DateTime _baseDateTime = DateTime.UtcNow;
		private readonly DateTimeOffset _baseDateTimeOffset = DateTimeOffset.UtcNow;

		[Benchmark(Baseline = true)]
		public Guid NewGuidV4() => Guid.NewGuid();

		[Benchmark]
		public Guid CombUuid7CreateNew() => _provider.Create();

		[Benchmark]
		public Guid FrameworkCreateVersion7() => Guid.CreateVersion7();

		[Benchmark]
		public Guid CombUuid7CreateBasedOnDateTime() => _provider.Create(_baseDateTime);

		[Benchmark]
		public Guid FrameworkCreateVersion7WithTimestamp() => Guid.CreateVersion7(_baseDateTimeOffset);
	}

}
#endif
