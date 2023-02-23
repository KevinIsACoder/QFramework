using System.Collections;
using System.Collections.Generic;

namespace KLFramework.Avatar
{

    //******************************************
    // BasicPartManager
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-28 11:34
    //******************************************
    public class BasicPartManager<C> : IPartManager<C>
    {
        private Dictionary<int, IAvatarPart<C>> _PartName2AvatarParts = new Dictionary<int, IAvatarPart<C>>();
        
        public IEnumerator<IAvatarPart<C>> GetEnumerator()
        {
            return _PartName2AvatarParts.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddPart(int partType, IAvatarPart<C> part)
        {
            if (_PartName2AvatarParts.ContainsKey(partType))
            {
                _PartName2AvatarParts[partType] = part;
            }
            else
            {
                _PartName2AvatarParts.Add(partType, part);
            }
        }

        public IAvatarPart<C> RemovePart(int partType)
        {
            var part = GetPart(partType);
            _PartName2AvatarParts.Remove(partType);
            return part;
        }

        public IAvatarPart<C> GetPart(int partType)
        {
            if (_PartName2AvatarParts.ContainsKey(partType))
            {
                return _PartName2AvatarParts[partType];
            }
            return null;
        }
    }
}