using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.Threading;
namespace New_Tradegy.Library
{

    public static class ChartState
    {
        public static Point? Chart1MousePosition { get; set; } = null;
        public static Point? Chart2MousePosition { get; set; } = null;
        public static string ActiveChart { get; set; } = null;
    }
    public static class ChartHelper
    {
        // Index
        // Stock to Buy : can be Empty
        // Group
        public static void DrawNumbersAtMousePosition(Graphics graphics, Point? mousePosition)
        {
            if (!mousePosition.HasValue)
                return;
   

            Color[] colors = new Color[3];
            int check_row = !g.connected ? g.Npts[1] - 1 : 0;

            int index = wk.return_index_of_ogldata("KODEX 레버리지");
            g.stock_data q = g.ogl_data[index];
            if (g.connected)
                check_row = q.nrow - 1;

            string sign = q.x[check_row, 1] < q.x[check_row - 1, 1] ? " - " : " + ";
            colors[0] = sign == " - " ? Color.Black : Color.Red;
            string string_index_kospi = $"{q.x[check_row, 1]}{sign}{Math.Abs(q.x[check_row, 1] - q.x[check_row - 1, 1])}";

            index = wk.return_index_of_ogldata("KODEX 코스닥150레버리지");
            q = g.ogl_data[index];
            if (g.connected)
                check_row = q.nrow - 1;

            sign = q.x[check_row, 1] < q.x[check_row - 1, 1] ? " - " : " + ";
            colors[1] = sign == " - " ? Color.Black : Color.Red;
            string string_index_kosdq = $"{q.x[check_row, 1]}{sign}{Math.Abs(q.x[check_row, 1] - q.x[check_row - 1, 1])}";

            StringBuilder string_group = new StringBuilder("\n");
            colors[2] = Color.Black;
            for (int i = 0; i < 5; i++)
            {
                string_group.AppendLine($"{g.oGL_data[i].title}  {Convert.ToInt32(g.oGL_data[i].푀분)}  {Convert.ToInt32(g.oGL_data[i].거분)}  {Convert.ToInt32(g.oGL_data[i].배차)}");
            }

            using (Font font = new Font("Arial", 9, FontStyle.Bold))
            {
                // Draw the first (kospi)
                using (Brush brush = new SolidBrush(colors[0]))
                {
                    graphics.DrawString(string_index_kospi, font, brush, mousePosition.Value);
                }

                // Calculate the position for the second number ("kospi")
                SizeF size1 = graphics.MeasureString(string_index_kospi, font);
                PointF position2 = new PointF(mousePosition.Value.X, mousePosition.Value.Y + size1.Height);

                // Draw the second (kosdq)
                using (Brush brush = new SolidBrush(colors[1]))
                {
                    graphics.DrawString(string_index_kosdq, font, brush, position2);
                }

                // Calculate the position for the second number ("kosdq")
                SizeF size2 = graphics.MeasureString(string_index_kosdq, font);
                PointF position3 = new PointF(mousePosition.Value.X, mousePosition.Value.Y + size1.Height + size2.Height);

                // Draw the third (group)
                using (Brush brush = new SolidBrush(colors[2]))
                {
                    graphics.DrawString(string_group.ToString(), font, brush, position3);
                }
            }
        }
    }
}
