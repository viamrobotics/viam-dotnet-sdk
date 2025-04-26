using System.Runtime.InteropServices;

namespace Viam.Client.WebRTC
{
    internal class RustRTC
    {
        //#if WINDOWS
        private const string LibraryName = "runtimes\\win-x64\\native\\libviam_rust_utils-windows_x86_64.dll";
        //#elif LINUX
        //    private const string LibraryName = "libviam_rust_utils-linux_x86_64.so";
        //#elif OSX
        //    private const string LibraryName = "libviam_rust_utils-macos_aarch64.dylib";
        //#else
        //#error Unsupported platform
        //#endif
        [DllImport(LibraryName, EntryPoint = "init_rust_runtime")]
        public static extern IntPtr InitRustRuntime();

        [DllImport(LibraryName, EntryPoint = "free_rust_runtime")]
        public static extern int FreeRustRuntime(IntPtr pointer);

        [DllImport(LibraryName, EntryPoint = "dial")]
        public static extern IntPtr Dial(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string c_uri,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? c_entity,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? c_type,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? c_payload,
            [MarshalAs(UnmanagedType.I1)] bool c_allow_insec,
            float c_timeout,
            IntPtr rt_ptr // This points to your Rust runtime (obtained separately)
        );

        [DllImport(LibraryName, EntryPoint = "free_string")]
        public static extern void FreeString(IntPtr ptr);
    }
}
