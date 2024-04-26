using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Viam.Core.Resources;
using Viam.ModularResources;

try
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        throw new PlatformNotSupportedException("We currently only support Modular Resources on Linux/macOS");
    if (args.Length != 1)
        throw new ArgumentException("You must provide a Unix socket path");
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureLogging(c =>
        {
            c.AddSimpleConsole(o => o.SingleLine = true);
        })
        .ConfigureServices(s =>
        {
            s.AddLogging();
            s.AddSingleton<ResourceManager>();
        })
        .ConfigureWebHostDefaults(b =>
        {
            b.UseStartup<Startup>();
        })
        .ConfigureWebHost(webBuilder =>
        {
            webBuilder.ConfigureKestrel(options =>
            {
                var socket = args[0];
                options.ListenUnixSocket(socket, c => c.Protocols = HttpProtocols.Http2);
            });
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
