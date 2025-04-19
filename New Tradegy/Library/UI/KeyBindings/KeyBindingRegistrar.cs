using New_Tradegy.Library.UI.KeyBindings;
using System;
using System.Windows.Forms;

namespace New_Tradegy.KeyBindings
{
    public static class KeyBindingRegistrar
    {
        public static void RegisterAll()
        {
            KeyBindingManager.Register(Keys.Escape, shift: false, ctrl: true, alt: false, form =>
            {
                if (g.test)
                {
                    if (g.clickedStock == null) return;
                    // Handle test escape logic
                }
                else
                {
                    mc.Sound("Keys", "cancel");
                    for (int i = OrderTracker.OrderMap.Count - 1; i >= 0; i--)
                    {
                        var data = OrderTracker.GetOrderByRowIndex(i);
                        if (data != null)
                            DealManager.DealCancelOrder(data);
                    }
                }
            });

            KeyBindingManager.Register(Keys.D1, shift: false, ctrl: true, alt: false, form =>
            {
                if (!g.test)
                {
                    if (g.일회거래액 < 100)
                        g.일회거래액 = 100;
                    else if (g.일회거래액 < 500)
                        g.일회거래액 = 500;
                    else
                        g.일회거래액 *= 2;

                    mc.Sound_돈(g.일회거래액);
                    if (g.일회거래액 > 4000)
                    {
                        DialogResult result = MessageBox.Show("                     더블 ?", "일회거래액", MessageBoxButtons.YesNo);
                        if (result == DialogResult.No)
                            g.일회거래액 /= 2;
                    }

                    if (g.제어.dtb.Rows[0][2].ToString() != g.일회거래액.ToString())
                        g.제어.dtb.Rows[0][2] = g.일회거래액.ToString();
                }
            });

            // You can keep registering other keys: '2', '3', '`', 'q', etc...
        }
    }
}
