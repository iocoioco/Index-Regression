using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy.Library
{
    internal class fm
    {
        public void PositionFormBasedOnPercentage(Form mainForm, Form form, float leftPercentage, float topPercentage, float widthPercentage, float heightPercentage)
        {
            // Get the main form's dimensions using the passed form
            int mainFormWidth = mainForm.ClientSize.Width;
            int mainFormHeight = mainForm.ClientSize.Height;

            // Calculate position and size
            var (location, size) = PositionHelper.CalculatePositionAndSize(mainFormWidth, mainFormHeight, leftPercentage, topPercentage, widthPercentage, heightPercentage);

            // Apply the calculated values to the form
            form.Location = location;
            form.Size = size;
            form.Show();
        }

        public static class PositionHelper
        {
            /// <summary>
            /// Calculates the pixel position and size based on percentage values of the container's dimensions.
            /// </summary>
            /// <param name="containerWidth">Width of the container (e.g., main form or screen width).</param>
            /// <param name="containerHeight">Height of the container (e.g., main form or screen height).</param>
            /// <param name="leftPercentage">Left position as a percentage of container width (0 to 1).</param>
            /// <param name="topPercentage">Top position as a percentage of container height (0 to 1).</param>
            /// <param name="widthPercentage">Width as a percentage of container width (0 to 1).</param>
            /// <param name="heightPercentage">Height as a percentage of container height (0 to 1).</param>
            /// <returns>Tuple containing the calculated Location and Size.</returns>
            public static (Point location, Size size) CalculatePositionAndSize(
                int containerWidth, int containerHeight,
                float leftPercentage, float topPercentage,
                float widthPercentage, float heightPercentage)
            {
                int left = (int)(containerWidth * leftPercentage);
                int top = (int)(containerHeight * topPercentage);
                int width = (int)(containerWidth * widthPercentage);
                int height = (int)(containerHeight * heightPercentage);

                return (new Point(left, top), new Size(width, height));
            }
        }


    }
}
