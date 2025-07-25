using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();

const string filePath = "story.json";

List<StoryNode> LoadStory()
{
    var json = File.ReadAllText(filePath);
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    return JsonSerializer.Deserialize<List<StoryNode>>(json, options) ?? new();
}

void SaveStory(List<StoryNode> story)
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    File.WriteAllText(filePath, JsonSerializer.Serialize(story, options));
}

app.MapGet("/story/start", () =>
{
    var story = LoadStory();
    return story.Find(s => s.Id == 1);
});

app.MapPost("/story/next", async (HttpContext context) =>
{
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var request = await JsonSerializer.DeserializeAsync<NextRequest>(context.Request.Body, options);
    var story = LoadStory();

    if (request?.Choice == null)
    {
        await context.Response.WriteAsJsonAsync(new { Message = "Missing choice in request." });
        return;
    }

    var current = story.Find(s => s.Id == request.CurrentId);

    if (current == null)
    {
        await context.Response.WriteAsJsonAsync(new { Message = "Current node not found." });
        return;
    }

    int? nextId = request.Choice.ToLower() == "left" ? current.NextLeft : current.NextRight;
    var nextNode = story.Find(s => s.Id == nextId);

    if (nextNode != null)
    {
        await context.Response.WriteAsJsonAsync(nextNode);
    }
    else
    {
        await context.Response.WriteAsJsonAsync(new { Message = "End of story or invalid choice." });
    }
});

app.MapPost("/story/add", async (HttpContext context) =>
{
    var newNode = await JsonSerializer.DeserializeAsync<StoryNode>(context.Request.Body);
    var story = LoadStory();
    story.Add(newNode);
    SaveStory(story);
    await context.Response.WriteAsJsonAsync(new { Message = "Node added successfully." });
});


app.Run();

record StoryNode(int Id, string Text, int? NextLeft, int? NextRight);
record NextRequest(int CurrentId, string Choice);
