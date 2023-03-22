using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Components
{
    public class AtlasAsset : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        private List<Sprite> spriteList = new List<Sprite>();
        public Sprite GetSpriteByName(string spriteName)
        {
            return spriteList.Find(sp => sp.name == spriteName);
        }

        public void AddSprite(Sprite sprite)
        {
            spriteList.Add(sprite);
        }

        public List<Sprite> sprites
        {
            get { return spriteList; }
        }
    }
}

