using UnityEngine;

namespace SimpleTalk
{
    public interface ITalkFactory
    {
        ITalk CreateTalk(string talkName);
        bool DeleteTalk(ITalk talk);
    }
}
