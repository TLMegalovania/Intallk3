using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Intallk.Tests
{
    public class UnitTest1: BasicTest
    {
        public UnitTest1(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public void ϲ��()
        {
            PaintingModel.PaintFile paintfile;
            paintfile = new PaintingCompiler().CompilePaintScript(
                "��ϲ��.pngΪ����������������0.5,100����д��{ϲ������}����СΪ0.9x0.8���Զ�������С����ɫΪ255,255,0��"
                , out _);
            Assert.NotNull(paintfile);
        }
        [Fact]
        public void CreateGraphicsTest1()
        {
            PaintingModel.PaintFile paintfile;
            paintfile = new PaintingCompiler().CompilePaintScript("��back.pngΪ��������������", out _);
            Assert.NotNull(paintfile);
        }
        [Fact]
        public void CreateGraphicsTest2()
        {
            PaintingModel.PaintFile paintfile;
            paintfile = new PaintingCompiler().CompilePaintScript("��100x300�ĳߴ紴����������0.5,0.3����д���������ֺ�Ϊ15��Blackɫ��", out _);
            Assert.NotNull(paintfile);
        }
    }
}