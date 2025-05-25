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
            KeyBindingManager.Register(Keys.Escape, false, false, false, ActionHandlers.DealCancelKey);
            KeyBindingManager.Register(Keys.F1, false, false, false, ActionHandlers.DealHalfKey);
            KeyBindingManager.Register(Keys.F2, false, false, false, ActionHandlers.DealDoubleKey);
            KeyBindingManager.Register(Keys.F3, false, false, false, ActionHandlers.DealConfirmSellKey);
            KeyBindingManager.Register(Keys.F3, false, false, false, ActionHandlers.DealEmergencySellKey);

            // Number
            KeyBindingManager.Register('`', false, false, ActionHandlers.TimeInitializeKey);
            KeyBindingManager.Register('~', false, false, ActionHandlers.TimeEndKey);
            KeyBindingManager.Register('1', false, false, ActionHandlers.TimeShortMoveKey);
            KeyBindingManager.Register('2', false, false, ActionHandlers.TimeLongMoveKey);

            // Top
            KeyBindingManager.Register('q', false, false, ActionHandlers.TimeOneForwardsKey);
            KeyBindingManager.Register('Q', false, false, ActionHandlers.TimeOneBackwardsKey);
            KeyBindingManager.Register('w', true, false, ActionHandlers.WeightControlKey);
            KeyBindingManager.Register('w', false, false, ActionHandlers.TimeTenForwardsKey);
            KeyBindingManager.Register('W', false, false, ActionHandlers.TimeTenBackwardsKey);
            KeyBindingManager.Register('e', false, false, ActionHandlers.TimeThirtyForwardsKey);
            KeyBindingManager.Register('E', false, false, ActionHandlers.TimeThirtyBackwardsKey);
            KeyBindingManager.Register('E', false, false, ActionHandlers.TimeThirtyBackwardsKey);
            KeyBindingManager.Register('r', false, false, ActionHandlers.ShrinkOrNotTenMinusKey);
            KeyBindingManager.Register('R', false, false, ActionHandlers.ShrinkOrNotTenPlusKey);
            KeyBindingManager.Register('o', false, false, false, ActionHandlers.OpenFilesKey);
            KeyBindingManager.Register('p', false, false, ActionHandlers.NewsPeoridDWMKey);
            KeyBindingManager.Register('E', false, false, ActionHandlers.DrawBollingerKey);
            KeyBindingManager.Register('E', false, false, ActionHandlers.DrawForeignAndInstituteKey);
            KeyBindingManager.Register('E', false, false, ActionHandlers.DrawNormaStockKey);

            // Home
            KeyBindingManager.Register('a', false, false, ActionHandlers.AllKospiOrKosdaqKey);
            KeyBindingManager.Register('s', false, false, ActionHandlers.AccumulationKey);
            KeyBindingManager.Register('s', true, false, ActionHandlers.AccumulationKey);
            KeyBindingManager.Register('d', false, false, ActionHandlers.ProgramAndForeignOrTotalKey);\
            KeyBindingManager.Register('f', false, false, ActionHandlers.SubChartKospiOrKosdaqKey);
            KeyBindingManager.Register('F', false, false, ActionHandlers.SubChartRankOrInsterestKey);
            KeyBindingManager.Register('f', false, false, ActionHandlers.SubChartKospiOrKosdaqKey);
            KeyBindingManager.Register('g', false, false, ActionHandlers.HighestOrLowestPriceKey);

            KeyBindingManager.Register('h', false, false, ActionHandlers.DeviationOrAverageKey);
            KeyBindingManager.Register('j', false, false, ActionHandlers.MultipleOrAmountKey);
            KeyBindingManager.Register('k', false, false, ActionHandlers.PriceIncreaseKey);

       
            KeyBindingManager.Register('z', false, false, ActionHandlers.OptimalTradingKey);
            KeyBindingManager.Register('x', false, false, ActionHandlers.RemoveBookAndInterest);
            KeyBindingManager.Register('c', false, false, ActionHandlers.KillWebTxtKey);
            KeyBindingManager.Register(' ', false, false, ActionHandlers.ListingForwadsKey);
            KeyBindingManager.Register('n', false, false, ActionHandlers.ListingBackwardKey);
            KeyBindingManager.Register('c', false, false, ActionHandlers.MemoOpenKey);
            KeyBindingManager.Register('.', false, false, ActionHandlers.DateBackwardsKey);
            KeyBindingManager.Register(',', false, false, ActionHandlers.DateForwardsKey);
            KeyBindingManager.Register(':', false, false, false, ActionHandlers.열기_상관);
            KeyBindingManager.Register('\'', false, false, false, ActionHandlers.열기_메모);
        }
    }
}
