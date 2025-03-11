using System;
using BenchmarkDotNet.Attributes;

namespace RT.Comb.Benchmark.Jobs {

	[AsciiDocExporter]
	[MemoryDiagnoser]
	public class SqlCombProviderJob {
		private readonly SqlCombProvider _provider = new(new UnixDateTimeStrategy());
		private readonly DateTime _baseDateTime = DateTime.UtcNow;
		private readonly Guid _baseGuid = Guid.NewGuid();

		[Benchmark]
		public Guid CreateNew() => _provider.Create();

		[Benchmark]
		public Guid CreateBasedOnDateTime() => _provider.Create(_baseDateTime);

		[Benchmark]
		public Guid CreateBasedOnGuid() => _provider.Create(_baseGuid);

		[Benchmark]
		public Guid CreateBasedOnGuidAndDateTime() => _provider.Create(_baseGuid, _baseDateTime);
	}

}