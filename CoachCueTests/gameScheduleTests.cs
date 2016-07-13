using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CoachCue.Model;

namespace CoachCue.Tests
{
    [TestFixture]
    public class gameScheduleTests
    {
        [Test]
        public void ImportSchedule()
        {
            gameschedule.ImportSchedule(2);

            Assert.Pass();
        }
    }
}