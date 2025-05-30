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
            KeyBindingManager.Register(Keys.F3, false, false, false,ActionHandlers.DealEmergencySellKey);

            // Number

            KeyBindingManager.Register('`', false, false, ActionHandlers.TimeShortMoveKey);
            KeyBindingManager.Register('1', false, false, ActionHandlers.TimeLongMoveKey);
            KeyBindingManager.Register('2', false, false, ActionHandlers.TimeOneForwardsKey);
            KeyBindingManager.Register('3', false, false, ActionHandlers.TimeOneBackwardsKey);
            KeyBindingManager.Register('4', false, false, ActionHandlers.TimeTenForwardsKey);
            KeyBindingManager.Register('5', false, false, ActionHandlers.TimeTenBackwardsKey);
            KeyBindingManager.Register('6', false, false, ActionHandlers.TimeThirtyForwardsKey);
            KeyBindingManager.Register('7', false, false, ActionHandlers.TimeThirtyBackwardsKey);
            // Top

            KeyBindingManager.Register('w', true, false, ActionHandlers.WeightControlKey);
            KeyBindingManager.Register('r', false, false, ActionHandlers.ShrinkOrNotTenMinusKey);
            KeyBindingManager.Register('R', false, false, ActionHandlers.ShrinkOrNotTenPlusKey);
            KeyBindingManager.Register('o', false, false, ActionHandlers.OpenFilesKey);
            KeyBindingManager.Register('O', false, false, ActionHandlers.OpenMemoKey);
            KeyBindingManager.Register('p', false, false, ActionHandlers.NewsPeoridKey);
            KeyBindingManager.Register('E', false, false, ActionHandlers.DrawBollingerKey);
            KeyBindingManager.Register('E', false, false, ActionHandlers.DrawForeignAndInstituteKey);
            KeyBindingManager.Register('E', false, false, ActionHandlers.DrawNormaStockKey);

            // Home
            KeyBindingManager.Register('a', false, false, ActionHandlers.피올_닥올);
            KeyBindingManager.Register('s', false, false, ActionHandlers.푀누_종누);
            KeyBindingManager.Register('s', true, false, ActionHandlers.푀누_종누);
            KeyBindingManager.Register('d', false, false, ActionHandlers.푀분_총점);
            KeyBindingManager.Register('f', false, false, ActionHandlers.보조차트_피올_닥올);
            KeyBindingManager.Register('F', false, false, ActionHandlers.보조차트_순위_관심);
  
            KeyBindingManager.Register('g', false, false, ActionHandlers.상순_저순);

            KeyBindingManager.Register('h', false, false, ActionHandlers.편차_평균);
            KeyBindingManager.Register('j', false, false, ActionHandlers.배차_분거);
            KeyBindingManager.Register('k', false, false, ActionHandlers.가격증순);

       
            KeyBindingManager.Register('z', false, false, ActionHandlers.OptimalTradingKey);
            KeyBindingManager.Register('x', false, false, ActionHandlers.RemoveBookAndInterest);
            KeyBindingManager.Register('c', false, false, ActionHandlers.KillWebTxtKey);
            KeyBindingManager.Register(' ', false, false, ActionHandlers.ListingForwadsKey);
            KeyBindingManager.Register('n', false, false, ActionHandlers.ListingBackwardKey);
            KeyBindingManager.Register('c', false, false, ActionHandlers.MemoOpenKey);
            KeyBindingManager.Register('.', false, false, ActionHandlers.DateBackwardsKey);
            KeyBindingManager.Register(',', false, false, ActionHandlers.DateForwardsKey);

            
        }
    }
}
