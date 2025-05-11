using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace New_Tradegy.Library.UI.KeyBindings
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public static class KeyBindingManager
    {
        private static readonly Dictionary<(Keys, bool, bool, bool), Action> _bindings = 
                            new Dictionary<(Keys, bool, bool, bool), Action>();

        // Main registration method
        public static void Register(Keys key, bool shift, bool ctrl, bool alt, Action action)
        {
            _bindings[(key, shift, ctrl, alt)] = action;
        }

        // Overload for char-based key registration (e.g., 'F', 'f')
        public static void Register(char c, bool ctrl, bool alt, Action action)
        {
            var (key, shift) = KeyHelper.FromChar(c);
            Register(key, shift, ctrl, alt, action);
        }

        // Try handling a key press event
        public static bool TryHandle(Keys keyData)
        {
            Keys keyOnly = keyData & ~Keys.Modifiers;
            bool isShift = (keyData & Keys.Shift) == Keys.Shift;
            bool isCtrl = (keyData & Keys.Control) == Keys.Control;
            bool isAlt = (keyData & Keys.Alt) == Keys.Alt;

            if (_bindings.TryGetValue((keyOnly, isShift, isCtrl, isAlt), out var action))
            {
                action?.Invoke();
                return true;
            }
            return false;
        }
    }


    public static class KeyHelper
    {
        public static (Keys key, bool shift) FromChar(char c)
        {
            if (char.IsLetter(c))
            {
                var upper = char.ToUpper(c);
                bool isShift = char.IsUpper(c);
                return ((Keys)upper, isShift);
            }

            if (char.IsDigit(c))
            {
                return ((Keys)(Keys.D0 + (c - '0')), false);
            }

            switch (c)
            {
                case ' ':
                    return (Keys.Space, false);
                case '~':
                    return (Keys.Oem3, true);
                case '`':
                    return (Keys.Oem3, false);
                case '!':
                    return (Keys.D1, true);
                case '@':
                    return (Keys.D2, true);
                case '#':
                    return (Keys.D3, true);
                case '^':
                    return (Keys.D6, true);
                default:
                    throw new ArgumentException($"Unsupported char '{c}' for key binding.");
            }
        }

    }

}
