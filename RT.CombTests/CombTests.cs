using System;
using Xunit;
using RT.CombUtils;

namespace RT.CombTests {
	// This project can output the Class library as a NuGet Package.
	// To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
	public class CombTests
    {

        [Fact]
        public void EmptyComb()
        {
			var d = new DateTime(0);
			var g = Guid.Empty;
			var c = Comb.NewComb(g, d);
            Assert.Equal("0000-0000-0000-00000000", c.ToString("D"));
        }

        [Fact]
        public void BadComb()
        {
            Assert.Equal(false, true);
        }

    }
}
