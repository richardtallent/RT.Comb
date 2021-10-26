using System;
using BenchmarkDotNet.Attributes;

namespace RT.Comb.Benchmark.Jobs {

	[AsciiDocExporter]
	[MemoryDiagnoser]
	public class PostgreSqlCombProviderJob {
		private PostgreSqlCombProvider provider = new(new UnixDateTimeStrategy());
		private DateTime baseDateTime = DateTime.UtcNow;
		private Guid baseGuid = Guid.NewGuid();

		[Benchmark]
		public Guid CreateNew() {
			return provider.Create();
		}

		[Benchmark]
		public Guid CreateBasedOnDateTime() {
			return provider.Create(baseDateTime);
		}

		[Benchmark]
		public Guid CreateBasedOnGuid() {
			return provider.Create(baseGuid);
		}

		[Benchmark]
		public Guid CreateBasedOnGuidAndDateTime() {
			return provider.Create(baseGuid, baseDateTime);
		}
	}

}