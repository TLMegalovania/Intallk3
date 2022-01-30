using Intallk.Models;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace Intallk.Tests;

public class UnitTest1 : BasicTest
{
    internal UnitTest1(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public void Glory()
    {
        PaintFile paintfile;
        paintfile = new PaintingCompiler().CompilePaintScript(
            "��1392x2475�ĳߴ紴����������698,618������ͼƬ��'QQͷ��'����СΪ715x715�����С���0,0������ͼƬ��'�ڸ�.png'����479,1135����д��'{QQ����}'����СΪ427x113�����У��Զ�������С����ɫΪ204,33,19����195,1853����д��'{ע��}'����СΪ991x269�����У��Զ�������С����ɫΪ239,231,220����409,1351����д��'{�������}'����СΪ585x349�����У��Զ�������С����ɫΪ239,231,220������Ϊzihun110hao-wulinjianghuti��"
            , out _);
        Assert.NotNull(paintfile);
    }
    [Fact]
    public void CreateGraphicsTest1()
    {
        PaintFile paintfile;
        paintfile = new PaintingCompiler().CompilePaintScript("��back.pngΪ��������������", out _);
        Assert.NotNull(paintfile);
    }
    [Fact]
    public void CreateGraphicsTest2()
    {
        PaintFile paintfile;
        paintfile = new PaintingCompiler().CompilePaintScript("��100x300�ĳߴ紴����������0.5,0.3����д���������ֺ�Ϊ15��Blackɫ��", out _);
        Assert.NotNull(paintfile);
    }
}
