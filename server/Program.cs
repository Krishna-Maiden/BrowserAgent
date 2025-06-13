using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using AgentCore.AI;
using AgentCore.Automation;
using AgentCore.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
builder.Services.AddSingleton<LlmProcessor>();
builder.Services.AddSingleton<PlaywrightExecutor>();
builder.Services.AddSingleton<RetryMemory>();
builder.Services.AddSingleton<AgentService>();

var app = builder.Build();
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.MapPost("/run-task", async (HttpContext http, AgentService agentService) =>
{
    var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
    var task = JsonSerializer.Deserialize<Dictionary<string, string>>(body)?["task"];
    if (!string.IsNullOrWhiteSpace(task))
    {
        await agentService.RunUserTask(task);
        return Results.Ok("Task executed.");
    }
    return Results.BadRequest("Invalid task input.");
});

app.Run("http://localhost:5099");