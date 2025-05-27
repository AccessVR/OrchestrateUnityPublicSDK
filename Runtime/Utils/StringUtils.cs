using System;

namespace AccessVR.OrchestrateVR.SDK
{

	public static class StringUtils
	{
		public static string ReplaceSpaces(string content)
		{
			return content.Replace(" ", "-").Replace("%20", "-");
		}

		public static bool AssertNotNullOrEmpty(string content)
		{
			if (String.IsNullOrEmpty(content))
			{
				throw new Exception("String cannot be null or empty");
			}

			return true;
		}

	}

}
