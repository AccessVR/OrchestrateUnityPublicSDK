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

		public static string Before(string content, string separator)
		{
			return content.Substring(0, content.IndexOf(separator));
		}
		
		public static string BeforeLast(string content, string separator)
		{
			return content.Substring(0, content.LastIndexOf(separator));
		}

		public static string After(string content, string separator)
		{
			return content.Substring(content.IndexOf(separator) + separator.Length);
		}
		
		public static string AfterLast(string content, string separator)
		{
			return content.Substring(content.LastIndexOf(separator) + separator.Length);
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
		
		/// <summary>
		/// Parses author-supplied CSS color strings: #RGB, #RGBA, #RRGGBB,
		/// #RRGGBBAA, rgb(), rgba(). Returns null (never transparent black)
		/// for anything unparseable, so callers can distinguish "no color"
		/// from "black". Alpha is preserved — it is load-bearing (e.g. the
		/// hidden-hotspot acknowledged disc derives its translucency from
		/// the background color's alpha channel).
		/// </summary>
		public static Color? ConvertToColor(string color)
        {
	        if (String.IsNullOrEmpty(color))
	        {
		        return null;
	        }

	        color = color.Trim();

	        if (color.StartsWith("#"))
            {
                // The failure path must return null, not default(Color):
                // transparent black is a valid-looking color and silently
                // wrong everywhere it is used.
                return ColorUtility.TryParseHtmlString(color, out Color parsedColor)
                    ? parsedColor
                    : (Color?) null;
            }
            
            if (color.StartsWith("rgb"))
            {
                try
                {
                    string[] values = color.Replace("rgba(", "").Replace("rgb(", "").Replace(")", "").Split(',');
                    if (values.Length >= 3)
                    {
                        // InvariantCulture: float.Parse under a comma-decimal
                        // locale would misread "0.5".
                        float r = ParseComponent(values[0]) / 255f;
                        float g = ParseComponent(values[1]) / 255f;
                        float b = ParseComponent(values[2]) / 255f;
                        // CSS alpha is 0-1; tolerate a 0-255 alpha defensively.
                        float a = 1f;
                        if (values.Length > 3)
                        {
                            a = ParseComponent(values[3]);
                            if (a > 1f)
                            {
                                a /= 255f;
                            }
                        }
                        return new Color(
                            Mathf.Clamp01(r),
                            Mathf.Clamp01(g),
                            Mathf.Clamp01(b),
                            Mathf.Clamp01(a));
                    }
                }
                catch (FormatException)
                {
                    return null;
                }
            }
            
            return null;
        }

		private static float ParseComponent(string value)
		{
			return float.Parse(value.Trim(), System.Globalization.CultureInfo.InvariantCulture);
		}

	}

}
