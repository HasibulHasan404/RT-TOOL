using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Linq;

#pragma warning disable CA1416 // Platform compatibility validation

namespace RTTool;

internal class Program
{
    private static string mainToken = "";
    private static string username = "";
    private static ConsoleColor defaultColor = ConsoleColor.Green;
    private static readonly HttpClient client = new HttpClient();
    private static List<string> savedTokens = new List<string>();
    private const string API_BASE = "https://discord.com/api/v10";
    private static bool useAnimations = true;

    static async Task Main()  // Changed to async Task
    {
        Console.Title = "RT-TOOL Advanced";
        SetupConsole();
        
        // Add the hacker animation at startup
        ShowHackerAnimation();
        
        // Show disclaimer
        ShowDisclaimer();
        
        // Check for spam.txt
        CheckAndCreateSpamFile();
        
        while (!await AuthenticateToken())
        {
            // Keep asking for token until valid
        }

        await ShowMainMenu();
    }

    static void SetupConsole()
    {
        Console.ForegroundColor = defaultColor;
        
        // Get the screen size
        int screenWidth = Console.LargestWindowWidth;
        int screenHeight = Console.LargestWindowHeight;

        // Set desired window size (adjust these values as needed)
        int windowWidth = 120;  // Increased from 100
        int windowHeight = 35;  // Increased from 30

        // Make sure window size doesn't exceed screen size
        windowWidth = Math.Min(windowWidth, screenWidth);
        windowHeight = Math.Min(windowHeight, screenHeight);

        try
        {
            // Set buffer size first (must be equal to or larger than window size)
            Console.SetBufferSize(windowWidth, windowHeight);
            
            // Set window size
            Console.SetWindowSize(windowWidth, windowHeight);
            
            // Center the window
            Console.SetWindowPosition(
                (screenWidth - windowWidth) / 2,
                (screenHeight - windowHeight) / 2
            );
        }
        catch
        {
            // Fallback if setting exact size fails
            try
            {
                Console.WindowWidth = windowWidth;
                Console.WindowHeight = windowHeight;
            }
            catch
            {
                // If all else fails, just maximize the window
                Console.SetWindowSize(
                    Console.LargestWindowWidth - 10,
                    Console.LargestWindowHeight - 10
                );
            }
        }
    }

    static void DisplayBanner()
    {
        ShowBannerAnimation();
    }

    static async Task<bool> AuthenticateToken()
    {
        DisplayBanner();
        
        // Try to load saved token
        if (File.Exists("token.txt"))
        {
            mainToken = File.ReadAllText("token.txt").Trim();
            if (!string.IsNullOrEmpty(mainToken))
            {
                if (await ValidateAndSetUsername())
                {
                    Console.WriteLine();
                    WriteLineCenter("Loaded saved token.");
                    WriteLineCenter($"Logged in as: {username}");
                    Thread.Sleep(1000);
                    return true;
                }
            }
        }

        Console.WriteLine();
        WriteCenter("Enter your main account token: ");
        mainToken = Console.ReadLine();

        if (!string.IsNullOrEmpty(mainToken))
        {
            if (await ValidateAndSetUsername())
            {
                File.WriteAllText("token.txt", mainToken);
                Console.WriteLine();
                WriteLineCenter("Token saved successfully!");
                WriteLineCenter($"Logged in as: {username}");
                Thread.Sleep(1000);
                return true;
            }
        }

        Console.WriteLine();
        WriteLineCenter("Invalid token! Please try again.");
        Thread.Sleep(1500);
        return false;
    }

    static async Task<bool> ValidateAndSetUsername()
    {
        try 
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(mainToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            
            var response = await client.GetAsync("https://discord.com/api/v10/users/@me");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<JsonElement>(content);
                username = userData.GetProperty("username").GetString();
                return true;
            }
        }
        catch 
        {
            // If any error occurs, return false
        }
        return false;
    }

    static async Task ShowMainMenu()
    {
        while (true)
        {
            Console.Clear();
            DisplayBanner();
            Console.WriteLine();
            WriteLineCenter("=== RT-TOOL Advanced ===");
            WriteLineCenter("1. Account Management");
            WriteLineCenter("2. Server Info");
            WriteLineCenter("3. Message Tools");
            WriteLineCenter("4. Settings");
            WriteLineCenter("5. Exit");
            Console.WriteLine();
            WriteCenter("Choose an option: ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1: await ShowAccountMenu(); break;
                    case 2: await ShowServerMenu(); break;
                    case 3: await ShowMessageTools(); break;
                    case 4: ShowSettingsMenu(); break;
                    case 5: return;
                }
            }
        }
    }

    static async Task ShowAccountMenu()
    {
        while (true)
        {
            Console.Clear();
            DisplayBanner();
            Console.WriteLine();
            WriteLineCenter("=== Account Management ===");
            WriteLineCenter("1. View Account Info");
            WriteLineCenter("2. Change Status");
            WriteLineCenter("3. Return");
            Console.WriteLine();
            WriteCenter("Choose option: ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1: await ViewAccountInfo(); break;
                    case 2: await ChangeStatus(); break;
                    case 3: return;
                }
            }
        }
    }

    static async Task ViewAccountInfo()
    {
        DisplayBanner();
        SimulateProgress("Fetching account information...");

        try 
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(mainToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            var response = await client.GetAsync("https://discord.com/api/v10/users/@me");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<JsonElement>(content);

                Console.WriteLine();
                
                // Create box with fixed width
                const int boxWidth = 60;
                string horizontalLine = new string('═', boxWidth - 2);
                string emptyLine = $"║{new string(' ', boxWidth - 2)}║";

                // Box elements
                WriteLineCenter($"╔{horizontalLine}╗");
                WriteLineCenter($"║{CenterText("Account Information", boxWidth - 2)}║");
                WriteLineCenter($"╠{horizontalLine}╣");
                WriteLineCenter(emptyLine);
                
                // Token info with truncation
                string tokenDisplay = mainToken.Length > 30 ? mainToken.Substring(0, 30) + "..." : mainToken;
                WriteLineCenter($"║  Token: {tokenDisplay.PadRight(boxWidth - 11)}║");
                WriteLineCenter(emptyLine);
                
                // Account Details section
                WriteLineCenter($"╠{horizontalLine}╣");
                WriteLineCenter($"║{CenterText("Account Details", boxWidth - 2)}║");
                WriteLineCenter($"╠{horizontalLine}╣");
                WriteLineCenter(emptyLine);
                
                string username = userData.GetProperty("username").GetString();
                string id = userData.GetProperty("id").GetString();
                string email = userData.GetProperty("email").GetString();
                string phone = userData.GetProperty("phone").GetString() ?? "Not Set";

                WriteLineCenter($"║  Username: {username.PadRight(boxWidth - 13)}║");
                WriteLineCenter($"║  ID: {id.PadRight(boxWidth - 7)}║");
                WriteLineCenter($"║  Email: {email.PadRight(boxWidth - 10)}║");
                WriteLineCenter($"║  Phone: {phone.PadRight(boxWidth - 10)}║");
                WriteLineCenter(emptyLine);
                
                // Status section
                WriteLineCenter($"╠{horizontalLine}╣");
                WriteLineCenter($"║{CenterText("Status Information", boxWidth - 2)}║");
                WriteLineCenter($"╠{horizontalLine}╣");
                WriteLineCenter(emptyLine);
                
                // Get actual status from Discord
                var statusResponse = await MakeDiscordRequest("/users/@me/settings", HttpMethod.Get);
                string status = "Online"; // Default value
                
                if (statusResponse.HasValue)
                {
                    status = statusResponse.Value.GetProperty("status").GetString() switch
                    {
                        "online" => "Online",
                        "idle" => "Idle",
                        "dnd" => "Do Not Disturb",
                        "invisible" => "Invisible",
                        _ => "Online"
                    };
                }
                
                string premium = userData.GetProperty("premium_type").GetInt32() > 0 ? "Nitro" : "Normal";
                
                WriteLineCenter($"║  Current Status: {status.PadRight(boxWidth - 19)}║");
                WriteLineCenter($"║  Premium Type: {premium.PadRight(boxWidth - 17)}║");
                WriteLineCenter(emptyLine);
                
                // Bottom of box
                WriteLineCenter($"╚{horizontalLine}╝");
            }
            else
            {
                WriteLineCenter($"\nError fetching account info: {response.StatusCode}");
                WriteLineCenter($"Response: {await response.Content.ReadAsStringAsync()}");
                WriteLineCenter($"Token used: {mainToken.Substring(0, 20)}...");
            }
        }
        catch (Exception ex)
        {
            WriteLineCenter($"\nError: {ex.Message}");
        }

        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    static void SimulateProgress(string message)
    {
        if (!useAnimations)
        {
            WriteLineCenter(message);
            WriteLineCenter("Done!");
            return;
        }

        int totalWidth = 52; // Progress bar width + 2 for brackets
        int spaces = (Console.WindowWidth - (message.Length + totalWidth)) / 2;
        Console.Write(new string(' ', spaces) + message + " [");
        
        for (int i = 0; i <= 50; i++)
        {
            Console.Write("█");
            Thread.Sleep(20);
        }
        Console.WriteLine("] 100%");
    }

    static async Task ShowMessageTools()
    {
        while (true)
        {
            Console.Clear();
            DisplayBanner();
            Console.WriteLine();
            WriteLineCenter("=== Message Tools ===");
            WriteLineCenter("1. Send Message");
            WriteLineCenter("2. Send DM");
            WriteLineCenter("3. View Message History");
            WriteLineCenter("4. Return");
            Console.WriteLine();
            WriteCenter("Choose option: ");
            
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1: await SendMessage(); break;
                    case 2: await SendDirectMessage(); break;
                    case 3: await ViewMessageHistory(); break;
                    case 4: return;
                }
            }
        }
    }

    static async Task ShowServerMenu()
    {
        while (true)
        {
            Console.Clear();
            DisplayBanner();
            Console.WriteLine();
            WriteLineCenter("=== Server Tools ===");
            WriteLineCenter("1. Server Info");
            WriteLineCenter("2. Member Count");
            WriteLineCenter("3. Channel List");
            WriteLineCenter("4. Role List");
            WriteLineCenter("5. Return");
            Console.WriteLine();
            WriteCenter("Choose option: ");
            
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1: await ShowServerInfo(); break;
                    case 2: await ShowMemberCount(); break;
                    case 3: await ShowChannelList(); break;
                    case 4: await ShowRoleList(); break;
                    case 5: return;
                }
            }
        }
    }

    static async Task ShowServerInfo()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Server Info ===");
        WriteCenter("Enter Server ID: ");
        string serverId = Console.ReadLine();
        
        if (!string.IsNullOrEmpty(serverId))
        {
            try
            {
                var result = await MakeDiscordRequest($"/guilds/{serverId}", HttpMethod.Get);
                if (result.HasValue)
                {
                    Console.WriteLine();
                    const int boxWidth = 60;
                    string horizontalLine = new string('═', boxWidth - 2);
                    string emptyLine = $"║{new string(' ', boxWidth - 2)}║";

                    WriteLineCenter($"╔{horizontalLine}╗");
                    WriteLineCenter($"║{CenterText("Server Information", boxWidth - 2)}║");
                    WriteLineCenter($"╠{horizontalLine}╣");
                    WriteLineCenter(emptyLine);
                    
                    string name = result.Value.GetProperty("name").GetString();
                    string owner = result.Value.GetProperty("owner_id").GetString();
                    string region = result.Value.GetProperty("region").GetString();
                    
                    WriteLineCenter($"║  Name: {name.PadRight(boxWidth - 9)}║");
                    WriteLineCenter($"║  Owner ID: {owner.PadRight(boxWidth - 13)}║");
                    WriteLineCenter($"║  Region: {region.PadRight(boxWidth - 11)}");
                    WriteLineCenter(emptyLine);
                    WriteLineCenter($"╚{horizontalLine}╝");
                }
            }
            catch (Exception ex)
            {
                WriteLineCenter($"Error: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    static void ShowSettingsMenu()
    {
        while (true)
        {
            Console.Clear();
            DisplayBanner();
            Console.WriteLine();
            WriteLineCenter("=== Settings ===");
            WriteLineCenter("1. Change Theme Color");
            WriteLineCenter("2. Toggle Animations");
            WriteLineCenter("3. Clear Saved Data");
            WriteLineCenter("4. About");
            WriteLineCenter("5. Return");
            Console.WriteLine();
            WriteCenter("Choose option: ");
            
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1:
                        ChangeThemeColor();
                        break;
                    case 2:
                        ToggleAnimations();
                        break;
                    case 3:
                        ClearSavedData();
                        break;
                    case 4:
                        ShowAbout();
                        break;
                    case 5:
                        return;
                }
            }
        }
    }

    static void ChangeThemeColor()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Change Theme Color ===");
        WriteLineCenter("1. Cyan (Default)");
        WriteLineCenter("2. Red");
        WriteLineCenter("3. Green");
        WriteLineCenter("4. Yellow");
        WriteLineCenter("5. Purple");
        Console.WriteLine();
        WriteCenter("Choose color: ");
        if (int.TryParse(Console.ReadLine(), out int choice))
        {
            defaultColor = choice switch
            {
                2 => ConsoleColor.Red,
                3 => ConsoleColor.Green,
                4 => ConsoleColor.Yellow,
                5 => ConsoleColor.Magenta,
                _ => ConsoleColor.Cyan
            };
        }
    }

    static async Task ChangeStatus()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Change Status ===");
        WriteLineCenter("1. Online");
        WriteLineCenter("2. Idle");
        WriteLineCenter("3. Do Not Disturb");
        WriteLineCenter("4. Invisible");
        Console.WriteLine();
        WriteCenter("Choose status: ");
        if (int.TryParse(Console.ReadLine(), out int choice))
        {
            string status = choice switch
            {
                1 => "online",
                2 => "idle",
                3 => "dnd",
                4 => "invisible",
                _ => "online"
            };

            var statusData = new
            {
                status = status,
                since = 0,
                activities = new object[] { },
                afk = false
            };

            try
            {
                SimulateProgress("Updating status...");
                var result = await MakeDiscordRequest("/users/@me/settings", HttpMethod.Patch, statusData);
                
                if (result.HasValue)
                {
                    Console.WriteLine("Status updated successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to update status. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating status: {ex.Message}");
            }
        }
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static async Task SecuritySettings()
    {
        while (true)
        {
            DisplayBanner();
            Console.WriteLine();
            WriteLineCenter("=== Security Settings ===");
            WriteLineCenter("1. Change Password");
            WriteLineCenter("2. Enable/Disable 2FA");
            WriteLineCenter("3. Get Backup Codes");
            WriteLineCenter("4. Return");
            Console.WriteLine();
            WriteCenter("Choose option: ");
            
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                switch (choice)
                {
                    case 1: await ChangePassword(); break;
                    case 2: await ManageTwoFactor(); break;
                    case 3: await GetBackupCodes(); break;
                    case 4: return;
                }
            }
        }
    }

    private static async Task<JsonElement?> MakeDiscordRequest(string endpoint, HttpMethod method, object data = null)
    {
        try
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(mainToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            HttpResponseMessage response;
            if (method == HttpMethod.Get)
            {
                response = await client.GetAsync($"{API_BASE}{endpoint}");
            }
            else
            {
                var content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json");
                response = method switch
                {
                    HttpMethod m when m == HttpMethod.Post => await client.PostAsync($"{API_BASE}{endpoint}", content),
                    HttpMethod m when m == HttpMethod.Patch => await client.PatchAsync($"{API_BASE}{endpoint}", content),
                    _ => await client.DeleteAsync($"{API_BASE}{endpoint}")
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<JsonElement>(responseContent);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                Console.WriteLine($"Details: {responseContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    private static void ViewSavedTokens()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Saved Tokens ===");
        if (savedTokens.Count == 0)
        {
            Console.WriteLine("No tokens saved.");
        }
        else
        {
            for (int i = 0; i < savedTokens.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {savedTokens[i].Substring(0, 20)}...");
            }
        }
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static void AddNewToken()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Add New Token ===");
        WriteCenter("Enter token: ");
        string token = Console.ReadLine();
        if (!string.IsNullOrEmpty(token))
        {
            savedTokens.Add(token);
            WriteLineCenter("Token added successfully!");
        }
        Thread.Sleep(1000);
    }

    private static void RemoveToken()
    {
        DisplayBanner();
        ViewSavedTokens();
        if (savedTokens.Count > 0)
        {
            Console.Write("\nEnter number to remove (0 to cancel): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= savedTokens.Count)
            {
                savedTokens.RemoveAt(choice - 1);
                Console.WriteLine("Token removed successfully!");
            }
        }
        Thread.Sleep(1000);
    }

    private static async Task ValidateTokens()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Validate Tokens ===");
        foreach (var token in savedTokens)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
                var response = await client.GetAsync($"{API_BASE}/users/@me");
                Console.WriteLine($"{token.Substring(0, 20)}... : {(response.IsSuccessStatusCode ? "Valid" : "Invalid")}");
            }
            catch
            {
                Console.WriteLine($"{token.Substring(0, 20)}... : Invalid");
            }
        }
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task SendMessage()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Send Message ===");
        WriteCenter("Enter Channel ID: ");
        string channelId = Console.ReadLine();
        
        if (string.IsNullOrEmpty(channelId))
        {
            WriteLineCenter("Channel ID cannot be empty!");
            Thread.Sleep(1500);
            return;
        }

        WriteCenter("Enter message: ");
        string message = Console.ReadLine();

        if (string.IsNullOrEmpty(message))
        {
            WriteLineCenter("Message cannot be empty!");
            Thread.Sleep(1500);
            return;
        }

        try
        {
            var messageData = new { content = message };
            var result = await MakeDiscordRequest($"/channels/{channelId}/messages", HttpMethod.Post, messageData);
            
            if (result.HasValue)
            {
                WriteLineCenter("Message sent successfully! ✓");
            }
            else
            {
                WriteLineCenter("Failed to send message ✗");
            }
        }
        catch (Exception ex)
        {
            WriteLineCenter($"Error: {ex.Message}");
        }

        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task ViewMessageHistory()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Message History ===");
        WriteCenter("Enter Channel ID: ");
        string channelId = Console.ReadLine();
        
        if (string.IsNullOrEmpty(channelId))
        {
            WriteLineCenter("Channel ID cannot be empty!");
            Thread.Sleep(1500);
            return;
        }

        try
        {
            SimulateProgress("Fetching messages...");
            var result = await MakeDiscordRequest($"/channels/{channelId}/messages?limit=50", HttpMethod.Get);
            
            if (result.HasValue)
            {
                Console.WriteLine();
                const int boxWidth = 60;
                string horizontalLine = new string('═', boxWidth - 2);
                string emptyLine = $"║{new string(' ', boxWidth - 2)}║";

                WriteLineCenter($"╔{horizontalLine}╗");
                WriteLineCenter($"║{CenterText("Recent Messages", boxWidth - 2)}║");
                WriteLineCenter($"╠{horizontalLine}╣");

                foreach (var message in result.Value.EnumerateArray())
                {
                    string author = message.GetProperty("author").GetProperty("username").GetString();
                    string content = message.GetProperty("content").GetString();
                    string timestamp = message.GetProperty("timestamp").GetString();

                    WriteLineCenter($"║ Author: {author.PadRight(boxWidth - 10)}║");
                    WriteLineCenter($"║ Time: {timestamp.PadRight(boxWidth - 8)}║");
                    WriteLineCenter($"║ Content: {content.PadRight(boxWidth - 11)}║");
                    WriteLineCenter($"╟{new string('─', boxWidth - 2)}╢");
                }

                WriteLineCenter($"╚{horizontalLine}╝");
            }
            else
            {
                WriteLineCenter("No messages found or access denied.");
            }
        }
        catch (Exception ex)
        {
            WriteLineCenter($"Error: {ex.Message}");
        }

        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task SendDirectMessage()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Send Direct Message ===");
        WriteCenter("Enter User ID: ");
        string userId = Console.ReadLine();
        
        if (string.IsNullOrEmpty(userId))
        {
            WriteLineCenter("User ID cannot be empty!");
            Thread.Sleep(1500);
            return;
        }

        WriteCenter("Enter message: ");
        string message = Console.ReadLine();

        if (string.IsNullOrEmpty(message))
        {
            WriteLineCenter("Message cannot be empty!");
            Thread.Sleep(1500);
            return;
        }

        try
        {
            // First create a DM channel
            var channelData = new { recipient_id = userId };
            var dmChannel = await MakeDiscordRequest("/users/@me/channels", HttpMethod.Post, channelData);
            
            if (dmChannel.HasValue)
            {
                var channelId = dmChannel.Value.GetProperty("id").GetString();
                
                // Then send the message
                var messageData = new { content = message };
                var result = await MakeDiscordRequest($"/channels/{channelId}/messages", HttpMethod.Post, messageData);
                
                if (result.HasValue)
                {
                    WriteLineCenter("Direct message sent successfully! ✓");
                }
                else
                {
                    WriteLineCenter("Failed to send direct message ✗");
                }
            }
            else
            {
                WriteLineCenter("Couldn't create DM channel with user ✗");
            }
        }
        catch (Exception ex)
        {
            WriteLineCenter($"Error: {ex.Message}");
        }

        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task SendMassDM()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Mass DM Tools ===");
        WriteCenter("Enter message to send: ");
        string message = Console.ReadLine();
        WriteCenter("Enter user IDs (comma-separated): ");
        string userIds = Console.ReadLine();

        if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(userIds))
        {
            WriteLineCenter("Message or user IDs cannot be empty!");
            Thread.Sleep(1500);
            return;
        }

        var ids = userIds.Split(',').Select(id => id.Trim()).ToList();
        int success = 0, failed = 0;
        
        WriteLineCenter("\nStarting Mass DM...");
        Console.WriteLine();

        foreach (var userId in ids)
        {
            try
            {
                // Create DM channel
                var channelData = new { recipient_id = userId };
                var dmChannel = await MakeDiscordRequest("/users/@me/channels", HttpMethod.Post, channelData);
                
                if (dmChannel.HasValue)
                {
                    var channelId = dmChannel.Value.GetProperty("id").GetString();
                    
                    // Send message
                    var messageData = new { content = message };
                    var result = await MakeDiscordRequest($"/channels/{channelId}/messages", HttpMethod.Post, messageData);
                    
                    if (result.HasValue)
                    {
                        success++;
                        WriteLineCenter($"Message sent to ID: {userId} ✓");
                    }
                    else
                    {
                        failed++;
                        WriteLineCenter($"Failed to send to ID: {userId} ✗");
                    }
                    
                    // Add delay to avoid rate limits
                    await Task.Delay(1000);
                }
                else
                {
                    failed++;
                    WriteLineCenter($"Couldn't create DM channel with ID: {userId} ✗");
                }
            }
            catch (Exception ex)
            {
                failed++;
                WriteLineCenter($"Error with ID {userId}: {ex.Message} ✗");
            }
        }

        Console.WriteLine();
        WriteLineCenter($"Mass DM Complete!");
        WriteLineCenter($"Successfully sent: {success}");
        WriteLineCenter($"Failed: {failed}");
        WriteLineCenter($"Total attempted: {ids.Count}");
        
        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task FriendRequestSpam()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Friend Request Spam ===");
        WriteLineCenter("Make sure you have added tokens to spam.txt!");
        Console.WriteLine();
        WriteCenter("Enter user ID: ");
        string userId = Console.ReadLine();
        WriteCenter("Enter number of requests per token (1-100): ");
        int count = int.TryParse(Console.ReadLine(), out int c) ? Math.Min(c, 100) : 1;

        if (string.IsNullOrEmpty(userId))
        {
            WriteLineCenter("Invalid user ID!");
            Thread.Sleep(1500);
            return;
        }

        // Read tokens from spam.txt
        if (!File.Exists("spam.txt"))
        {
            WriteLineCenter("spam.txt not found! Creating file...");
            File.WriteAllText("spam.txt", "// Enter your tokens here (one per line)");
            WriteLineCenter("Please add tokens to spam.txt and try again!");
            Thread.Sleep(2000);
            return;
        }

        var tokens = File.ReadAllLines("spam.txt")
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//"))
            .ToList();

        if (tokens.Count == 0)
        {
            WriteLineCenter("No tokens found in spam.txt!");
            WriteLineCenter("Please add tokens to spam.txt (one per line)");
            Thread.Sleep(2000);
            return;
        }

        WriteLineCenter($"\nFound {tokens.Count} tokens in spam.txt");
        WriteLineCenter("\nStarting Friend Request Spam...");
        Console.WriteLine();

        int totalSuccess = 0;
        int totalFailed = 0;

        foreach (var token in tokens)
        {
            WriteLineCenter($"Using token: {token.Substring(0, 20)}...");
            
            for (int i = 0; i < count; i++)
            {
                try
                {
                    // Set up the request
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.Trim());
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                    // Send friend request
                    var response = await client.PutAsync(
                        $"{API_BASE}/users/@me/relationships/{userId}",
                        new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
                    );

                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        totalSuccess++;
                        WriteLineCenter($"Friend request sent ({i + 1}/{count}) ✓");
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        totalFailed++;
                        WriteLineCenter($"Failed to send request ({i + 1}/{count}) ✗ - {response.StatusCode}");
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            WriteLineCenter($"Error: {errorMessage}");
                        }
                    }

                    // Add delay to avoid rate limits
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    totalFailed++;
                    WriteLineCenter($"Error sending request: {ex.Message} ✗");
                }
            }
            
            WriteLineCenter("Switching to next token...");
            Console.WriteLine();
        }

        Console.WriteLine();
        WriteLineCenter($"Friend Request Spam Complete!");
        WriteLineCenter($"Successfully sent: {totalSuccess}");
        WriteLineCenter($"Failed: {totalFailed}");
        WriteLineCenter($"Total attempted: {tokens.Count * count}");
        WriteLineCenter($"Tokens used: {tokens.Count}");
        
        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task ClearDMs()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Clear DMs ===");
        SimulateProgress("Clearing DMs...");
        // Implementation here
        Console.WriteLine("DMs cleared!");
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task ServerNuker()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Server Nuker ===");
        WriteCenter("Enter Server ID: ");
        string serverId = Console.ReadLine();
        if (!string.IsNullOrEmpty(serverId))
        {
            SimulateProgress("Nuking server...");
            // Implementation here
            Console.WriteLine("Server nuked!");
        }
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task MassBan()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Mass Ban ===");
        Console.Write("Enter Server ID: ");
        string serverId = Console.ReadLine();
        if (!string.IsNullOrEmpty(serverId))
        {
            SimulateProgress("Banning members...");
            // Implementation here
            Console.WriteLine("Mass ban complete!");
        }
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task MassChannelCreate()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Mass Channel Create ===");
        Console.Write("Enter Server ID: ");
        string serverId = Console.ReadLine();
        if (!string.IsNullOrEmpty(serverId))
        {
            SimulateProgress("Creating channels...");
            // Implementation here
            Console.WriteLine("Channels created!");
        }
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task MassRoleCreate()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Mass Role Create ===");
        Console.Write("Enter Server ID: ");
        string serverId = Console.ReadLine();
        if (!string.IsNullOrEmpty(serverId))
        {
            SimulateProgress("Creating roles...");
            // Implementation here
            Console.WriteLine("Roles created!");
        }
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task TokenChecker()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Token Checker ===");
        SimulateProgress("Checking tokens...");
        // Implementation here
        Console.WriteLine("Token check complete!");
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task AccountGenerator()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Account Generator ===");
        SimulateProgress("Generating accounts...");
        // Implementation here
        Console.WriteLine("Accounts generated!");
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task EmailVerifier()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Email Verifier ===");
        SimulateProgress("Verifying emails...");
        // Implementation here
        Console.WriteLine("Emails verified!");
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task TokenGenerator()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Token Generator ===");
        SimulateProgress("Generating tokens...");
        // Implementation here
        Console.WriteLine("Tokens generated!");
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task WebhookSpammer()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Webhook Spammer ===");
        WriteCenter("Enter webhook URL: ");
        string webhook = Console.ReadLine();
        if (!string.IsNullOrEmpty(webhook))
        {
            SimulateProgress("Spamming webhook...");
            // Implementation here
            Console.WriteLine("Webhook spam complete!");
        }
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task ServerLookup()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Server Lookup ===");
        Console.Write("Enter Server ID: ");
        string serverId = Console.ReadLine();
        if (!string.IsNullOrEmpty(serverId))
        {
            SimulateProgress("Looking up server...");
            // Implementation here
            Console.WriteLine("Server lookup complete!");
        }
        Console.WriteLine();
        WriteCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static void ToggleAnimations()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Toggle Animations ===");
        useAnimations = !useAnimations;
        Console.WriteLine();
        WriteLineCenter($"Animations have been {(useAnimations ? "enabled" : "disabled")}! ✓");
        Thread.Sleep(1500);
    }

    private static void ClearSavedData()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Clear Saved Data ===");
        WriteCenter("Are you sure? This will remove all saved tokens (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            savedTokens.Clear();
            if (File.Exists("token.txt")) File.Delete("token.txt");
            WriteLineCenter("All saved data cleared!");
        }
        Thread.Sleep(1000);
    }

    private static void ShowAbout()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== About RT-TOOL ===");
        WriteLineCenter("Version: 1.0.0");
        WriteLineCenter("Created by: Hasan");
        Console.WriteLine();
        WriteLineCenter("A powerful Discord account management tool.");
        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task ChangePassword()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Change Password ===");
        WriteCenter("Enter current password: ");
        string currentPass = Console.ReadLine();
        WriteCenter("Enter new password: ");
        string newPass = Console.ReadLine();
        WriteCenter("Confirm new password: ");
        string confirmPass = Console.ReadLine();

        if (string.IsNullOrEmpty(currentPass) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirmPass))
        {
            WriteLineCenter("All fields are required!");
            Thread.Sleep(1500);
            return;
        }

        if (newPass != confirmPass)
        {
            WriteLineCenter("New passwords don't match!");
            Thread.Sleep(1500);
            return;
        }

        try
        {
            // Set up headers
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(mainToken);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            var passwordData = new
            {
                password = currentPass,
                new_password = newPass
            };

            var jsonContent = JsonSerializer.Serialize(passwordData);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PatchAsync($"{API_BASE}/users/@me", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (responseContent.Contains("Two factor is required"))
            {
                var mfaData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                string ticket = mfaData.GetProperty("mfa").GetProperty("ticket").GetString();

                WriteLineCenter("\nTwo-factor authentication required.");
                WriteCenter("Enter your password for 2FA verification: ");
                string password2fa = Console.ReadLine();

                if (!string.IsNullOrEmpty(password2fa))
                {
                    var twoFactorData = new
                    {
                        password = password2fa,
                        ticket = ticket,
                        new_password = newPass  // Include new password in 2FA request
                    };

                    var mfaContent = new StringContent(
                        JsonSerializer.Serialize(twoFactorData),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    response = await client.PostAsync($"{API_BASE}/users/@me/mfa/password", mfaContent);
                    responseContent = await response.Content.ReadAsStringAsync();
                }
            }

            if (response.IsSuccessStatusCode)
            {
                WriteLineCenter("\nPassword changed successfully! ✓");
                WriteLineCenter("Please log in again with your new password.");
                mainToken = ""; // Force re-login
                File.Delete("token.txt"); // Remove saved token
            }
            else
            {
                WriteLineCenter($"\nFailed to change password! ✗");
                WriteLineCenter($"Error: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            WriteLineCenter($"\nError: {ex.Message}");
        }

        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task ManageTwoFactor()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Two-Factor Authentication ===");
        
        try
        {
            // First check current 2FA status
            var userInfo = await MakeDiscordRequest("/users/@me", HttpMethod.Get);
            bool has2FA = userInfo.HasValue && userInfo.Value.GetProperty("mfa_enabled").GetBoolean();

            if (has2FA)
            {
                WriteLineCenter("2FA is currently enabled.");
                WriteLineCenter("1. Disable 2FA");
                WriteLineCenter("2. Return");
            }
            else
            {
                WriteLineCenter("2FA is currently disabled.");
                WriteLineCenter("1. Enable 2FA");
                WriteLineCenter("2. Return");
            }

            Console.WriteLine();
            WriteCenter("Choose option: ");
            
            if (int.TryParse(Console.ReadLine(), out int choice) && choice == 1)
            {
                if (has2FA)
                {
                    // Disable 2FA
                    WriteCenter("Enter your 2FA code: ");
                    string code = Console.ReadLine();
                    
                    var disableData = new { code = code };
                    var result = await MakeDiscordRequest("/users/@me/mfa/totp/disable", HttpMethod.Post, disableData);
                    
                    if (result.HasValue)
                    {
                        WriteLineCenter("2FA disabled successfully! ✓");
                    }
                    else
                    {
                        WriteLineCenter("Failed to disable 2FA! ✗");
                    }
                }
                else
                {
                    // First get password for verification
                    WriteCenter("Enter your password: ");
                    string password = Console.ReadLine();

                    if (string.IsNullOrEmpty(password))
                    {
                        WriteLineCenter("Password is required!");
                        Thread.Sleep(1500);
                        return;
                    }

                    // Enable 2FA with password
                    var enableData = new { password = password };
                    var result = await MakeDiscordRequest("/users/@me/mfa/totp/enable", HttpMethod.Post, enableData);
                    
                    if (result.HasValue)
                    {
                        string secret = result.Value.GetProperty("secret").GetString();
                        string qrCode = result.Value.GetProperty("qr_code").GetString();
                        
                        WriteLineCenter("\n2FA Setup Instructions:");
                        WriteLineCenter("1. Open your authenticator app");
                        WriteLineCenter($"2. Enter this secret key: {secret}");
                        WriteLineCenter("3. Or scan the QR code (saved to qr.png)");
                        
                        // Save QR code if provided
                        if (!string.IsNullOrEmpty(qrCode))
                        {
                            File.WriteAllBytes("qr.png", Convert.FromBase64String(qrCode));
                        }

                        WriteCenter("\nEnter the 6-digit code from your authenticator: ");
                        string code = Console.ReadLine();
                        
                        var confirmData = new 
                        { 
                            code = code,
                            password = password  // Include password in confirmation
                        };
                        var confirmResult = await MakeDiscordRequest("/users/@me/mfa/totp/enable", HttpMethod.Post, confirmData);
                        
                        if (confirmResult.HasValue)
                        {
                            WriteLineCenter("2FA enabled successfully! ✓");
                            WriteLineCenter("Make sure to save your backup codes!");
                        }
                        else
                        {
                            WriteLineCenter("Failed to enable 2FA! ✗");
                        }
                    }
                    else
                    {
                        WriteLineCenter("Failed to start 2FA setup! ✗");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            WriteLineCenter($"Error: {ex.Message}");
        }

        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task GetBackupCodes()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Backup Codes ===");
        
        try
        {
            var result = await MakeDiscordRequest("/users/@me/mfa/codes", HttpMethod.Post);
            
            if (result.HasValue)
            {
                var codes = result.Value.GetProperty("backup_codes").EnumerateArray();
                
                Console.WriteLine();
                WriteLineCenter("Your Backup Codes:");
                Console.WriteLine();
                
                foreach (var code in codes)
                {
                    WriteLineCenter(code.GetProperty("code").GetString());
                }
                
                WriteLineCenter("\nSave these codes in a safe place!");
                WriteLineCenter("Each code can only be used once.");
            }
            else
            {
                WriteLineCenter("Failed to get backup codes! ✗");
                WriteLineCenter("Make sure 2FA is enabled first.");
            }
        }
        catch (Exception ex)
        {
            WriteLineCenter($"Error: {ex.Message}");
        }

        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static void ShowLoadingAnimation(string message, int duration = 1000)
    {
        if (!useAnimations) return;
        
        string[] frames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
        int frameCount = frames.Length;
        int delay = 50;
        int iterations = duration / (delay * frameCount);
        int spaces = (Console.WindowWidth - (message.Length + 2)) / 2;

        Console.Write(new string(' ', spaces) + message);
        
        for (int i = 0; i < iterations * frameCount; i++)
        {
            Console.Write($"\r{new string(' ', spaces)}{message} {frames[i % frameCount]}");
            Thread.Sleep(delay);
        }
        Console.WriteLine();
    }

    private static void ShowSuccessAnimation(string message)
    {
        if (!useAnimations) 
        {
            Console.WriteLine(message);
            return;
        }

        Console.Write("\r");
        string[] frames = { "⣾", "⣽", "", "⢿", "⡿", "⣟", "⣯", "⣷" };
        
        // Loading animation
        for (int i = 0; i < 20; i++)
        {
            Console.Write($"\r{frames[i % frames.Length]} {message}");
            Thread.Sleep(50);
        }
        
        // Success animation
        Console.Write($"\r✓ {message}");
        Console.WriteLine();
    }

    private static void ShowTypingAnimation(string text, int delay = 30)
    {
        if (!useAnimations)
        {
            Console.WriteLine(text);
            return;
        }

        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep(delay);
        }
        Console.WriteLine();
    }

    private static void ShowBannerAnimation()
    {
        Console.Clear();
        Console.ForegroundColor = defaultColor;

        string banner = @"
:::::::::  :::::::::::::      ::::::::::: ::::::::   ::::::::  :::        
:+:    :+:     :+:               :+:    :+:    :+: :+:    :+: :+:        
+:+    +:+     +:+               +:+    +:+    +:+ +:+    +:+ +:+        
+#++:++#:      +#+     +#++:+    +#+    +#+    +:+ +#+    +:+ +#+        
+#+    +#+     +#+               +#+    +#+    +#+ +#+    +#+ +#+        
#+#    #+#     #+#               #+#    #+#    #+# #+#    #+# #+#        
###    ###     ###               ###     ########   ########  ########## 
                        RT-TOOL Advanced Edition";

        string[] lines = banner.Split('\n');
        foreach (string line in lines)
        {
            WriteLineCenter(line);
            if (useAnimations) Thread.Sleep(50);
        }

        Console.WriteLine();
        
        if (!string.IsNullOrEmpty(username))
        {
            string separator = "+---------------------------+";
            string userLine = $"| Logged in as: {username}{new string(' ', 20 - username.Length)}|";
            
            WriteLineCenter(separator);
            WriteLineCenter(userLine);
            WriteLineCenter(separator);
        }
        
        Console.ResetColor();
    }

    private static void ShowHackerAnimation()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        // Get console dimensions for centering
        int windowWidth = Console.WindowWidth;
        int windowHeight = Console.WindowHeight;
        
        // HACKED ASCII art
        string[] hackedArt = {
            " ██░ ██  ▄▄▄       ▄████▄   ██ ▄█▀▓█████ ▓█████▄ ",
            "▓██░ ██▒▒████▄    ▒██▀ ▀█   ██▄█▒ ▓█   ▀ ▒██▀ ██▌",
            "▒██▀▀██░▒██  ▀█▄  ▒▓█    ▄ ▓███▄░ ▒███   ░██   █▌",
            "░▓█ ░██ ░██▄▄▄▄██ ▒▓▓▄ ▄██▒▓██ █▄ ▒▓█  ▄ ░▓█▄   ▌",
            "░▓█▒░██▓ ▓█   ▓██▒▒ ▓███▀ ░▒██▒ █▄░▒████▒░▒████▓ ",
            " ▒ ░▒░ ░▒ ▒▒   ▓▒█░░ ░▒ ▒  ░▒ ▒▒ ▓▒░░ ▒░ ░ ▒▒▓  ▒ ",
            " ▒ ░▒░ ░  ▒   ▒▒ ░  ░  ▒   ░ ░▒ ▒░ ░ ░  ░ ░ ▒  ▒ ",
            " ░  ░░ ░  ░   ▒   ░        ░ ░░ ░    ░    ░ ░  ░ ",
            " ░  ░  ░      ░  ░░ ░      ░  ░      ░  ░   ░    ",
            "                   ░                       ░      "
        };

        // Calculate position for centering
        int artHeight = hackedArt.Length;
        int artWidth = hackedArt[0].Length;
        int topMargin = (windowHeight - artHeight) / 2;
        int leftMargin = (windowWidth - artWidth) / 2;

        // Flash animation
        for (int i = 0; i < 3; i++)
        {
            Console.Clear();
            Thread.Sleep(200);
            
            // Draw centered HACKED
            Console.SetCursorPosition(0, topMargin - 1);
            foreach (string line in hackedArt)
            {
                Console.SetCursorPosition(leftMargin, Console.CursorTop);
                Console.WriteLine(line);
            }
            Thread.Sleep(200);
        }

        // Show "by Hasan" with typing effect
        Console.SetCursorPosition(leftMargin + (artWidth - 11) / 2, topMargin + artHeight + 2);
        string byText = "by Hasan :)";
        foreach (char c in byText)
        {
            Console.Write(c);
            Thread.Sleep(100);
        }
        Thread.Sleep(1000);

        // Matrix effect with Islamic text
        Console.Clear();
        Random random = new Random();
        string islamicText = "لا إله إلا الله محمد رسول الله";
        
        // Create box for Islamic text
        const int boxWidth = 50;
        string horizontalLine = new string('═', boxWidth - 2);
        string emptyLine = $"║{new string(' ', boxWidth - 2)}║";

        // Calculate center for box
        int boxCenterX = (windowWidth - boxWidth) / 2;
        int boxCenterY = windowHeight / 2 - 2;

        // Matrix rain effect with Islamic text typing
        int charIndex = 0;
        while (charIndex <= islamicText.Length)
        {
            Console.Clear();
            
            // Draw box
            Console.SetCursorPosition(boxCenterX, boxCenterY);
            Console.Write($"╔{horizontalLine}╗");
            Console.SetCursorPosition(boxCenterX, boxCenterY + 1);
            Console.Write(emptyLine);
            Console.SetCursorPosition(boxCenterX, boxCenterY + 2);
            Console.Write($"║{CenterText(islamicText.Substring(0, charIndex), boxWidth - 2)}║");
            Console.SetCursorPosition(boxCenterX, boxCenterY + 3);
            Console.Write(emptyLine);
            Console.SetCursorPosition(boxCenterX, boxCenterY + 4);
            Console.Write($"╚{horizontalLine}╝");
            
            // Matrix rain effect around the box
            for (int x = 0; x < windowWidth; x++)
            {
                if (random.Next(20) == 0)
                {
                    int y = random.Next(windowHeight);
                    // Skip the area where the box appears
                    if (y < boxCenterY || y > boxCenterY + 4)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(random.Next(2));
                    }
                }
            }

            if (charIndex < islamicText.Length)
            {
                charIndex++;
                Thread.Sleep(150); // Slower typing for Arabic text
            }
            else
            {
                Thread.Sleep(2000); // Hold final text
                break;
            }
        }

        // Clear and reset
        Console.Clear();
        Console.ResetColor();
    }

    private static void WriteLineCenter(string text)
    {
        int spaces = (Console.WindowWidth - text.Length) / 2;
        Console.WriteLine(new string(' ', spaces) + text);
    }

    private static void WriteCenter(string text)
    {
        int spaces = (Console.WindowWidth - text.Length) / 2;
        Console.Write(new string(' ', spaces) + text);
    }

    private static string CenterText(string text, int width)
    {
        int spaces = width - text.Length;
        int padLeft = spaces / 2 + text.Length;
        return text.PadLeft(padLeft).PadRight(width);
    }

    private static void CheckAndCreateSpamFile()
    {
        string spamFile = "spam.txt";
        if (!File.Exists(spamFile))
        {
            File.WriteAllText(spamFile, "// Enter your tokens here (one per line)");
            WriteLineCenter("Created spam.txt file for tokens!");
            WriteLineCenter("Please add your tokens to spam.txt (one per line)");
            Thread.Sleep(2000);
        }
    }

    private static async Task ShowMemberCount()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Member Count ===");
        WriteCenter("Enter Server ID: ");
        string serverId = Console.ReadLine();
        
        if (!string.IsNullOrEmpty(serverId))
        {
            try
            {
                var result = await MakeDiscordRequest($"/guilds/{serverId}", HttpMethod.Get);
                if (result.HasValue)
                {
                    int memberCount = result.Value.GetProperty("approximate_member_count").GetInt32();
                    int onlineCount = result.Value.GetProperty("approximate_presence_count").GetInt32();
                    
                    Console.WriteLine();
                    WriteLineCenter($"Total Members: {memberCount}");
                    WriteLineCenter($"Online Members: {onlineCount}");
                }
            }
            catch (Exception ex)
            {
                WriteLineCenter($"Error: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task ShowChannelList()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Channel List ===");
        WriteCenter("Enter Server ID: ");
        string serverId = Console.ReadLine();
        
        if (!string.IsNullOrEmpty(serverId))
        {
            try
            {
                var result = await MakeDiscordRequest($"/guilds/{serverId}/channels", HttpMethod.Get);
                if (result.HasValue)
                {
                    Console.WriteLine();
                    WriteLineCenter("Channels:");
                    foreach (var channel in result.Value.EnumerateArray())
                    {
                        string name = channel.GetProperty("name").GetString();
                        string type = channel.GetProperty("type").GetInt32() switch
                        {
                            0 => "Text",
                            2 => "Voice",
                            4 => "Category",
                            _ => "Other"
                        };
                        WriteLineCenter($"• {name} ({type})");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLineCenter($"Error: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task ShowRoleList()
    {
        DisplayBanner();
        Console.WriteLine();
        WriteLineCenter("=== Role List ===");
        WriteCenter("Enter Server ID: ");
        string serverId = Console.ReadLine();
        
        if (!string.IsNullOrEmpty(serverId))
        {
            try
            {
                var result = await MakeDiscordRequest($"/guilds/{serverId}/roles", HttpMethod.Get);
                if (result.HasValue)
                {
                    Console.WriteLine();
                    WriteLineCenter("Roles:");
                    foreach (var role in result.Value.EnumerateArray())
                    {
                        string name = role.GetProperty("name").GetString();
                        WriteLineCenter($"• {name}");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLineCenter($"Error: {ex.Message}");
            }
        }
        
        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }

    private static void ShowDisclaimer()
    {
        Console.Clear();
        DisplayBanner();
        Console.WriteLine();
        
        const int boxWidth = 70;
        string horizontalLine = new string('═', boxWidth - 2);
        string emptyLine = $"║{new string(' ', boxWidth - 2)}║";

        WriteLineCenter($"╔{horizontalLine}╗");
        WriteLineCenter($"║{CenterText("DISCLAIMER", boxWidth - 2)}║");
        WriteLineCenter($"╠{horizontalLine}╣");
        WriteLineCenter(emptyLine);
        WriteLineCenter($"║  WARNING: This tool comes with no warranty or guarantees.                ║");
        WriteLineCenter($"║  Using this tool may violate Discord's Terms of Service.                ║");
        WriteLineCenter($"║  The author takes no responsibility for any consequences of using       ║");
        WriteLineCenter($"║  this tool, including but not limited to account bans or suspensions.   ║");
        WriteLineCenter(emptyLine);
        WriteLineCenter($"║  Use at your own risk!                                                  ║");
        WriteLineCenter(emptyLine);
        WriteLineCenter($"╚{horizontalLine}╝");
        
        Console.WriteLine();
        WriteLineCenter("Press any key to continue...");
        Console.ReadKey();
    }
}