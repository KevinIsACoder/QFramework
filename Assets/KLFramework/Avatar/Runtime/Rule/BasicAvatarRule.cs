using System.Collections.Generic;

namespace KLFramework.Avatar
{

    //******************************************
    // BasicAvatarRule
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-08-01 17:54
    //******************************************
    public class BasicAvatarRule<C> : IAvatarRule<C>
    {
        private Dictionary<int, int[]> _PartType2MuteParts = new Dictionary<int, int[]>();
        
        public BasicAvatarRule<C> AddMute(int part, params int[] muteParts)
        {
            _PartType2MuteParts.Add(part, muteParts);
            return this;
        }

        public int[] GetMuteParts(int partType, IAvatarPart<C> part)
        {
            if (_PartType2MuteParts.TryGetValue(partType, out int[] muteParts))
            {
                return muteParts;
            }
            return null;
        }
    }
}