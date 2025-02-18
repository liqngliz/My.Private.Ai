using GlobalExtensions;
using LLama.Common;
using My.Ai.Lib;

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
    var llmFolder = "llm".LlmFolder();
    var modelPath = Path.Combine(llmFolder, "DeepSeek-R1-Distill-Qwen-7B-Q5_K_M.gguf");
    var modelParams = modelPath.ToModelParams(80000);

    if(chatHistory.Messages.Count < 2) return chatHistory;

    var input = chatHistory.Messages[chatHistory.Messages.Count - 1];
    var messages = new List<ChatHistory.Message>();
    
    for(int i = 0; i < chatHistory.Messages.Count - 1; i++)
    {
        messages.Add(chatHistory.Messages[i]);
    }

    using var chat = new Chat(modelParams, new ChatHistory(messages.ToArray()));
    var inferenceParams = GlobalExt.DefaultAntiPrompt.ToInferenceParams(4096);
    await foreach(var text in chat.Prompt(input, inferenceParams))
        _ = text;

    return chat.History();
}).WithName("Chat")
.WithOpenApi();

app.Run();