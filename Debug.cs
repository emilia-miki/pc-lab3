using System.Diagnostics;

class Debug
{
	[Conditional("DEBUG")]
	public static void WriteLine(string str)
	{
		Console.WriteLine(str);
	}
}
