using UnityEngine;

namespace OniExtract
{
    public class OutPrioritizable
    {
        public Vector2 iconOffset;
        public float iconScale = 1f;
        public bool showIcon = true;


        public OutPrioritizable(Prioritizable obj)
        {
            this.iconOffset = obj.iconOffset;
            this.iconOffset = obj.iconOffset;
            this.showIcon = obj.showIcon;
        }
    }
}
