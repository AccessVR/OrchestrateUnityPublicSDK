using System;
using UnityEngine;

namespace AccessVR.OrchestrateVR.SDK
{

	public static class StringUtils
	{
		public static string ReplaceSpaces(string content)
		{
			return content.Replace(" ", "-").Replace("%20", "-");
		}

		public static string MakeDirectorySeparatorsConsistent(string path)
		{
			// Windows doesn't like backslashes, even though Path.DirectorySeparatorChar is a backslash...
			return path.Replace("\\", "/");
		}
		
		public static string AssertNotNullOrEmpty(string content)
		{
			if (String.IsNullOrEmpty(content))
			{
				throw new Exception("String cannot be null or empty");
			}
			return content;
		}
		
		public static Color? ConvertToColor(string color)
        {
	        if (String.IsNullOrEmpty(color))
	        {
		        return null;
	        }

	        if (color.StartsWith("#"))
            {
                ColorUtility.TryParseHtmlString(color, out Color parsedColor);
                return parsedColor;
            }
            
            if (color.StartsWith("rgb"))
            {
                string[] values = color.Replace("rgba(", "").Replace("rgb(", "").Replace(")", "").Split(',');
                if (values.Length >= 3)
                {
                    float r = float.Parse(values[0]) / 255f;
                    float g = float.Parse(values[1]) / 255f;
                    float b = float.Parse(values[2]) / 255f;
                    float a = values.Length > 3 ? float.Parse(values[3]) : 1f;
                    return new Color(r, g, b, a);
                }
            }
            
            return null;
        }

	}

}
