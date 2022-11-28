using OneBot.CommandRoute.Services;
using Sora.Entities.Base;
using Sora.EventArgs.SoraEvent;
using System.Collections.Concurrent;
using System.Text.Json;
using Intallk.Models;
using Microsoft.Net.Http.Headers;

namespace Intallk.Modules;


[ModuleInformation(HelpCmd = "dproblem", ModuleName = "力扣每日一问", ModuleUsage = "为群里推送每日力扣问题。（感谢TLMegalovania的贡献！！）")]
public class DailyProblem : IHostedService
{
    private readonly HttpClient client;
    private readonly ILogger<DailyProblem> logger;
    private readonly System.Timers.Timer timer;
    private readonly ConcurrentDictionary<long, SoraApi> apiManager;
    public PermissionService PermissionService;
    public DailyProblem(IHttpClientFactory factory, ILogger<DailyProblem> logger, ICommandService commandService, PermissionService permissionService)
    {
        client = factory.CreateClient("leetcode");
        this.logger = logger;
        timer = new(TimeSpan.FromDays(1).TotalMilliseconds);
        apiManager = new();
        commandService.Event.OnClientConnect += (context) =>
            {
                var args = context.WrapSoraEventArgs<ConnectEventArgs>();
                apiManager.TryAdd(args.LoginUid, args.SoraApi);
                return 0;
            };
        commandService.Event.OnClientStatusChangeEvent += (context) =>
            {
                var args = context.WrapSoraEventArgs<ClientStatusChangeEventArgs>();
                if (args.Online)
                {
                    apiManager.TryAdd(args.LoginUid, args.SoraApi);
                }
                else
                {
                    apiManager.TryRemove(args.LoginUid, out _);
                }
                return 0;
            };
        this.PermissionService = permissionService;
    }
    readonly static Dictionary<string, string> Mapper = new()
    {
        ["<code>"] = "",
        ["</code>"] = "",
        ["<p>"] = "",
        ["</p>"] = "",
        ["<em>"] = "",
        ["</em>"] = "",
        ["<pre>"] = "",
        ["</pre>"] = "",
        ["<strong>"] = "",
        ["</strong>"] = "",
        ["<li>"] = "",
        ["</li>"] = "",
        ["<ul>"] = "",
        ["</ul>"] = "",
        ["<sup>"] = "^",
        ["</sup>"] = "",
        ["<ol>"] = "",
        ["</ol>"] = ""
    };
    async Task FetchDaily()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent("{\"query\":\"query questionOfToday{todayRecord{question{questionTitleSlug}}}\",\"variables\":{}}")
        };
        var response = await client.SendAsync(request);
        string? title = JsonDocument.Parse(response.Content.ReadAsStream()).RootElement.GetProperty("data").GetProperty("todayRecord")[0].GetProperty("question").GetProperty("questionTitleSlug").GetString();
        if (title is null)
        {
            logger.LogWarning("daily problem service gets null title");
            return;
        }
        request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = new StringContent($"{{\"query\":\"query{{question(titleSlug: \"{title}\"){{questionId translatedTitle translatedContent difficulty}}}}\",\"variables\":{{}}}}")
        };
        response = await client.SendAsync(request);
        var question = JsonDocument.Parse(response.Content.ReadAsStream()).RootElement.GetProperty("data").GetProperty("question");
        string? content = question.GetProperty("translatedContent").GetString();
        if (content is null)
        {
            logger.LogWarning("daily problem service gets null content");
            return;
        }
        foreach (var pair in Mapper)
        {
            content = content.Replace(pair.Key, pair.Value);
        }
        string message = $"{question.GetProperty("questionId").GetString()}. {question.GetProperty("translatedTitle").GetString()}\r\n难度: {question.GetProperty("difficulty").GetString()}\r\n{content}";
        foreach (var api in apiManager.Values)
        {
            foreach (var group in (await api.GetGroupList()).groupList)
            {
                if (PermissionService.JudgeGroup(group.GroupId, "LEETCODETODAY_PUSH", Models.PermissionPolicy.RequireAccepted))
                {
                    await api.SendGroupMessage(group.GroupId, message);
                    Thread.Sleep(1000);
                }     
            }
        }
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("leetcode daily problem service started");
        timer.Elapsed += (_, _) => _ = FetchDaily();
        timer.Enabled = true;
        _ = FetchDaily();
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("leetcode daily problem service stopped");
        return Task.CompletedTask;
    }
}