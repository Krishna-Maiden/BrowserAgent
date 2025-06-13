using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using AgentCore.AI;
using AgentCore.Automation;
using AgentCore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<LlmProcessor>();
builder.Services.AddSingleton<PlaywrightExecutor>();
builder.Services.AddSingleton<RetryMemory>();
builder.Services.AddSingleton<AgentService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();