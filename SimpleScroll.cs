using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using System.Collections;

namespace SimpleScrollUnity
{
    public class SimpleScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Main Components")]
        public GameObject ItemTemplate;
        public GameObject Container;
        public Scrollbar ScrollingBar;

        private Vector2 itemRect;
        private Vector2 containerRect;

        private List<SimpleScrollItemObject> cachedRows;
        private List<SimpleScrollItem> allItems;

        [Header("Layout")]
        [Tooltip("Uncheck to use vertical scroll")]
        public bool IsHorizontal;

        [Header("Interaction")]

        public bool EnableMouseWheelScroll = true;
        public bool EnableTouchScroll = true;
        public float MouseScrollSensitivity = 0.1f;
        public float TouchScrollSensitivity = 0.1f;
        public float TouchScrollMomentumSpeed = 0.1f;
        public float TouchScrollMomentumSlowDownSpeed = 0.1f;

        private bool initialized_ = false;
        private bool mouseover_ = false;
        private bool isdragging_ = false;
        private bool islocked_ = false;

        private Coroutine autoscrollroutine_;

        private Vector2 dragstart_;

        private float defaultitemsize_;
        private float momentumtimeout_;
        private float touchscrolllaststepsize_;
        private float scrollbarbeforedrag_;
        private float touchscrollvelocity_;
        private float cachedfullcanvassize_;
        /// <summary>
        /// Initializes the stuff.
        /// </summary>
        public void Initialize()
        {
            if (initialized_) return;
            ScrollingBar.value = 0;
            ScrollingBar.onValueChanged.AddListener(ScrollbarChanged);
            cachedRows = new List<SimpleScrollItemObject>();
            itemRect = ((RectTransform)ItemTemplate.transform).rect.size;
            containerRect = ((RectTransform)Container.transform).rect.size;
            defaultitemsize_ = IsHorizontal ? itemRect.x : itemRect.y;
            ItemTemplate.SetActive(false);
            initialized_ = true;
        }
        private void Update()
        {
            HandleMouseScroll();
            HandleTouchScroll();
        }
        void OnDisable()
        {
            StopAutoScroll();
            LockInput(false);
        }
        /// <summary>
        /// Initializes list with items.  
        /// </summary>
        /// <param name="items">List of objects of class derived from SimpleScrollItem</param>
        public void FillWithData(List<SimpleScrollItem> items)
        {
            allItems = items;
            IndexItems();
        }
        private void IndexItems()
        {
            for (int i = 0; i < allItems.Count; i++)
            {
                var item = allItems[i];
                item.ItemIndex = i;
                if (item.ItemSize == 0) item.ItemSize = defaultitemsize_;
            }
        }
        /// <summary>
        /// Disables all rows.
        /// </summary>
        public void DisableRows()
        {
            foreach (var o in cachedRows)
                o.gameObject.SetActive(false);
        }
        /// <summary>
        /// Disables rows that aren't visible from the container rect.
        /// </summary>
        private void DisableUnusedRows()
        {
            foreach (var o in cachedRows)
            {
                if (!IsItemVisibleOnPosition(o.MyItem.ItemCanvasPosition, o.MyItem.ItemSize))
                {
                    o.gameObject.SetActive(false);
                }
            }
        }
        /// <summary>
        /// Destroys all cached rows.
        /// </summary>
        public void DeleteCachedRows()
        {
            foreach (var o in cachedRows)
                Destroy(o.gameObject);
            cachedRows.Clear();
        }
        public void DeleteCachedCanvasSize()
        {
            cachedfullcanvassize_ = 0;
        }
        public void InsertItem(int insertIndex, SimpleScrollItem item)
        {
            allItems.Insert(insertIndex, item);

            //Redraw everything again.
            Repaint(true);
        }
        public void DeleteItem(int deleteIndex)
        {
            allItems.RemoveAt(deleteIndex);

            //Redraw everything again.
            Repaint(true);
        }
        public void RepositionItem(int oldIndex, int newIndex)
        {
            var i = allItems[oldIndex];
            allItems.Remove(i);
            allItems.Insert(newIndex, i);

            //Redraw everything again.
            Repaint(true);
        }
        /// <summary>
        /// Scrolls to the item with the index.
        /// </summary>
        /// <param name="index">Index of the item</param>
        public void FastForwardToIndex(int index)
        {
            float scrollAmount = CalculateScrollAmountForIndex(index); //Returns -1 if the index is out of range
            if (scrollAmount != -1) ScrollingBar.value = Mathf.Clamp(scrollAmount, 0, 1);
        }
        public void SmoothMoveToIndex(int index, float timeInSeconds, bool lockUntilFinish)
        {
            float scrollAmount = CalculateScrollAmountForIndex(index); //Returns -1 if the index is out of range
            if (scrollAmount == -1) return;

            if (lockUntilFinish) LockInput(true);

            StopAutoScroll();
            autoscrollroutine_ = StartCoroutine(SmoothMoveRoutine(scrollAmount, timeInSeconds));
        }
        /// <summary>
        /// Repositions all rows
        /// </summary>
        public void Repaint(bool deleteCache)
        {
            if (allItems == null) return;
            
            if(deleteCache)
            {
                DeleteCachedCanvasSize();
                IndexItems();
                DeleteCachedRows();
            }

            DrawItemsAtPosition(IsHorizontal ? -ScrollingBar.value : ScrollingBar.value);
            DisableUnusedRows();
        }
        /// <summary>
        /// Gets an unused (disabled) row prefab from the cached list
        /// </summary>
        private SimpleScrollItemObject GetFreeRowOrCreate(int index)
        {
            var freeRow = GetRowForIndex(index);
            if (freeRow != null) return freeRow;
            freeRow = cachedRows.Where(x => !x.gameObject.activeInHierarchy).FirstOrDefault();
            if (freeRow == null)
            {
                var o = Instantiate(ItemTemplate, ItemTemplate.transform.parent);
                var s = o.GetComponent<SimpleScrollItemObject>();
                freeRow = s;
                cachedRows.Add(freeRow);
            }
            freeRow.Index = index;
            return freeRow;
        }
        /// <summary>
        /// Finds the created row prefab that's been used for the index last time, so it doesn't need to recreate it again if exists.
        /// </summary>
        /// <param name="index">Index of the item</param>
        private SimpleScrollItemObject GetRowForIndex(int index)
        {
            return cachedRows.Where(x => x.Index == index).FirstOrDefault();
        }
        /// <summary>
        /// 1. Gets an unused row or creates one, 2. Sets the position and index, 3. Redraws itself
        /// </summary>
        /// <param name="y">Anchored position of the row</param>
        /// <param name="index">Index of the item</param>
        private void DrawItemAtPosition(float position, int index)
        {
            var i = allItems[index];
            var o = GetFreeRowOrCreate(index);
            var s = o.GetComponent<SimpleScrollItemObject>();
            var r = o.GetComponent<RectTransform>();
            o.MyScroll = this;
            o.gameObject.SetActive(true);

            if (IsHorizontal)
            {
                r.sizeDelta = new Vector2(i.ItemSize, r.sizeDelta.y);
                r.anchoredPosition = new Vector2(position, r.anchoredPosition.y);
            }
            else
            {
                r.sizeDelta = new Vector2(r.sizeDelta.x, i.ItemSize);
                r.anchoredPosition = new Vector2(r.anchoredPosition.x, position);
            }
            s.MyItem = i;
            s.OnItemRedraw();
        }
        /// <summary>
        /// Repaints all items.
        /// </summary>
        private void DrawItemsAtPosition(float offset)
        {
            //Total size of allItems[] together
            float totalSize = CalculateFullCanvasSize();
            //Initial position calculated from scroll percentage and total size.
            float scrollOffset = offset * totalSize / 1.0f;

            //Looping all the items, drawing only these which are visible inside the container area.
            for (int i = 0; i < allItems.Count; i++)
            {
                var item = allItems[i];
                //Updates the CanvasPosition even for invisible items,
                //so i can check which one are out of bounds and disable them for caching purposes.
                item.ItemCanvasPosition = scrollOffset;
                //Draws the item if the position'd be visible
                if (IsItemVisibleOnPosition(scrollOffset, item.ItemSize))
                {
                    DrawItemAtPosition(scrollOffset, i);
                }
                //Prepares position for next item
                if (IsHorizontal) scrollOffset += item.ItemSize;
                else scrollOffset -= item.ItemSize;
            }
        }
        /// <summary>
        /// Gets the full width (or height). All allItems[] sizes together.
        /// </summary>
        private float CalculateFullCanvasSize()
        {
            //Returns cached
            if (cachedfullcanvassize_ != 0) return cachedfullcanvassize_;

            float totalSize = 0;
            for (int i = 0; i < allItems.Count; i++)
            {
                totalSize += allItems[i].ItemSize;
            }
            totalSize -= IsHorizontal ? containerRect.x : containerRect.y;
            ; //Also cache it.
            cachedfullcanvassize_ = totalSize;
            return totalSize;
        }
        private float CalculateScrollAmountForIndex(int index)
        {
            if (index >= allItems.Count) return -1;
            float spaceBehind = 0;
            for (int i = 0; i < index; i++)
            {
                var item = allItems[i];
                spaceBehind += item.ItemSize;
            }
            float scrollAmount = spaceBehind * 1.0f / CalculateFullCanvasSize();
            return scrollAmount;
        }
        /// <summary>
        /// Checks whether the position is visible inside the container area
        /// </summary>
        private bool IsItemVisibleOnPosition(float position, float itemSize)
        {
            if (IsHorizontal)
            {
                return position + itemSize >= 0 && position < containerRect.x;
            }
            else
            {
                return position >= -containerRect.y && position - itemSize <= 0;
            }
        }
        /// <summary>
        /// Scrolling stuff
        /// </summary>
        private void HandleMouseScroll()
        {
            if (!EnableMouseWheelScroll || !mouseover_ || islocked_) return;
            float scrollDelta = Input.mouseScrollDelta.y;

            //Stop autoscroll if enabled
            if (scrollDelta != 0) StopAutoScroll();

            float scrollAmmount = 0.05f * MouseScrollSensitivity;
            float newValue = Mathf.Clamp(ScrollingBar.value - scrollDelta * scrollAmmount, 0, 1);
            ScrollingBar.value = newValue;
        }
        /// <summary>
        /// Call Repaint() after the scrollbar has changed.
        /// </summary>
        /// <param name="position"></param>
        private void ScrollbarChanged(float position)
        {
            Repaint(false);
        }
        /// <summary>
        /// Sets mouseover_, so I can know whether to enable or disable scrolling functions.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            mouseover_ = false;
        }
        /// <summary>
        /// ...
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseover_ = true;
        }
        /// <summary>
        /// Start touchscroll. Save the initial values.
        /// </summary>
        private void StartTouchScroll()
        {
            //Stop autoscroll if enabled
            StopAutoScroll();
            touchscrollvelocity_ = 0;
            dragstart_ = Input.mousePosition;
            scrollbarbeforedrag_ = ScrollingBar.value;
            isdragging_ = true;
        }
        /// <summary>
        /// Calls after the mouse button/touch is released.
        /// </summary>
        private void OnTouchScrollQuit()
        {
            isdragging_ = false;
            //Apply the velocity based on the last scrollbar step size
            //But ignore if was dragging the same position for more than 0.2 seconds
            if (momentumtimeout_ > 0)
            {
                touchscrollvelocity_ = touchscrolllaststepsize_ * TouchScrollMomentumSpeed;
            }
        }
        /// <summary>
        /// Recalculate the scrollbar value and velocity from mouse/touch position.
        /// </summary>
        private void UpdateTouchScrollPosition()
        {
            if (!isdragging_) return;

            //Scrollbar step size calculatzed from the allItems[] length.
            float scrollbarStep = (1.0f / allItems.Count) / 5;
            //Distance in pixels between start point and actual point.
            float actualDistance;
            if (IsHorizontal) actualDistance = dragstart_.x - Input.mousePosition.x;
            else actualDistance = dragstart_.y - Input.mousePosition.y;

            float actualStepSize = (actualDistance * scrollbarStep * TouchScrollSensitivity);
            if (IsHorizontal) actualStepSize = -actualStepSize;
            ScrollingBar.value = Mathf.Clamp(scrollbarbeforedrag_ - actualStepSize, 0, 1);

            //Reset calculation for the next frame.
            dragstart_ = Input.mousePosition;
            scrollbarbeforedrag_ = ScrollingBar.value;

            //Reset momentum timeout if the position has changed.
            if (actualStepSize != touchscrolllaststepsize_) momentumtimeout_ = 0.2f;
            touchscrolllaststepsize_ = actualStepSize;
            momentumtimeout_ -= Time.deltaTime;
        }
        private void UpdateTouchScrollMomentum()
        {
            if (isdragging_) return;

            //Slown down multipilier calculated from size allItems[] list length.
            float slowDownMultipilier = 1.0f / allItems.Count;

            touchscrollvelocity_ = Mathf.MoveTowards(touchscrollvelocity_, 0, slowDownMultipilier * TouchScrollMomentumSlowDownSpeed * Time.deltaTime);
            ScrollingBar.value = Mathf.Clamp(ScrollingBar.value - touchscrollvelocity_, 0, 1);
        }
        private void HandleTouchScroll()
        {
            if (!EnableTouchScroll || islocked_) return;

            if (Input.GetMouseButtonDown(0) && mouseover_)
            {
                //Stop touchscroll momentum after a click/tap
                touchscrollvelocity_ = 0;

                var selectedObject = EventSystem.current.currentSelectedGameObject;
                //Don't touchscroll if he's over the scrollbar
                if (selectedObject == null || selectedObject.GetComponent<Scrollbar>() == null)
                {
                    StartTouchScroll();
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (isdragging_) OnTouchScrollQuit();
            }
            UpdateTouchScrollPosition();
            UpdateTouchScrollMomentum();
        }
        private void StopAutoScroll()
        {
            if (autoscrollroutine_ != null)
            {
                StopCoroutine(autoscrollroutine_);
                autoscrollroutine_ = null;
            }
        }
        private void LockInput(bool locked)
        {
            islocked_ = locked;
            if(ScrollingBar != null) ScrollingBar.interactable = !locked;
        }
        IEnumerator SmoothMoveRoutine(float scrollAmount, float timeInSeconds)
        {
            float oldScrollValue = ScrollingBar.value;
            for (float t = 0; t < 1; t += Time.deltaTime / timeInSeconds)
            {
                ScrollingBar.value = Mathf.Lerp(oldScrollValue, scrollAmount, t);
                yield return new WaitForEndOfFrame();
            }
            ScrollingBar.value = scrollAmount;
            LockInput(false);
            autoscrollroutine_ = null;
        }
    }
}

