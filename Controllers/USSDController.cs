using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

[Route("api/[controller]")]
[ApiController]
public class UssdController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;

    public UssdController(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    private IDatabase RedisDb => _redis.GetDatabase();

    [HttpPost("ReceiveUssd")]
    public async Task<IActionResult> ReceiveUssd()
    {
        var form = await Request.ReadFormAsync();
        string sessionId = form["sessionId"];
        string userInput = form["text"].ToString().Trim();

        string redisKey = $"ussd:{sessionId}";

        // Get saved path from Redis
        string savedPath = RedisDb.StringGet(redisKey);
        string path = !string.IsNullOrEmpty(savedPath) ? savedPath : "";

        // -------- Handle navigation manually --------
        if (userInput == "00")
        {
            // Main Menu → clear session
            path = "";
        }
        else if (userInput == "0")
        {
            // Back → remove last part
            var parts = string.IsNullOrEmpty(path) ? new string[] { } : path.Split('*');
            if (parts.Length > 0)
                path = string.Join('*', parts[..^1]);
        }
        else
        {
            // Normal input → append
            path = string.IsNullOrEmpty(path) ? userInput : path + "*" + userInput;
        }

        // Save updated path in Redis
        RedisDb.StringSet(redisKey, path, TimeSpan.FromMinutes(5));

        // Split updated path into parts
        var partsFinal = string.IsNullOrEmpty(path) ? new string[] { } : path.Split('*');

        // -------- Build USSD menu --------
        string response;

        if (partsFinal.Length == 0)
        {
            response = "CON Welcome to Kili's Profile\n";
            response += "1. Career Path\n";
            response += "2. Education Background\n";
            response += "3. Family\n";
            response += "4. Skills\n";
            response += "5. More\n";
            response += "0. Back\n00. Main Menu";
        }
        else if (partsFinal[0] == "1" && partsFinal.Length == 1)
        {
            response = "CON Career Path:\n";
            response += "=> Senior .NET Developer\n";
            response += "=> Backend Engineer\n";
            response += "=> Fintech Integrations Specialist\n";
            response += "0. Back\n00. Main Menu";
        }
        else if (partsFinal[0] == "2" && partsFinal.Length == 1)
        {
            response = "CON Education Background:\n";
            response += "=> BSc Computer Science\n";
            response += "=> Certifications in Cloud & APIs\n";
            response += "0. Back\n00. Main Menu";
        }
        else if (partsFinal[0] == "3" && partsFinal.Length == 1)
        {
            response = "CON Family:\n";
            response += "Proud family man.\n";
            response += "0. Back\n00. Main Menu";
        }
        else if (partsFinal[0] == "4" && partsFinal.Length == 1)
        {
            response = "CON View Skills:\n";
            response += "1. Coding Skills\n";
            response += "2. General Skills\n";
            response += "0. Back\n00. Main Menu";
        }
        else if (partsFinal[0] == "4" && partsFinal.Length == 2 && partsFinal[1] == "1")
        {
            response = "CON Coding Skills:\n";
            response += "1. .NET\n";
            response += "2. C#\n";
            response += "3. API Integrations\n";
            response += "0. Back\n00. Main Menu";
        }
        else if (partsFinal[0] == "4" && partsFinal.Length == 2 && partsFinal[1] == "2")
        {
            response = "CON General Skills:\n";
            response += "=> Leadership\n";
            response += "=> Problem Solving\n";
            response += "=> System Design\n";
            response += "0. Back\n00. Main Menu";
        }
        else if (partsFinal[0] == "5" && partsFinal.Length == 1)
        {
            response = "CON More Info:\n";
            response += "Other info will be displayed here.\n";
            response += "0. Back\n00. Main Menu";
        }
        else
        {
            response = "CON Invalid choice.\n0. Back\n00. Main Menu";
        }

        return Content(response, "text/plain");
    }
}