using BenchmarkDotNet.Running;

namespace RT.Comb.Benchmark {

	public static class Program {
		public static void Main(string[] args) => BenchmarkSwitcher
				.FromAssembly(typeof(Program).Assembly)
				.Run(args, new BenchmarkConfig());
	}

}