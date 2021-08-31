using UnityEngine;

namespace Robin
{
    public abstract class SimpleScrollItemObject : MonoBehaviour
    {
        [HideInInspector]
        public int Index;
        [HideInInspector]
        public SimpleScrollItem MyItem;
        [HideInInspector]
        public SimpleScroll MyScroll;
        public abstract void OnItemRedraw();

        public void MoveToIndex(int newIndex)
        {
            MyScroll.RepositionItem(Index, newIndex);
        }
    }
}