using System.IO;
using System.Linq;
using System.Reflection;

namespace RT.Comb.Benchmark
{
    internal static class RuntimeContext
    {
        static RuntimeContext()
        {
            var assemblyFolder = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            for (var currDir = assemblyFolder; currDir != null; currDir = currDir.Parent)
            {
                if (!currDir.EnumerateFiles(".gitignore", SearchOption.TopDirectoryOnly).Any())
                    continue;

                RootDirectory = currDir.FullName;
                break;
            }
        }

        public static string RootDirectory { get; } = string.Empty;
    }
}