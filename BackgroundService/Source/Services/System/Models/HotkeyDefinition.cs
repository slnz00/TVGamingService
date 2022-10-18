using System;
using System.Windows.Forms;

namespace BackgroundService.Source.Services.System.Models
{
    internal class HotkeyDefinition
    {
        private KeyModifiers keyModifier;
        private Keys key;

        public KeyModifiers KeyModifier => keyModifier;

        public Keys Key => key;

        public string KeyModifierName => Enum.GetName(typeof(KeyModifiers), keyModifier);

        public string KeyName => Enum.GetName(typeof(Keys), key);

        public HotkeyDefinition(KeyModifiers keyModifier, Keys key)
        {
            this.key = key;
            this.keyModifier = keyModifier;
        }
    }
}
