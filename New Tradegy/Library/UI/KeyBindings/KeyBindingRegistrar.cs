using System.Windows.Forms;
using New_Tradegy.Library.UI.KeyBindings;

namespace New_Tradegy.Library.UI.KeyBindings
{
    public static class KeyBindingRegistrar
    {
        public static void RegisterAll()
        {
            // private static readonly Dictionary<(Keys key, bool shift, bool ctrl, bool alt), Action<Form>>
            KeyBindingManager.Register(Keys.Escape, false, false, false, ActionHandlers.DealCancelOrder);
            KeyBindingManager.Register(Keys.F1, false, false, false, ActionHandlers.DoubleDealMoney);
            KeyBindingManager.Register(Keys.F2, false, false, false, ActionHandlers.HalfDealMoney);
            KeyBindingManager.Register('F', false, false, ActionHandlers.코피_코닥_관심);
            KeyBindingManager.Register('f', true, false, ActionHandlers.코피_코닥_관심);
            KeyBindingManager.Register(Keys.Escape, true, false, false, ActionHandlers.상순_저순);
            KeyBindingManager.Register('j', false, false, ActionHandlers.편차_평균);
            KeyBindingManager.Register('d', false, false, false, ActionHandlers.푀분_총점);
            KeyBindingManager.Register('a', false, false, false, ActionHandlers.피올_닥올);
            KeyBindingManager.Register('s', false, false, false, ActionHandlers.프누_종누);
            KeyBindingManager.Register('S', false, false, false, ActionHandlers.프편_종편);
            KeyBindingManager.Register('D', false, false, false, ActionHandlers.배차);
            KeyBindingManager.Register('F', false, false, false, ActionHandlers.분거);
            KeyBindingManager.Register('z', false, false, false, ActionHandlers.토글_옵티멈);
            KeyBindingManager.Register(' ', false, false, false, ActionHandlers.화면_다음);
            KeyBindingManager.Register('n', false, false, false, ActionHandlers.화면_이전);
            KeyBindingManager.Register(';', false, false, false, ActionHandlers.열기_제어);
            KeyBindingManager.Register(':', false, false, false, ActionHandlers.열기_상관);
            KeyBindingManager.Register('\'', false, false, false, ActionHandlers.열기_메모);
        }
    }
}
