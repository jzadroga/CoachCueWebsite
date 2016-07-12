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
    public class nflteamsTests
    {
        [Test]
        public void GetID()
        {
            int teamID = nflteam.GetID("PHI");
            if (teamID == 0)
                Assert.Fail("Team not found");

            Assert.Pass("TeamID: " + teamID.ToString());
        }
    }
}