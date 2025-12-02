using System.Reflection;
using System.Runtime.InteropServices;

namespace Viam.Client.WebRTC
{
    internal partial class ViamRustUtils
    {
        static ViamRustUtils()
        {
            // Register the DllImport resolver for the current assembly.
            NativeLibrary.SetDllImportResolver(
                typeof(ViamRustUtils).Assembly,
                DllImportResolver
            );
        }

        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            // Use the default resolver if not our library.
            if (libraryName != LibraryName) return IntPtr.Zero;
            
            var assemblyPath = Path.GetDirectoryName(System.AppContext.BaseDirectory);
            if (assemblyPath == null)
            {
                throw new InvalidOperationException("Unable to get assembly path");
            }

            var libPath = Path.Combine(assemblyPath, "runtimes", RuntimeInformation.RuntimeIdentifier, "native");
            if (!Path.Exists(libPath))
            {
                throw new FileNotFoundException($"Expected library path does not exist: {libPath}");
            }

            if (OperatingSystem.IsLinux())
            {
                libraryName += ".so";
            } else if (OperatingSystem.IsMacOS())
            {
                libraryName += ".dylib";
            }
            else if (OperatingSystem.IsWindows())
            {
                libraryName += ".dll";
            }
            else
            {
                throw new PlatformNotSupportedException(
                    $"Unsupported platform: {RuntimeInformation.OSDescription}");
            }

            return NativeLibrary.Load(Path.Combine(libPath, libraryName));
        }

        private const string LibraryName = "libviam_rust_utils";

        [LibraryImport(LibraryName, EntryPoint = "init_rust_runtime")]
        private static partial IntPtr InitRustRuntimeInternal();

        [LibraryImport(LibraryName, EntryPoint = "free_rust_runtime")]
        private static partial int FreeRustRuntimeInternal(IntPtr pointer);

        [LibraryImport(LibraryName, EntryPoint = "dial")]
        private static partial IntPtr DialInternal(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string c_uri,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? c_entity,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? c_type,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? c_payload,
            [MarshalAs(UnmanagedType.I1)] bool c_allow_insec,
            float c_timeout,
            IntPtr rt_ptr // This points to your Rust runtime (obtained separately)
        );

        [LibraryImport(LibraryName, EntryPoint = "free_string")]
        private static partial void FreeStringInternal(IntPtr ptr);

        public static IntPtr InitRustRuntime()
        {
            return InitRustRuntimeInternal();
        }

        public static int FreeRustRuntime(IntPtr pointer)
        {
            return FreeRustRuntimeInternal(pointer);
        }

        public static string Dial(string uri, string? entity, string? type, string? payload, bool allowInsecure,
            float timeout, IntPtr rtPtr)
        {
            var result = DialInternal(uri, entity, type, payload, allowInsecure, timeout, rtPtr);
            if (result == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to dial, check address and credentials");
            }

            var resultString = Marshal.PtrToStringUTF8(result);
            FreeStringInternal(result);
            if (resultString == null)
            {
                throw new InvalidOperationException("Failed to convert result to string");
            }

            return resultString;
        }
    }
}