﻿using Intallk.Config;
using Intallk.Models;
using Intallk.Modules;

using Newtonsoft.Json;

using OneBot.CommandRoute.Attributes;
using OneBot.CommandRoute.Models.Enumeration;
using OneBot.CommandRoute.Services;

using RestSharp;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;

using System.Text;

class Painting : IOneBotController
{
    public static List<PaintingProcessing> paints = new List<PaintingProcessing>();
    [Command("draw <template> <qq> [s1] [s2] [s3] [s4] [s5] [s6] [s7] [s8] [s9] [s10] [s11] [s12] [s13] [s14] [s15]")]
    public void Draw(GroupMessageEventArgs e, string template, User qq, [ParsedArguments] object[] args)
    {
        int pi = -1;
        if (!int.TryParse(template, out pi)) paints.FindIndex(m => m.Source.Name == template); else pi--;
        if (pi < 0 || pi >= paints.Count)
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "什么嘛，黑嘴...可不是因为不会画这个才不帮你画的呢！");
            e.Reply(SoraSegment.Image(IntallkConfig.DataPath + "\\Resources\\angry.jpg"));
            return;
        }
        if (paints[pi].Source.NeedQQParameter && qq == null)
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "绘制指令有误噢，您可以发送“.draw help " + template + "”取得帮助。");
            return;
        }
        string outfile = IntallkConfig.DataPath + "\\Images\\" + template + ".png";
        paints[pi].Paint(outfile, e, qq, args);
        e.Reply(SoraSegment.Image(outfile));
    }
    [Command("draw <template> [s1] [s2] [s3] [s4] [s5] [s6] [s7] [s8] [s9] [s10] [s11] [s12] [s13] [s14] [s15]")]
    public void Draw(GroupMessageEventArgs e, string template, [ParsedArguments] object[] args) => Draw(e, template, null!, args);
    [Command("draw help <template>")]
    public void DrawHelp(GroupMessageEventArgs e, string template)
    {
        int pi = -1;
        if (!int.TryParse(template, out pi)) paints.FindIndex(m => m.Source.Name == template); else pi--;
        if (pi < 0 || pi >= paints.Count)
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "这是什么绘图模板呀，黑嘴找不到呢。");
            e.Reply(SoraSegment.Image(IntallkConfig.DataPath + "\\Resources\\oh.png"));
            return;
        }
        e.Reply(SoraSegment.Reply(e.Message.MessageId) + "作者：" + MainModule.GetQQName(e, paints[pi].Source.Author) + "\n"
                                    + "绘制步骤：共" + paints[pi].Source.Commands!.Count.ToString() + "步"
                                    + "使用方法：.draw " + (pi+1).ToString() + "/" + paints[pi].Source.Name + paints[pi].Source.ParameterDescription);
        return;
    }
    [Command("draw list")]
    public void DrawList(GroupMessageEventArgs e) => DrawList(e, 1);
    [Command("draw list <index>")]
    public void DrawList(GroupMessageEventArgs e, int index)
    {
        string ret = "";
        int pagetotal = (int)Math.Ceiling(paints.Count * 1.0 / 10.0);
        if (index > pagetotal || index < 1) return;
        for(int i = (index - 1) * 10;i <= (index - 1) * 10 + 9; i++)
        {
            if (i >= paints.Count) break;
            ret += $"{i + 1}.{paints[i].Source.Name} by {MainModule.GetQQName(e,paints[i].Source.Author)}\n";
        }
        e.Reply($"黑嘴现总计收录绘图模板{paints.Count}个\n{ret}第{index}/{pagetotal}页，使用指令“.draw list 页数”查看更多模板。");
    }
    [Command("draw remove <template>", EventType = EventType.PrivateMessage)]
    public void DrawRemove(PrivateMessageEventArgs e, string template)
    {
        int pi = -1;
        if (!int.TryParse(template, out pi)) paints.FindIndex(m => m.Source.Name == template); else pi--;
        if (pi < 0 || pi >= paints.Count)
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "这是什么绘图模板呀，黑嘴找不到呢。");
            e.Reply(SoraSegment.Image(IntallkConfig.DataPath + "\\Resources\\oh.png"));
            return;
        }
        if (paints[pi].Source.Author != e.Sender.Id && e.Sender.Id != 1361778219)
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "怎么可以删除别人的模板呢！");
            e.Reply(SoraSegment.Image(IntallkConfig.DataPath + "\\Resources\\no.png"));
            return;
        }
        File.Delete(IntallkConfig.DataPath + "\\DrawingScript\\" + template);
        Directory.Delete(IntallkConfig.DataPath + "\\DrawingScript\\" + template);
        paints.RemoveAt(pi);
        e.Reply("删掉啦~");
    }
    [Command("draw edit <name> <code>", EventType = EventType.PrivateMessage)]
    public void DrawEdit(PrivateMessageEventArgs e, string name, string code) => DrawBuild(e, name, code, true);
    [Command("draw build <name> <code>", EventType = EventType.PrivateMessage)]
    public void DrawBuild(PrivateMessageEventArgs e, string name, string code) => DrawBuild(e, name, code, false);
    public void DrawBuild(PrivateMessageEventArgs e, string name, string code, bool skipNameCheck)
    {
        if (name.Contains('*') || name.Contains('\\') || name.Contains('/') || name.Contains('|') || name.Contains('?')
            || name.Contains(':') || name.Contains('\"') || name.Contains('<') || name.Contains('>'))
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "设定的模板名字里面不能有特殊符号噢！");
            e.Reply(SoraSegment.Image(IntallkConfig.DataPath + "\\Resources\\oh.png"));
            return;
        }
        if (name == "")
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "设定的模板名字不能为空。");
            return;
        }
        int pi = paints.FindIndex(m => m.Source.Name == name);
        if (skipNameCheck)
        {
            if (paints[pi].Source.Author != e.Sender.Id && e.Sender.Id != 1361778219)
            {
                e.Reply(SoraSegment.Reply(e.Message.MessageId) + "不可以改别人的模板文件哦！");
                e.Reply(SoraSegment.Image(IntallkConfig.DataPath + "\\Resources\\oh.png"));
                return;
            }
        }
        else
        {
            if (pi != -1)
            {
                e.Reply(SoraSegment.Reply(e.Message.MessageId) + "设定的模板名字'" + name + "'已经被'" + MainModule.GetQQName(e, paints[pi].Source.Author) + "'使用过了。");
                return;
            }
        }
        if (MainModule.hooks2.Exists(m => m.QQ == e.Sender.Id))
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "黑嘴还在等待您完成上一个操作呢！");
            e.Reply(SoraSegment.Image(IntallkConfig.DataPath + "\\Resources\\angry.jpg"));
            return;
        }
        var paintfile = new PaintFile();
        try
        {
            List<string> picList;
            paintfile = new PaintingCompiler().CompilePaintScript(code, out picList);
            paintfile.Author = e.Sender.Id;
            paintfile.Name = name;
            var serializer = new JsonSerializer();
            var sb = new StringBuilder();
            serializer.Serialize(new StringWriter(sb), paintfile);
            //e.Reply(SoraSegment.Reply(e.Message.MessageId) + sb.ToString());
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "恭喜您，绘图脚本编译通过了，以下是模板信息：\n" +
                                                             "作者：" + e.SenderInfo.Nick + "\n" +
                                                             "模板名称：" + name + "\n" +
                                                             "参数说明：" + paintfile.ParameterDescription + "\n" +
                                                             "绘制步骤：共" + paintfile.Commands!.Count.ToString() + "步");
            string picl = "";
            foreach (string s in picList) picl += s + "，";
            if (picList.Count > 0)
            {
                picList.Add(name);
                Directory.CreateDirectory(IntallkConfig.DataPath + "\\DrawingScript\\" + name);
                e.Reply("👍只差一步...接下来按照下面图片的顺序依次发出图片：\n" + picl, new TimeSpan(0,0,2));
                MainModule.RegisterHook(e.Sender.Id, DrawImageUploadCallBack, picList);
            }
            File.WriteAllText(IntallkConfig.DataPath + "\\DrawingScript\\" + name + ".json", sb.ToString());
        }
        catch (Exception ex)
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + ex.Message);
        }
    }
    public bool DrawImageUploadCallBack(PrivateMessageEventArgs e, MainModule.PrivateMessageHook hook)
    {
        foreach (SoraSegment msg in e.Message.MessageBody)
        {
            if (msg.MessageType == SegmentType.Image)
            {
                var img = (ImageSegment)msg.Data;
                string file = IntallkConfig.DataPath + "\\DrawingScript\\" + ((List<string>)hook.Data!)[^1] + "\\" + ((List<string>)hook.Data)[0];
                if (!File.Exists(file))
                    File.WriteAllBytes(file, new RestClient(img.Url).DownloadDataAsync(new RestRequest("#", Method.Get)).Result);

                ((List<string>)hook.Data).RemoveAt(0);
                if (((List<string>)hook.Data).Count == 1) break;
            }
        }
        if (((List<string>)hook.Data!).Count == 1)
        {
            string template = ((List<string>)hook.Data!)[^1];
            string outfile = IntallkConfig.DataPath + "\\Images\\" + template + ".png";
            string code = File.ReadAllText(IntallkConfig.DataPath + "\\DrawingScript\\" + template + ".json");
            JsonSerializer serializer = new();
            PaintFile paintfile = (PaintFile)serializer.Deserialize(new StringReader(code), typeof(PaintFile))!;
            PaintingProcessing painter = new(paintfile);
            painter.Paint(outfile, null!, null!, null!);
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "感谢哥哥的配合~以下是根据您提交的模板绘制的~\n" + 
                        "如果您觉得满意，请回复“是”；放弃本次提交，请回复“取消”；回复其他内容则当作修改脚本重新绘制~");
            e.Reply(SoraSegment.Image(outfile));
            MainModule.RegisterHook(e.Sender.Id, DrawImageConfirmCallBack, painter);
            return true;
        }
        return false;
    }
    public bool DrawImageConfirmCallBack(PrivateMessageEventArgs e, MainModule.PrivateMessageHook hook)
    {
        string? template = ((PaintingProcessing)hook.Data!).Source.Name;
        if (e.Message.RawText == "是")
        {
            e.Reply("🎉恭喜，绘图模板已收录！感谢您为黑嘴的绘图模板生态增添活力！");
            paints.Add((PaintingProcessing)hook.Data!);
            return true;
        } 
        else if (e.Message.RawText == "取消")
        {
            e.Reply(SoraSegment.Reply(e.Message.MessageId) + "好的。");
            File.Delete(IntallkConfig.DataPath + "\\DrawingScript\\" + template);
            Directory.Delete(IntallkConfig.DataPath + "\\DrawingScript\\" + template);
            return true;
        } 
        else
        {
            try
            {
                List<string> picList;
                PaintFile paintfile = new();
                PaintingProcessing painter = (PaintingProcessing)hook.Data!;
                paintfile = new PaintingCompiler().CompilePaintScript(e.Message.RawText, out picList);
                paintfile.Author = e.Sender.Id;
                paintfile.Name = template;
                var serializer = new JsonSerializer();
                var sb = new StringBuilder();
                serializer.Serialize(new StringWriter(sb), paintfile);
                for (int i = 0; i < picList.Count; i++)
                {
                    if (!File.Exists(IntallkConfig.DataPath + "\\DrawingScript\\" + template + "\\" + picList[i]))
                    {
                        e.Reply(SoraSegment.Reply(e.Message.MessageId) + "脚本更正失败，请不要在更正过程中添加新的图片。");
                        return false;
                    }
                }
                e.Reply(SoraSegment.Reply(e.Message.MessageId) + "脚本更正成功，并已重新为您生成预览图片。\n" +
                        "如果您觉得满意，请回复“是”；放弃本次提交，请回复“取消”；回复其他内容则当作修改脚本重新绘制~");
                painter.Source = paintfile;
                string outfile = IntallkConfig.DataPath + "\\Images\\" + template + ".png";
                painter.Paint(outfile, null!, null!, null!);
                e.Reply(SoraSegment.Image(outfile));
                File.WriteAllText(IntallkConfig.DataPath + "\\DrawingScript\\" + template + ".json", sb.ToString());
            }
            catch (Exception ex)
            {
                e.Reply(SoraSegment.Reply(e.Message.MessageId) + ex.Message + "\n更正脚本失败。");
            }
        }
        return false;
    }
}
