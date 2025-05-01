using System.Windows.Forms;
using System.Drawing;
using System;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library.Utils
{
    public static class GridUtils
    {
        public static void SetupBasicGrid(
        DataGridView dgv,
        int rowHeight = 25,
        string fontName = "Arial",
        int fontSize = 9,
        bool bold = true,
        bool showColumnHeaders = false,
        bool showRowHeaders = false,
        DataGridViewContentAlignment alignment = DataGridViewContentAlignment.MiddleCenter,
        bool alternateRowColor = false,
        Color? headerBackColor = null,
        Action<object, DataGridViewCellEventArgs> onDoubleClick = null,
        int[] columnWidths = null
    )
        {
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;

            dgv.ColumnHeadersVisible = showColumnHeaders;
            dgv.RowHeadersVisible = showRowHeaders;

            dgv.ReadOnly = true;
            dgv.ScrollBars = ScrollBars.None;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgv.DefaultCellStyle.Alignment = alignment;
            dgv.DefaultCellStyle.Font = new Font(fontName, fontSize, bold ? FontStyle.Bold : FontStyle.Regular);
            dgv.RowTemplate.Height = rowHeight;

            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            dgv.BackgroundColor = Color.White;
            dgv.GridColor = Color.LightGray;

            // Zebra style
            if (alternateRowColor)
            {
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            }

            // Header color
            if (headerBackColor.HasValue)
            {
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = headerBackColor.Value;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            }

            // Optional double-click handler
            if (onDoubleClick != null)
            {
                dgv.CellDoubleClick += new DataGridViewCellEventHandler(onDoubleClick);
            }

            // Set fixed column widths if provided
            if (columnWidths != null && dgv.Columns.Count == columnWidths.Length)
            {
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    dgv.Columns[i].Width = columnWidths[i];
                }
            }
        }

        // Safe cell set with null check
        public static void SetCellText(DataGridView dgv, int row, int col, string text)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke((MethodInvoker)(() => TrySetCell(dgv, row, col, text)));
            }
            else
            {
                TrySetCell(dgv, row, col, text);
            }
        }

        private static void TrySetCell(DataGridView dgv, int row, int col, string text)
        {
            if (row < dgv.Rows.Count && col < dgv.Columns.Count)
                dgv.Rows[row].Cells[col].Value = text;
        }

        // Clear all cell text and optionally color
        public static void ClearGrid(DataGridView dgv, bool clearColor = true)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = "";
                    if (clearColor)
                        cell.Style.BackColor = Color.White;
                }
            }
        }

        // Center alignment
        public static void CenterAlignAll(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }

        // Resize columns evenly based on total width
        public static void AutoSizeColumnsEvenly(DataGridView dgv)
        {
            int colCount = dgv.Columns.Count;
            if (colCount == 0) return;

            int colWidth = dgv.Width / colCount;
            for (int i = 0; i < colCount; i++)
            {
                dgv.Columns[i].Width = colWidth;
            }
        }
    }
}
