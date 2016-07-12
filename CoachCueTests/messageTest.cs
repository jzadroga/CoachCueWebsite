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
    public class messageTests
    {
        [Test]
        public void Create()
        {
            SavedMessage newMessage = message.Save(1, "2919", "This is a test", "general");
            if( newMessage.UserMessage.messageID == 0 )
                Assert.Fail("Message not created");

            Assert.Pass("MessageID: " + newMessage.UserMessage.messageID.ToString());
        }

        [Test]
        public void GetStream()
        {
            List<StreamContent> streamList = stream.GetStream(1, true);

            string messages = string.Empty;
            foreach (StreamContent item in streamList)
            {
                if( item.ContentType == "message" )
                    messages += item.MessageItem.messageText + "|";
            }

            Assert.Pass(messages);
        }

        [Test]
        public void FormatMessage()
        {
            string messageText = "Here is the link to espn http://espn.go.com/espn/otl/story/_/id/9134814/assistant-rutgers-basketball-coach-seen-video-pushing-players-using-slurs-resigns and yahoo.com to someone @ctwilson55";
            messageText = message.FormatMessage(messageText).messageText;

            Assert.Pass(messageText);
        }
    }
}