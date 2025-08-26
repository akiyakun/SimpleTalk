#nullable enable
using System;
using UnityEngine;

namespace SimpleTalk
{
    public class TalkManager
    {
        protected ITalkFactory TalkFactory { get; set; }

        public TalkManager(ITalkFactory talkFactory)
        {
            // Debug.Assert(talkFactory != null);
            TalkFactory = talkFactory ?? throw new ArgumentNullException(nameof(talkFactory));
        }

        public ITalk? CreateTalk(string talkName)
        {
            return TalkFactory.CreateTalk(talkName);
        }

        public T? CreateTalk<T>(string talkName) where T : class, ITalk
        {
            return TalkFactory.CreateTalk(talkName) as T;
        }

        public void RemoveTalk(ITalk talk)
        {
            TalkFactory.DeleteTalk(talk);
        }

    }
}
#nullable restore