using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Bindings;

namespace MegabonkBetterMinimap
{
    public static class TextureManager
    {
        public static Texture2D Load(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(
                $"MegabonkBetterMinimap.Assets.Resources.Textures.{resourceName}.png"
            );

            if (stream == null)
            {
                Debug.LogError($"Embedded resource not found!");
            }

            byte[] imageAsBytes = new byte[stream.Length];
            stream.Read(imageAsBytes, 0, imageAsBytes.Length);

            Texture2D tex = new(2, 2, TextureFormat.RGBA32, false, false);

            unsafe
            {
                System.IntPtr intPtr = Object.MarshalledUnityObject.MarshalNotNull(tex);

                fixed (byte* ptr = imageAsBytes)
                {
                    ManagedSpanWrapper managedSpanWrapper = new(ptr, imageAsBytes.Length);
                    ImageConversion.LoadImage_Injected(intPtr, ref managedSpanWrapper, false);
                }
            }

            return tex;
        }
    }
}
