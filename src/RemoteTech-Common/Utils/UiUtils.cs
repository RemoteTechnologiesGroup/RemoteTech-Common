using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RemoteTech.Common.Utils
{
    public static class UiUtils
    {
        /// <summary>
        ///     Load a texture by its filename (without extension).
        /// </summary>
        /// <param name="fileName">The file name of the texture (without file extension!)</param>
        /// <returns>The <see cref="Texture2D" /> object if the texture file is found, otherwise a black texture.</returns>
        public static Texture2D LoadTexture(string fileName)
        {
            var caller = Assembly.GetCallingAssembly();
            var textureDirectory = PathUtils.TextureDirectory(caller);
            if (string.IsNullOrEmpty(textureDirectory))
                return Texture2D.blackTexture;

            var textureFileName = PathUtils.CanonicalizePathToKspPath(Path.Combine(textureDirectory, fileName));
            if (GameDatabase.Instance.ExistsTexture(textureFileName))
                return GameDatabase.Instance.GetTexture(textureFileName, false);

            Logging.Error($"Cannot Find Texture: {textureFileName}");
            return Texture2D.blackTexture;
        }

        /// <summary>
        /// Cursor detection within the given window
        /// </summary>
        public static bool ContainsMouse(Rect window)
        {
            return window.Contains(new Vector2(Input.mousePosition.x,
                Screen.height - Input.mousePosition.y));
        }

        /// <summary>
        /// Round a given number to nearest metric factor
        /// </summary>
        public static string RoundToNearestMetricFactor(double number, int decimalPlaces = 0)
        {
            string formatStr = "{0:0}";
            if (decimalPlaces >= 1)
            {
                formatStr = "{0:0.";
                for (int i = 0; i < decimalPlaces; i++)
                    formatStr += "0";
                formatStr += "}";
            }

            if (number > Math.Pow(10, 9))
                return string.Format(formatStr + " G", number / Math.Pow(10, 9));
            else if (number > Math.Pow(10, 6))
                return string.Format(formatStr + " M", number / Math.Pow(10, 6));
            else if (number > Math.Pow(10, 3))
                return string.Format(formatStr + " k", number / Math.Pow(10, 3));
            else
                return string.Format(formatStr, number);
        }

        /// <summary>
        /// Convert the color to hex string (#RRGGBB)
        /// </summary>
        //http://answers.unity3d.com/questions/1102232/how-to-get-the-color-code-in-rgb-hex-from-rgba-uni.html
        public static string colorToHex(Color thisColor) { return string.Format("#{0:X2}{1:X2}{2:X2}", toByte(thisColor.r), toByte(thisColor.g), toByte(thisColor.b)); }
        private static byte toByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        /// <summary>
        /// Create new texture and fill up with given color
        /// </summary>
        //https://forum.unity3d.com/threads/best-easiest-way-to-change-color-of-certain-pixels-in-a-single-sprite.223030/
        public static Texture2D createAndColorize(int width, int height, Color thisColor)
        {
            Texture2D newTexture = new Texture2D(width, height);
            newTexture.filterMode = FilterMode.Point;
            newTexture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    newTexture.SetPixel(x, y, thisColor);
                }
            }

            newTexture.Apply();
            return newTexture;
        }

        /// <summary>
        /// Fill the existing texture with the given color
        /// </summary>
        public static void colorize(Texture2D existingTexture, Color thisColor)
        {
            for (int y = 0; y < existingTexture.height; y++)
            {
                for (int x = 0; x < existingTexture.width; x++)
                {
                    existingTexture.SetPixel(x, y, thisColor);
                }
            }

            existingTexture.Apply();
        }

        /// <summary>
        /// Overlay two base and topmost textures to create a new texture
        /// </summary>
        public static Texture2D createAndOverlay(Texture2D baseTexture, Texture2D frontTexture)
        {
            Texture2D newTexture = new Texture2D(baseTexture.width, baseTexture.height);
            newTexture.filterMode = FilterMode.Point;
            newTexture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    if (frontTexture.GetPixel(x, y).a <= 0f) // transparent
                        newTexture.SetPixel(x, y, baseTexture.GetPixel(x, y));
                    else
                        newTexture.SetPixel(x, y, frontTexture.GetPixel(x, y));
                }
            }

            newTexture.Apply();
            return newTexture;
        }

        /// <summary>
        /// Easy method to convert list to string 
        /// </summary>
        public static string Concatenate<T>(IEnumerable<T> source, string delimiter)
        {
            var itr = source.GetEnumerator();
            var s = new StringBuilder();
            bool first = true;
            while (itr.MoveNext())
            {
                if (first)
                    first = false;
                else
                    s.Append(delimiter);
                s.Append(itr.Current);
            }
            return s.ToString();
        }
    }
}