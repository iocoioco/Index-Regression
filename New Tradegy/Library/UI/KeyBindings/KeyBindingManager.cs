using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace New_Tradegy.KeyBindings
{
    public static class KeyBindingManager
    {
        private static readonly Dictionary<(Keys key, bool shift, bool ctrl, bool alt), Action<Form>> _bindings = new();

        public static void Register(Keys key, bool shift, bool ctrl, bool alt, Action<Form> action)
        {
            _bindings[(key, shift, ctrl, alt)] = action;
        }

        public static bool TryHandle(Keys keyData, Form context)
        {
            Keys keyOnly = keyData & ~Keys.Modifiers;
            bool isShift = (keyData & Keys.Shift) == Keys.Shift;
            bool isCtrl = (keyData & Keys.Control) == Keys.Control;
            bool isAlt = (keyData & Keys.Alt) == Keys.Alt;

            if (_bindings.TryGetValue((keyOnly, isShift, isCtrl, isAlt), out var action))
            {
                action?.Invoke(context);
                return true;
            }

            return false;
        }
    }

}
