using ChatApp.Server.Hubs;
using ChatApp.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults from Aspire
builder.AddServiceDefaults();

// Add Redis connection using Aspire
builder.AddRedisClient("redis");

// Register services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IUserService, UserService>();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7000", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add OpenAPI for development
builder.Services.AddOpenApi();

var app = builder.Build();

// Map service defaults
app.MapDefaultEndpoints();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors();
}

app.UseHttpsRedirection();

// Map SignalR hub
app.MapHub<ChatHub>("/chathub");

// Optional: Add health check endpoint
app.MapGet("/health", () => "Healthy");

// Add a simple test endpoint to verify Redis connectivity
app.MapGet("/test-redis", async (IServiceProvider services) =>
{
    try
    {
        var chatService = services.GetRequiredService<IChatService>();
        var userService = services.GetRequiredService<IUserService>();
        
        // Test basic operations
        var users = await userService.GetOnlineUsersAsync();
        var messages = await chatService.GetChatHistoryAsync();
        
        return Results.Ok(new { 
            Status = "Redis connection successful", 
            OnlineUsers = users.Count, 
            MessageHistory = messages.Count 
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Redis connection failed: {ex.Message}");
    }
});

app.Run();
