using Core.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BackgroundService.Source.Services.OS.Models
{
    internal class HotkeyDefinition
    {
        private readonly KeyModifiers keyModifier;
        private readonly Keys key;

        public KeyModifiers KeyModifier => keyModifier;

        public Keys Key => key;

        public string KeyModifierName => Enum.GetName(typeof(KeyModifiers), keyModifier);

        public string KeyName => Enum.GetName(typeof(Keys), key);

        public HotkeyDefinition(KeyModifiers keyModifier, Keys key)
        {
            this.keyModifier = keyModifier;
            this.key = key;
        }

        public HotkeyDefinition(List<string> listDefinition)
        {
            if (listDefinition.Count != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(listDefinition), "Must have exactly 2 elements, a modifier and a key.");
            }

            keyModifier = EnumUtils.GetValue<KeyModifiers>(listDefinition[0]);
            key = EnumUtils.GetValue<Keys>(listDefinition[1]);
        }
    }
}
