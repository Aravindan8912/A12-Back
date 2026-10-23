using A12.Infrastructure;
using A12.SchedulerWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddA12Database(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
