using System;
using RT.Comb;
/*
	Copyright 2015-2025 Richard S. Tallent, II

	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
	(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
	publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to
	do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
	LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
	CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

// ******************************************************************************
// This is an example console application, showing a minimal-code use of the full
// COMB provider API. Run it with no arguments and it will generate a COMB GUID
// for you. Pass it a COMB GUID and it will return the timestamp. Keep in mind
// that the default timestamp provider uses UST.
// ******************************************************************************

enum ExitCode : int {
	Success = 0,
	NotAGuid = 1
}

class CombMe {

	public static int Main(string[] args) {

		if (args.Length == 0) {
			Console.WriteLine(Provider.Sql.Create());
			return (int)ExitCode.Success;
		}

		if (!Guid.TryParse(args[0], out Guid g)) {
			Console.WriteLine("Not a GUID.");
			return (int)ExitCode.NotAGuid;
		}
		Console.WriteLine(Provider.Sql.GetTimestamp(g));
		return (int)ExitCode.Success;

	}

}