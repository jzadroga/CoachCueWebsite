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
    public class positionTests
    {
        [Test]
        public void GetID()
        {
            int posID = position.GetID("OT");
            if (posID == 0)
                Assert.Fail("Position not found");

            Assert.Pass("PositionID: " + posID.ToString());
        }
    }
}