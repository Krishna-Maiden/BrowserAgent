using AgentCore.AI;
using AgentCore.Automation;
using AgentCore.ElementResolution;

//using AgentCore.Interfaces;
using AgentCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using server.AgentCore.ElementResolution;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Dependency injection for Selector Resolver Engine
builder.Services.AddSingleton<ISelectorMemory, InMemorySelectorMemory>();

if (builder.Configuration.GetValue<bool>("LlmConfig:UseLocal"))
{
    
    builder.Services.AddSingleton<LocalLlmSelectorSuggester>();
    builder.Services.AddHttpClient<OpenAiSelectorSuggester>();
    //builder.Services.AddSingleton<ILlmSelectorSuggester, HybridSelectorSuggester>();

    builder.Services.AddSingleton<ILlmSelectorSuggester>(sp =>
        new HybridSelectorSuggester(
            sp.GetRequiredService<LocalLlmSelectorSuggester>(),
            sp.GetRequiredService<OpenAiSelectorSuggester>()));
}
else
{
    builder.Services.AddSingleton<ILlmSelectorSuggester, OpenAiSelectorSuggester>();
}

builder.Services.AddSingleton<IPageElementCache, PageElementCache>();
builder.Services.AddSingleton<PageElementCacheLoader>(); // Responsible for extracting page elements after navigation

// var llmProvider = builder.Configuration["LlmProvider"];
//if (llmProvider == "ollama")
//    builder.Services.AddSingleton<ILlmSelectorSuggester, LocalLlmSelectorSuggester>();
//else
//    builder.Services.AddHttpClient<ILlmSelectorSuggester, OpenAiSelectorSuggester>();


//builder.Services.AddSingleton<ILlmSelectorSuggester, OpenAiSelectorSuggester>();
builder.Services.AddSingleton<SelectorResolver>(); // Core resolver used by PlaywrightExecutor

// Core automation and LLM integration
builder.Services.AddHttpClient<LlmProcessor>();
builder.Services.AddSingleton<LlmProcessor>();
builder.Services.AddSingleton<PlaywrightExecutor>();
//builder.Services.AddSingleton<IAgentService, AgentService>();
//builder.Services.AddSingleton<IRetryMemory, RetryMemory>();
builder.Services.AddSingleton<RetryMemory>();
builder.Services.AddSingleton<AgentService>();

var app = builder.Build();

// Middleware
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

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

//app.UseAuthorization();
//app.MapControllers();

//app.Run();
