using System.IO;

public static class StringFormattingFunctions
{
	public static string ReplaceSpaces(string inString)
	{
		return inString.Replace(" ", "-").Replace("%20", "-");
	}
	
}
