using NUnit.Framework;
using MauiApp1.Logic;

namespace MauiApp1.Tests
{
    [TestFixture]
    public class TableLogicTests
    {
        [Test]
        public void SimpleAddition_ShouldReturnCorrectValue()
        {
            var table = new TableLogic(1, 1);
            table.SetExpression(0, 0, "2+3");
            Assert.That(table.GetValue(0, 0), Is.EqualTo("5"));
        }

        [Test]
        public void Division_ShouldReturnCorrectValue()
        {
            var table = new TableLogic(1, 1);
            table.SetExpression(0, 0, "10/2");
            Assert.That(table.GetValue(0, 0), Is.EqualTo("5"));
        }

        [Test]
        public void InvalidExpression_ShouldReturnERR()
        {
            var table = new TableLogic(1, 1);
            table.SetExpression(0, 0, "abc+1");
            Assert.That(table.GetValue(0, 0), Is.EqualTo("ERR"));
        }
    }
}
