var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();

// TODO: Use the actual socket
builder.WebHost.ConfigureKestrel(o => o.ListenUnixSocket("/tmp/kestrel-test.sock"));

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

app.Run();
