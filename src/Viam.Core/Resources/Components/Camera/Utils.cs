using System;

namespace Viam.Core.Resources.Components.Camera
{
    internal static class Utils
    {
        public static (int width, int height) GetImageDimensions(ReadOnlySpan<byte> bytes, MimeType mimeType) =>
            mimeType switch
            {
                MimeType.Jpeg => GetJpegImageDimensions(bytes),
                MimeType.Png => GetPngImageDimensions(bytes),
                MimeType.ViamRgba => GetViamRgbaImageDimensions(bytes),
                _ => throw new ArgumentOutOfRangeException(nameof(mimeType), mimeType, "Unsupported image format")
            };

        private static (int width, int height) GetViamRgbaImageDimensions(ReadOnlySpan<byte> bytes)
        {
            if (bytes[..4] != [0x52, 0x47, 0x42, 0x41])
                throw new ArgumentException("Invalid Viam RGBA header", nameof(bytes));
#if NET6_0_OR_GREATER
            var width = BitConverter.ToInt32(bytes.Slice(4, 4));
            var height = BitConverter.ToInt32(bytes.Slice(8, 4));
#elif NETFRAMEWORK
            var width = BitConverter.ToInt32(bytes.Slice(4, 4).ToArray(), 0);
            var height = BitConverter.ToInt32(bytes.Slice(8, 4).ToArray(), 0);
#else
            throw new PlatformNotSupportedException();
#endif
            return (width, height);
        }

        private static (int width, int height) GetPngImageDimensions(ReadOnlySpan<byte> bytes)
        {
            if (bytes[12..16] != [0x49, 0x48, 0x44, 0x52])
                throw new ArgumentException("Invalid PNG header", nameof(bytes));
#if NET6_0_OR_GREATER
            var width = BitConverter.ToInt32(bytes[16..20]);
            var height = BitConverter.ToInt32(bytes[20..24]);
#elif NETFRAMEWORK
            var width = BitConverter.ToInt32(bytes.Slice(16, 4).ToArray(), 0);
            var height = BitConverter.ToInt32(bytes.Slice(20, 4).ToArray(), 0);
#else
            throw new PlatformNotSupportedException();
#endif
            return (width, height);
        }

        public static (int width, int height) GetJpegImageDimensions(ReadOnlySpan<byte> bytes)
        {
            if (bytes[0] != 0xFF || bytes[1] != 0xD8) // check for SOI marker
                throw new ArgumentException("Not a valid JPEG image.", nameof(bytes));

            var i = 2;
            while (i < bytes.Length)
            {
                if (bytes[i] == 0xFF)
                {
                    if (bytes[i + 1] >= 0xC0 && bytes[i + 1] <= 0xC3) // check for SOF marker
                    {
                        var height = (bytes[i + 5] << 8) + bytes[i + 6];
                        var width = (bytes[i + 7] << 8) + bytes[i + 8];
                        return (width, height);
                    }
                    else
                    {
                        i += 2;
                        var length = (bytes[i] << 8) + bytes[i + 1];
                        i += length;
                    }
                }
                else
                {
                    i++;
                }
            }

            throw new ArgumentException("Not a valid JPEG image.");
        }
    }
}