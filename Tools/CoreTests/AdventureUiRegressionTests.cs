using System;
using System.Reflection;
using NUnit.Framework;

namespace MazeMath.CoreTests
{
    // Reflection keeps this test-first commit compilable before the policies exist.
    public sealed class AdventureUiRegressionTests
    {
        private static object Create(string name, params object[] args)
        {
            var type = Assembly.GetExecutingAssembly().GetType("MazeMath.Adventure." + name);
            Assert.That(type, Is.Not.Null, name + " must be implemented and used by the runtime HUD.");
            return Activator.CreateInstance(type, args);
        }
        private static object Call(object instance, string method, params object[] args)
        {
            var member = instance.GetType().GetMethod(method);
            Assert.That(member, Is.Not.Null, method);
            return member.Invoke(instance, args);
        }
        private static T Read<T>(object instance, string name)
        {
            return (T)instance.GetType().GetProperty(name).GetValue(instance);
        }

        [Test]
        public void CorrectAnswerClosesOnceAfterBriefFeedback()
        {
            var flow = Create("QuestionFeedbackFlow");
            Call(flow, "Begin");
            Assert.That(Call(flow, "RecordAnswer", true, 10.0), Is.EqualTo(true));
            Assert.That(Read<bool>(flow, "CanSubmit"), Is.False);
            Assert.That(Call(flow, "ShouldClose", 10.3), Is.EqualTo(false));
            Assert.That(Call(flow, "ShouldClose", 10.7), Is.EqualTo(true));
            Assert.That(Call(flow, "ShouldClose", 11.0), Is.EqualTo(false));
        }
        [Test]
        public void WrongAnswerLeavesTheQuestionOpenForRetry()
        {
            var flow = Create("QuestionFeedbackFlow");
            Call(flow, "Begin");
            Call(flow, "RecordAnswer", false, 0.0);
            Assert.That(Read<bool>(flow, "CanSubmit"), Is.True);
            Assert.That(Call(flow, "ShouldClose", 60.0), Is.EqualTo(false));
        }
        [Test]
        public void RepeatedSubmitCannotRestartTheSuccessTimer()
        {
            var flow = Create("QuestionFeedbackFlow");
            Call(flow, "Begin");
            Call(flow, "RecordAnswer", true, 1.0);
            Assert.That(Call(flow, "RecordAnswer", true, 1.5), Is.EqualTo(false));
            Assert.That(Call(flow, "ShouldClose", 1.7), Is.EqualTo(true));
        }
        [Test]
        public void ClosingThenOpeningAnotherDialogCancelsTheOldTimer()
        {
            var flow = Create("QuestionFeedbackFlow");
            Call(flow, "Begin");
            Call(flow, "RecordAnswer", true, 1.0);
            Call(flow, "Cancel");
            Assert.That(Call(flow, "ShouldClose", 3.0), Is.EqualTo(false));
            Call(flow, "Begin");
            Assert.That(Call(flow, "ShouldClose", 5.0), Is.EqualTo(false));
            Assert.That(Read<bool>(flow, "CanSubmit"), Is.True);
        }
        [TestCase(1280.0,720.0,false)]
        [TestCase(1920.0,1080.0,false)]
        [TestCase(1024.0,768.0,false)]
        [TestCase(844.0,390.0,true)]
        [TestCase(1280.0,720.0,true)]
        [TestCase(390.0,844.0,true)]
        [TestCase(320.0,568.0,true)]
        public void HudAndTouchControlsStayOutsideTheCameraArea(double width, double height, bool touch)
        {
            var layout = Create("AdventureHudLayout", width, height, touch);
            var header = Read<double>(layout,"HeaderHeight");
            var dock = Read<double>(layout,"DockHeight");
            var bottom = Read<double>(layout,"WorldBottom");
            var top = Read<double>(layout,"WorldTop");
            Assert.That(bottom * height, Is.GreaterThanOrEqualTo(dock - 0.001));
            Assert.That(top * height, Is.LessThanOrEqualTo(height - header + 0.001));
            Assert.That(top - bottom, Is.GreaterThanOrEqualTo(0.62));
            Assert.That(Read<double>(layout,"SlotSize"), Is.InRange(24.0,42.0));
            Assert.That(Read<double>(layout,"SlotSize") * 6 + 28, Is.LessThanOrEqualTo(width));
        }
    }
}
