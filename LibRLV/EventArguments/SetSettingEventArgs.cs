using System;

namespace LibRLV.EventArguments
{
    public class SetSettingEventArgs : EventArgs
    {
        public string SettingName { get; }
        public string SettingValue { get; }

        public SetSettingEventArgs(string settingName, string settingValue)
        {
            SettingName = settingName;
            SettingValue = settingValue;
        }
    }
}
