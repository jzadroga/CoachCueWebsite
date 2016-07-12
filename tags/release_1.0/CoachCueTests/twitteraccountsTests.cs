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
    public class twitteraccountsTests
    {
        [Test]
        public void Delete()
        {
            twitteraccount.Delete(1054);

            Assert.Pass();
        }

        [Test]
        public void GetRelativeTime()
        {
            DateTime time = new DateTime(2013, 5, 8, 8, 27, 40);

            string timeago = twitter.GetRelativeTime(time);

            Assert.Pass(timeago);
        }
    }
}