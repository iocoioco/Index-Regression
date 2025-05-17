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
            KeyBindingManager.Register(Keys.Escape, false, false, false, ActionHandlers.매매취소);
            KeyBindingManager.Register(Keys.F1, false, false, false, ActionHandlers.거액두배);
            KeyBindingManager.Register(Keys.F2, false, false, false, ActionHandlers.거액절반);
            KeyBindingManager.Register(Keys.F3, false, false, false, ActionHandlers.비상매도);

            // Number
            KeyBindingManager.Register('1', false, false, ActionHandlers.시간단동);
            KeyBindingManager.Register('2', false, false, ActionHandlers.시간장동);
            KeyBindingManager.Register('q', false, false, ActionHandlers.시간일진);
            KeyBindingManager.Register('Q', false, false, ActionHandlers.시간일후);
            KeyBindingManager.Register('w', false, false, ActionHandlers.시간십진);
            KeyBindingManager.Register('W', false, false, ActionHandlers.시간십후);
            KeyBindingManager.Register('e', false, false, ActionHandlers.시간삼진);
            KeyBindingManager.Register('E', false, false, ActionHandlers.시간삼후);


            KeyBindingManager.Register('a', false, false, ActionHandlers.코피코닥);
            KeyBindingManager.Register('s', false, false, ActionHandlers.프누종누);
            KeyBindingManager.Register('d', false, false, ActionHandlers.푀분총점);
            KeyBindingManager.Register('g', false, false, ActionHandlers.상순하순);

            KeyBindingManager.Register(Keys.Escape, true, false, false, ActionHandlers.상순_저순);
            KeyBindingManager.Register('j', false, false, ActionHandlers.편차_평균);
            
            
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
