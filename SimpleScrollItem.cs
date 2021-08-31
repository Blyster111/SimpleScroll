namespace Robin
{
    public abstract class SimpleScrollItem
    {
        /// <summary>
        /// Index of the allItems[].
        /// </summary>
        public int ItemIndex { get; set; }
        /// <summary>
        /// Generated position on canvas.
        /// </summary>
        public float ItemCanvasPosition { get; set; }
        /// <summary>
        /// Custom item size, set to -1 to use the template size instead.
        /// </summary>
        public float ItemSize { get; set; }
    }
}
