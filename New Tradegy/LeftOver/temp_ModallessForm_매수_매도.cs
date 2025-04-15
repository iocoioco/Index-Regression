using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace New_Tradegy.Library
{
    internal class temp_ModallessForm_매수_매도
    {
        //private Dictionary<string, Form_매수_매도> activeForms = new Dictionary<string, Form_매수_매도>();

        //private void Dgv_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        //{
        //    // Ensure valid click
        //    if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

        //    string stockName = dgv.Rows[11].Cells[0].Value.ToString(); // Stock name
        //    int price = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells[1].Value); // Price
        //    int quantity = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells[0].Value); // Quantity

        //    // Determine if it's a Sell or Buy action
        //    bool isSell = (e.ColumnIndex == 0 || e.RowIndex == 4); // 매도 (Sell)
        //    bool isBuy = (e.ColumnIndex == 2 || e.RowIndex == 5); // 매수 (Buy)

        //    if (isSell)
        //    {
        //        OpenOrUpdateConfirmationForm(stockName, price, quantity, true);
        //    }
        //    else if (isBuy)
        //    {
        //        OpenOrUpdateConfirmationForm(stockName, price, quantity, false);
        //    }
        //    else
        //    {
        //        // Save order for later execution
        //        if (e.RowIndex < 4) SaveSellOrder(stockName, price, quantity);
        //        else if (e.RowIndex > 5 && e.RowIndex < 10) SaveBuyOrder(stockName, price, quantity);
        //    }
        //}



        //private void OpenOrUpdateConfirmationForm(string stockName, int price, int quantity, bool isSell)
        //{
        //    if (activeForms.ContainsKey(stockName))
        //    {
        //        // Update existing form
        //        activeForms[stockName].UpdateForm(stockName, price, quantity, isSell);
        //    }
        //    else
        //    {
        //        // Create and show a new non-blocking (modeless) confirmation form
        //        Form_매수_매도 form = new Form_매수_매도(stockName, isSell ? "매도 ?" : "매수 ?", price, quantity, isSell);
        //        form.FormClosed += (s, e) => activeForms.Remove(stockName); // Remove from tracking when closed

        //        activeForms[stockName] = form;
        //        form.Show(); // Modeless (non-blocking)
        //    }
        //}
          


        //public void MonitorPrices(string stock, int sellHogaVolume, int sellHogaPrice, int buyHogaVolume, int buyHogaPrice)
        //{
        //    // Process Sell Orders First
        //    foreach (var order in new List<Order>(sellOrders))
        //    {
        //        if (order.Stock == stock && order.Price == buyHogaPrice)
        //        {
        //            OpenOrUpdateConfirmationForm(stock, order.Price, order.Quantity, true);
        //            RemoveOrder(sellOrders, order);
        //        }
        //    }

        //    // Process Buy Orders Next (Only if No Sell Form is Open for This Stock)
        //    if (!activeForms.ContainsKey(stock) || !activeForms[stock].IsSell)
        //    {
        //        foreach (var order in new List<Order>(buyOrders))
        //        {
        //            if (order.Stock == stock && order.Price == sellHogaPrice)
        //            {
        //                OpenOrUpdateConfirmationForm(stock, order.Price, order.Quantity, false);
        //                RemoveOrder(buyOrders, order);
        //            }
        //        }
        //    }
        //}



        //public partial class Form_매수_매도 : Form
        //{
        //    private string stock;
        //    private int price;
        //    private int quantity;
        //    public bool IsSell { get; private set; }

        //    public Form_매수_매도(string stockName, string text, int price, int quantity, bool isSell)
        //    {
        //        InitializeComponent();
        //        this.Text = text; // Set to "매수 ?" or "매도 ?"
        //        stock = stockName;
        //        this.price = price;
        //        this.quantity = quantity;
        //        this.IsSell = isSell;

        //        UpdateUI();
        //        PositionForm();

        //        this.TopMost = true;
        //    }

        //    public void UpdateForm(string stockName, int newPrice, int newQuantity, bool isSell)
        //    {
        //        this.Text = isSell ? "매도 ?" : "매수 ?";
        //        this.stock = stockName;
        //        this.price = newPrice;
        //        this.quantity = newQuantity;
        //        this.IsSell = isSell;

        //        UpdateUI();
        //        PositionForm();
        //    }

        //    private void UpdateUI()
        //    {
        //        richTextBox1.Text = $"{stock} : {price} X {quantity} = {price * quantity / 10000}만원";
        //    }

        //    private void PositionForm()
        //    {
        //        DataGridView dgv = fm.FindDataGridViewByName(Application.OpenForms["Form1"], stock);
        //        if (dgv == null) return;

        //        Point stockPosition = dgv.PointToScreen(Point.Empty);
        //        int newX = stockPosition.X + dgv.Width + 10;
        //        int newY = stockPosition.Y;

        //        Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
        //        newX = Math.Min(newX, screenBounds.Width - this.Width - 10);
        //        newY = Math.Min(newY, screenBounds.Height - this.Height - 10);

        //        this.StartPosition = FormStartPosition.Manual;
        //        this.Location = new Point(newX, newY);
        //    }

        //    private void Yes_Click(object sender, EventArgs e)
        //    {
        //        ExecuteTrade("03");
        //    }

        //    private void Ok_Click(object sender, EventArgs e)
        //    {
        //        ExecuteTrade("01");
        //    }

        //    private void Cancel_Click(object sender, EventArgs e)
        //    {
        //        this.Close();
        //    }

        //    private void ExecuteTrade(string orderType)
        //    {
        //        dl.deal_exec(IsSell ? "매도" : "매수", stock, quantity, price, orderType);
        //        this.Close();
        //    }
        //}





    }
}
