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
    public class statusTests
    {
        [Test]
        public void GetID()
        {
            int posID = status.GetID("Active", "nflplayers"); ;
            if (posID == 0)
                Assert.Fail("Status not found");

            Assert.Pass("StatusID: " + posID.ToString());
        }
    }
}