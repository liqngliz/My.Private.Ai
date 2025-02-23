using Autofac;
using GlobalExtensions;
using LLama.Common;
using My.Ai.App.Lib.ViewModels;
using My.Ai.Lib;
using My.Ai.Lib.Container;

//AIContainer
var container = new AIContainer("settings.json", "Chat.History.Template.json", "fileMetas.json").Container();
var chatViewModelFactory = container.Resolve<Func<ChatMode, IChatViewModel>>();
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
    ChatHistory newHistory = (await File.ReadAllTextAsync("Chat.History.Template.json")).ToChatHistory();
    return newHistory;
})
.WithName("NewChat")
.WithOpenApi();


app.MapPost("/Chat", async (ChatHistory chatHistory) => {
    var chat = await statelessChatViewModel.ChatAsync(chatHistory);
    return (ChatHistory)chat;
}).WithName("Chat")
.WithOpenApi();

app.Run();