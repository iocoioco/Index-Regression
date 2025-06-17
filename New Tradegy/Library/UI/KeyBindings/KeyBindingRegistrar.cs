using System.Windows.Forms;
using New_Tradegy.Library.UI.KeyBindings;

namespace New_Tradegy.Library.UI.KeyBindings
{
    public static class KeyBindingRegistrar
    {
        // Function
        // Number
        // Top
        // Home
        // Bottom
        // Control
        public static void RegisterAll()
        {
            // private static readonly Dictionary<(Keys key, bool shift, bool ctrl, bool alt), Action<Form>>
            
            // Function
            KeyBindingManager.Register(Keys.Escape, false, false, false, ActionHandlers.DealCancel_TimeInitalKey);
            KeyBindingManager.Register(Keys.F1, false, false, false, ActionHandlers.DealHalf_TimeFinalKey);
            KeyBindingManager.Register(Keys.F2, false, false, false, ActionHandlers.DealDoubleKey);
            KeyBindingManager.Register(Keys.F3, false, false, false, ActionHandlers.DealConfirmSellKey);
            KeyBindingManager.Register(Keys.F4, false, false, false,ActionHandlers.DealEmergencySellKey);

            // Number
            KeyBindingManager.Register('`', false, false, ActionHandlers.피올_닥올);
            KeyBindingManager.Register('1', false, false, ActionHandlers.푀누_종누);
            KeyBindingManager.Register('2', false, false, ActionHandlers.푀분_총점);
            KeyBindingManager.Register('3', false, false, ActionHandlers.보조차트_피올_닥올);
            KeyBindingManager.Register('4', false, false, ActionHandlers.보조차트_순위_관심);
            KeyBindingManager.Register('5', false, false, ActionHandlers.상순_저순);
            KeyBindingManager.Register('6', false, false, ActionHandlers.편차_평균);
            KeyBindingManager.Register('7', false, false, ActionHandlers.배차_분거);
            KeyBindingManager.Register('8', false, false, ActionHandlers.가격증순);
           

            // Top
            KeyBindingManager.Register('q', false, false, ActionHandlers.TimeOneForwardsKey);
            KeyBindingManager.Register('Q', false, false, ActionHandlers.TimeOneBackwardsKey);
            KeyBindingManager.Register('w', false, false, ActionHandlers.TimeShortMoveKey);
            KeyBindingManager.Register('W', false, false, ActionHandlers.TimeLongMoveKey);
            KeyBindingManager.Register('w', true, false, ActionHandlers.WeightControlKey);
            KeyBindingManager.Register('e', false, false, ActionHandlers.TimeTenForwardsKey);
            KeyBindingManager.Register('E', false, false, ActionHandlers.TimeTenBackwardsKey);
            KeyBindingManager.Register('r', false, false, ActionHandlers.TimeThirtyForwardsKey);
            KeyBindingManager.Register('R', false, false, ActionHandlers.TimeThirtyBackwardsKey);

            KeyBindingManager.Register('o', false, false, ActionHandlers.OpenFilesKey);
            KeyBindingManager.Register('O', false, false, ActionHandlers.OpenMemoKey); // not implemented
            KeyBindingManager.Register('p', false, false, ActionHandlers.NewsPeoridKey);
            KeyBindingManager.Register('[', false, false, ActionHandlers.DrawBollingerKey);
            KeyBindingManager.Register(']', false, false, ActionHandlers.DrawForeignAndInstituteKey);
            KeyBindingManager.Register('\\', false, false, ActionHandlers.DrawNormaStockKey);

            // Home
            KeyBindingManager.Register('a', false, false, ActionHandlers.ShrinkOrNotTenPlusKey);
            KeyBindingManager.Register('A', false, false, ActionHandlers.ShrinkOrNotTenMinusKey);
            KeyBindingManager.Register('a', false, true, ActionHandlers.AddInterestToggle);


            KeyBindingManager.Register('s', false, false, ActionHandlers.SaveAllStocks);


            // Bottom
            KeyBindingManager.Register('z', false, false, ActionHandlers.OptimalTradingToggleKey);
            KeyBindingManager.Register('x', false, false, ActionHandlers.RemoveInterestedOnlyListKey);
            KeyBindingManager.Register('x', false, false, ActionHandlers.RemoveInterestedWithBidListKey);
            KeyBindingManager.Register('c', false, false, ActionHandlers.KillWebTxtFormKey);

            KeyBindingManager.Register(' ', false, false, ActionHandlers.HistoryDateForwardsKey);
            KeyBindingManager.Register('n', false, false, ActionHandlers.HistoryDateBackwardsKey);

          
     
        }
    }
}
