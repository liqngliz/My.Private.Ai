using Autofac;
using GlobalExtensions;
using LLama.Common;
using My.Ai.App.Lib.ChatModels;
using My.Ai.App.Lib.Models;
using My.Ai.Lib;
using My.Ai.Lib.Container;

//AIContainer
var settingsJson = File.ReadAllText("settings.json");
var chatJson = File.ReadAllText("Chat.History.Template.json");
Settings settings = settingsJson;
var modelPath = Path.Combine("llm".LlmFolder(), settings.ModelPath);
var container = new AIContainer(settingsJson, chatJson, modelPath).Container();
var chatViewModelFactory = container.Resolve<Func<ChatMode, IChatModel>>();
var statelessChatViewModel = chatViewModelFactory(ChatMode.Stateless);
//

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapGet("/", async (HttpContext context) => {
    context.Response.ContentType = "text/html";
    return await File.ReadAllTextAsync("wwwroot/index.html");
}).ExcludeFromDescription();

app.MapGet("/New", async () => {
    History newHistory = (await File.ReadAllTextAsync("Chat.History.Template.json")).ToHistory();
    return newHistory;
})
.WithName("NewChat")
.WithOpenApi();


app.MapPost("/Chat", async (History chatHistory) => {
    var chat = await statelessChatViewModel.ChatAsync(chatHistory);
    return (History)chat;
}).WithName("Chat")
.WithOpenApi();

app.Run();