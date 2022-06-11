using Intallk.Config;
using Intallk.Modules;

using OneBot.CommandRoute.Configuration;
using OneBot.CommandRoute.Mixin;
using OneBot.CommandRoute.Models.VO;
using OneBot.CommandRoute.Services;

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
        .AddSingleton<IOneBotController, Keyword>()
        .AddSingleton<IOneBotCommandRouteConfiguration, IntallkConfig>();

    foreach (string childPath in new string[] { "", "\\Images", "\\Cache", "\\Resources", "\\Logs", "\\FileDetection" })
    {
        if (!Directory.Exists(IntallkConfig.DataPath + childPath))
            Directory.CreateDirectory(IntallkConfig.DataPath + childPath);
    }


});


var app = builder.Build();
app.Run();