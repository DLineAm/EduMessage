using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EduMessage.Resources
{
    public class UniformGrid : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var itemWidth = availableSize.Width / Columns;
            var actualRows = Math.Ceiling((double)Children.Count / Columns);
            var actualHeight = itemWidth * actualRows;

            return new Size(availableSize.Width, actualHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size cellSize = new Size(finalSize.Width / Columns, finalSize.Width / Columns);
            int row = 0, col = 0;
            foreach (UIElement child in Children)
            {
                child.Arrange(new Rect(new Point(cellSize.Width * col, cellSize.Height * row), cellSize));
                if (++col == Columns)
                {
                    row++;
                    col = 0;
                }
            }
            return finalSize;

        }

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(UniformGrid), new PropertyMetadata(1, OnColumnsChanged));


        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(UniformGrid), new PropertyMetadata(1, OnRowsChanged));

        static void OnColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            int cols = (int)e.NewValue;
            if (cols < 1)
                ((UniformGrid)obj).Columns = 1;
        }

        static void OnRowsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            int rows = (int)e.NewValue;
            if (rows < 1)
                ((UniformGrid)obj).Rows = 1;
        }
    }
}