using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using LinkListStory;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "wwwroot"
});

var contentRoot = builder.Environment.ContentRootPath;
var dataDir = Path.Combine(contentRoot, "data");
Directory.CreateDirectory(dataDir);

var storySrc = Path.Combine(contentRoot, "..", "LinkListStory", "story.json");
var storyDest = Path.Combine(dataDir, "story.json");
if (!File.Exists(storyDest))       
    File.Copy(storySrc, storyDest);

var usersPath = Path.Combine(dataDir, "users.json");
if (!File.Exists(usersPath))
    File.WriteAllText(usersPath, "[]");

builder.WebHost.UseUrls("http://localhost:51441");
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

const string storyFilePath = "data/story.json";
const string userFilePath = "data/users.json";

int totalScore = 0;
string? currentUserName = null;

List<StoryNode> LoadStory()
{
    if (!File.Exists(storyFilePath))
    {
        Console.WriteLine($"Error: Story file not found at {storyFilePath}");
        return new List<StoryNode>();
    }

    try
    {
        var json = File.ReadAllText(storyFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<List<StoryNode>>(json, options) ?? new();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading story: {ex.Message}");
        return new List<StoryNode>();
    }
}

void SaveStory(List<StoryNode> story)
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    File.WriteAllText(storyFilePath, JsonSerializer.Serialize(story, options));
}

List<User> LoadUsers()
{
    if (!File.Exists(userFilePath))
    {
        Console.WriteLine($"Error: User file not found at {userFilePath}");
        return new List<User>();
    }

    var json = File.ReadAllText(userFilePath);
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    return JsonSerializer.Deserialize<List<User>>(json, options) ?? new();
}

void SaveUsers(List<User> users)
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    File.WriteAllText(userFilePath, JsonSerializer.Serialize(users, options));
}

// User registration endpoint
app.MapPost("/user/register", async (HttpContext context) =>
{
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var user = await JsonSerializer.DeserializeAsync<User>(context.Request.Body, options);

    if (user == null || string.IsNullOrWhiteSpace(user.Name))
    {
        await context.Response.WriteAsJsonAsync(new { Message = "Invalid user data." });
        return;
    }

    var users = LoadUsers();

    currentUserName = user.Name; // Set the current user
    await context.Response.WriteAsJsonAsync(new { Message = "User registered successfully." });
});

app.MapGet("/story/start", (HttpContext context) =>
{
    if (string.IsNullOrEmpty(currentUserName))
    {
        return Results.Json(new { Message = "Please register a user first." });
    }

    var story = LoadStory();
    if (story == null || !story.Any())
    {
        return Results.Json(new { Message = "Story data is missing or invalid." });
    }

    totalScore = 0;
    var startNode = story.Find(s => s.Id == 1);
    if (startNode != null)
    {
        totalScore += startNode.Score;
        return Results.Json(new { Node = startNode, TotalScore = totalScore });
    }

    return Results.Json(new { Message = "Starting node not found in the story." });
});

// Proceed to the next story node
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
        totalScore += nextNode.Score; // Add the score for the next node
        await context.Response.WriteAsJsonAsync(new { Node = nextNode, TotalScore = totalScore });
    }
    else
    {
        // Save the user's final score
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Name.Equals(currentUserName, StringComparison.OrdinalIgnoreCase));
        if (user != null)
        {
            users.Remove(user);
            users.Add(user with { Score = totalScore });
            SaveUsers(users);
        }

        await context.Response.WriteAsJsonAsync(new { Message = "End of story or invalid choice.", TotalScore = totalScore });
    }
});

// Add a new story node
app.MapPost("/story/add", async (HttpContext context) =>
{
    var newNode = await JsonSerializer.DeserializeAsync<StoryNode>(context.Request.Body);
    var story = LoadStory();
    story.Add(newNode);
    SaveStory(story);
    await context.Response.WriteAsJsonAsync(new { Message = "Node added successfully." });
});


app.MapPost("/user/score", async (HttpContext context) =>
{
    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var incoming = await JsonSerializer.DeserializeAsync<User>(context.Request.Body, opts);

    if (incoming == null || string.IsNullOrWhiteSpace(incoming.Name))
    {
        await context.Response.WriteAsJsonAsync(new { Message = "Invalid user data." });
        return;
    }

    var users = LoadUsers();
    users.Add(incoming); 
    SaveUsers(users);

    await context.Response.WriteAsJsonAsync(new { Message = "Score saved." });
});

app.MapGet("/user/list", (string? sort, string? order, string? algo) =>
{
    var users = LoadUsers();
    string sortBy = sort ?? "score";
    bool ascending = order?.ToLower() != "desc";
    string algorithm = algo?.ToLower(); //?? "bubble"; test unknown methods.


    // set different sorting methods based on the algorithm parameter
    if (algorithm == "bubble")
    {
        users = SortMethod.BubbleSort(users, sortBy, ascending);
    }
    if (algorithm == "insertion")
    {
        users = SortMethod.InsertionSort(users, sortBy, ascending);
    }
    if (algorithm == "selection")
    {
        users = SortMethod.SelectionSort(users, sortBy, ascending);
    }

    return Results.Ok(users);
});
app.Run();
