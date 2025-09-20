var builder = DistributedApplication.CreateBuilder(args);

// Add Redis for chat storage
var redis = builder.AddRedis("redis");

// Add chat server with Redis dependency
var chatServer = builder.AddProject<Projects.ChatApp_Server>("chatserver")
    .WithReference(redis);

builder.Build().Run();
