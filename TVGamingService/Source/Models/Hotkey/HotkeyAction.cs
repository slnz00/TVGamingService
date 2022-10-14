using System;

namespace TVGamingService.Source.Models
{
    internal class HotkeyAction : HotkeyDefinition
    {
        private readonly string name;
        private readonly Action handler;
        private readonly uint timeout;
        private long lastActivatedAt;

        public string Name => name;
        public Action Handler => handler;
        public uint Timeout => timeout;
        public long LastActivatedAt => lastActivatedAt;

        public bool IsActive
        {
            get
            {
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                return lastActivatedAt + timeout < now;
            }
        }

        public HotkeyAction(HotkeyDefinition def, string name, Action handler, uint timeout) : base(def.KeyModifier, def.Key)
        {
            this.name = name;
            this.handler = handler;
            this.timeout = timeout;

            lastActivatedAt = 0;
        }

        public void TriggerAction()
        {
            lastActivatedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            handler();
        }
    }
}
