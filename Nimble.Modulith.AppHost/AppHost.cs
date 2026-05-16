using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sqlserver").WithDataVolume();
var usersDb = sqlServer.AddDatabase("usersdb");
var productsDb = sqlServer.AddDatabase("productsdb");
var customersDb = sqlServer.AddDatabase("customersdb");

var papercut = builder.AddContainer("papercut", "jijiechen/papercut", "latest")
  .WithEndpoint("smtp", e => { e.TargetPort = 25; e.Port = 25; e.Protocol = ProtocolType.Tcp; e.UriScheme = "smtp"; })
  .WithEndpoint("ui", e => { e.TargetPort = 37408; e.Port = 37408; e.UriScheme = "http"; });

builder.AddProject<Projects.Nimble_Modulith_Web>("webapi")
    .WithReference(usersDb).WithReference(productsDb).WithReference(customersDb)
    .WithEnvironment("Papercut__Smtp__Url", papercut.GetEndpoint("smtp"))
    .WithEnvironment("Papercut__Ui__Url", papercut.GetEndpoint("ui"))
    .WaitFor(usersDb).WaitFor(productsDb).WaitFor(customersDb).WaitFor(papercut);

builder.Build().Run();