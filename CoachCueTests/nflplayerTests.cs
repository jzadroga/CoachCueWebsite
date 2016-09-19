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
    public class nflplayerTests
    {
        [Test]
        public void GetPlayerVotes()
        {
            matchup.GetTopMathupVotes(8, false);

            Assert.Pass();
        }

        [Test]
        public void ImportRoster()
        {
            nflplayer.ImportRoster("MIA");

            Assert.Pass();
        }

        [Test]
        public void SavePlayer()
        {
            nflplayer player = nflplayer.SavePlayer("Mark", "Anderson", "DE", "93", "Alabama", "6", 20);
            if (player == null)
                Assert.Fail("Player could not be saved");

            Assert.Pass("PlayerID: " + player.playerID.ToString());
        }

        [Test]
        public void GetRoster()
        {
            List<nflplayer> players = nflplayer.GetRoster(20, false);
            if (players.Count() <= 0 )
                Assert.Fail("No players found");

            Assert.Pass();
        }

        [Test]
        public void GetTrending()
        {
            List<nflplayer> players = nflplayer.GetTrending(5);

            Assert.Pass();
        }
    }
}