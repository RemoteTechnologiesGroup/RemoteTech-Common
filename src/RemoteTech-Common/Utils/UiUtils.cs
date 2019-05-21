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
        /// Create new texture and fill up with given color
        /// </summary>
        //https://forum.unity3d.com/threads/best-easiest-way-to-change-color-of-certain-pixels-in-a-single-sprite.223030/
        public static Texture2D CreateAndColorize(int width, int height, Color thisColor)
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
        public static void Colorize(Texture2D existingTexture, Color thisColor)
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
        public static Texture2D CreateAndOverlay(Texture2D baseTexture, Texture2D frontTexture)
        {
            Texture2D newTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false);
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

            newTexture.Compress(true);
            newTexture.Apply(false, true);
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

        /// <summary>
        /// Convert from Color to String (R,G,B,A)
        /// </summary>
        public static string ColorToString(Color color)
        {
            return color.r + "," + color.g + "," + color.b + "," + color.a;
        }

        /// <summary>
        /// Convert from String (R,G,B,A) to Color
        /// </summary>
        public static Color StringToColor(string colorString)
        {
            try
            {
                string[] colors = colorString.Split(',');
                return new Color(float.Parse(colors[0]), float.Parse(colors[1]), float.Parse(colors[2]), float.Parse(colors[3]));
            }
            catch
            {
                return Color.white;
            }
        }

        /// <summary>
        /// Duplicate and make given texture readable
        /// </summary>
        public static Texture2D getReadableCopy(Texture2D source)
        {
            //source: https://stackoverflow.com/questions/44733841/how-to-make-texture2d-readable-via-script
            RenderTexture tmp = RenderTexture.GetTemporary(
                source.width, source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);
            Graphics.Blit(source, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;

            Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
            readable.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readable.Apply(false, false); //latter false to keep readable

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            return readable;
        }

        /// <summary>
        /// Make new unreadble texture on subregion of a bigger texture
        /// Eg make a bunch of UI textures out of 1 giant texture of bunch subregions
        /// </summary>
        public static Texture2D createSubregionTexture(Texture2D source, int startX, int startY, int subregionWidth, int subregionHeight, bool compress = false, bool readable = false)
        {
            Texture2D newTexture = new Texture2D(subregionWidth, subregionHeight, TextureFormat.ARGB32, false);
            Color[] c = source.GetPixels(startX, startY, subregionWidth, subregionHeight, 0);
            newTexture.SetPixels(c, 0);
            if (compress) { newTexture.Compress(true); }
            newTexture.Apply(false, !readable); //unreadable state has performance boost

            return newTexture;
        }
    }
}