using Intallk.Config;
using Intallk.Models;
using Intallk.Modules;

using OneBot.CommandRoute.Configuration;
using OneBot.CommandRoute.Mixin;
using OneBot.CommandRoute.Models.VO;
using OneBot.CommandRoute.Services;
using static Intallk.Models.DictionaryReplyModel;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureOneBotHost();

builder.ConfigureServices((context, services) =>
{
    IConfiguration configuration = context.Configuration;

    // ���û����˺���
    // ���� OneBot ����
    services.Configure<CQHttpServerConfigModel>(configuration.GetSection("CQHttpConfig"));
    services.ConfigureOneBot();

    // ���ָ�� / �¼�
    // �Ƽ�ʹ�õ���ģʽ����ʵ���Ͽ�ܴ���Ҳ�ǵ�����ģʽʹ�õģ�
    services.AddSingleton<IOneBotController, MainModule>()
        .AddSingleton<IOneBotController, GIFProcess>()
        .AddSingleton<IOneBotController, Testing>()
        .AddSingleton<IOneBotController, BugLanguage>()
        .AddSingleton<IOneBotController, Nbnhhsh>()
        .AddSingleton<IOneBotController, RepeatCollector>()
        .AddSingleton<IOneBotController, Painting>()
        .AddSingleton<IOneBotController, MsgWordCloud>()
        .AddSingleton<IOneBotController, UrlPreview>()
        .AddSingleton<IOneBotController, IntallkRandom>()
        .AddSingleton<IOneBotController, TTS>()
        .AddSingleton<IOneBotController, DictionaryReply>()
        .AddSingleton<IOneBotController, Permission>()
        .AddSingleton<IOneBotController, RhythmGameSong>()
        .AddSingleton<IOneBotCommandRouteConfiguration, IntallkConfig>();

    foreach (string childPath in new string[] { "", "\\Images", "\\Cache", "\\Resources", "\\Logs", "\\FileDetection" })
    {
        if (!Directory.Exists(IntallkConfig.DataPath + childPath))
            Directory.CreateDirectory(IntallkConfig.DataPath + childPath);
    }


});


var app = builder.Build();
MainModule.Services = app.Services;
MainModule.Config = new IntallkConfig();
app.Run();