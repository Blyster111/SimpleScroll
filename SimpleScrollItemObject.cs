using UnityEngine;

namespace SimpleScrollUnity
{
    public abstract class SimpleScrollItemObject : MonoBehaviour
    {   
        /// <summary>
        /// Index of the allItems[].
        /// </summary>
        [HideInInspector]
        public int Index;
        /// <summary>
        /// My Item Data. (Item of allItems[])
        /// </summary>
        [HideInInspector]
        public SimpleScrollItem MyItem;
        /// <summary>
        /// The SimpleScroll instance.
        /// </summary>
        [HideInInspector]
        public SimpleScroll MyScroll;
        public abstract void OnItemRedraw();
    }
}