using System.Drawing;

namespace New_Tradegy.Library.Utils
{
    public static class ChartLayoutUtils
    {
        public static int ScreenWidth => g.screenWidth;
        public static int ScreenHeight => g.screenHeight;

        public static int ChartRows => g.nRow;
        public static int ChartCols => g.nCol;

        public static Size GetChartSize()
        {
            int width = ScreenWidth / ChartCols;
            int height = ScreenHeight / ChartRows;

            return new Size(width, height);
        }

        public static Point GetChartLocation(int row, int col, int xGap = 5, int yGap = 5)
        {
            var size = GetChartSize();
            int x = size.Width * col + xGap;
            int y = size.Height * row + yGap;
            return new Point(x, y);
        }

        public static Point GetBookBidLocation(int row, int col)
        {
            // BookBids appear to the right of chart area
            var baseLoc = GetChartLocation(row, col);
            return new Point(baseLoc.X + GetChartSize().Width + 10, baseLoc.Y);
        }

        public static (int row, int col) GetGridIndexFromPoint(Point p)
        {
            var size = GetChartSize();
            int col = p.X / size.Width;
            int row = p.Y / size.Height;
            return (row, col);
        }
    }
}
