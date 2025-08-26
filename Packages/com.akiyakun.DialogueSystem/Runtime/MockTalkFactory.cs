using UnityEngine;

namespace SimpleTalk
{
    public class MockTalkFactory : ITalkFactory
    {
        public ITalk CreateTalk(string talkName)
        {
            return talkName switch
            {
                DefaultTalkNames.TalkWindow => new MockTalkWindow(),
                DefaultTalkNames.ChatBubble  => new MockChatBubble(),
                _ => null
            };
        }

        public bool DeleteTalk(ITalk talk)
        {
            if (talk == null) return false;
            return true;
        }
    }
}
