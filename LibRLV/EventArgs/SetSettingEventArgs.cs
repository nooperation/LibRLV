using System;

namespace LibRLV
{
    public class SetSettingEventArgs : EventArgs
    {
        public SetSettingEventArgs(string settingName, string settingValue)
        {
            this.SettingName = settingName;
            this.SettingValue = settingValue;
        }

        public string SettingName { get; }
        public string SettingValue { get; }
    }
}
