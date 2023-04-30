using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace ScatteringDiagrams
{
    class RegistryService
    {
        RegistryKey regKey;
        RegistryKey progSubKey;
        
        public RegistryService(string programSubKey)
        {
            regKey = Registry.CurrentUser.CreateSubKey("Software");
            progSubKey = regKey.CreateSubKey(programSubKey);
        }

        public void SetValue(string valueName, object value)
        {
            if (valueName != null && value != null)
                progSubKey.SetValue(valueName, value);
        }

        public void SetValue(string valueName, object value, string regSubKeyName)
        {
            if (regSubKeyName!= null && valueName != null && value != null)
                progSubKey.CreateSubKey(regSubKeyName).SetValue(valueName, value);
        }

        public object GetValue(string valueName)
        {
            return progSubKey.GetValue(valueName);
        }

        public object GetValue(string valueName, string regSubKeyName)
        {
            return progSubKey.CreateSubKey(regSubKeyName).GetValue(valueName);
        }

        public double GetDoubleValue(string valueName, string regSubKeyName)
        {
            return Convert.ToDouble(progSubKey.CreateSubKey(regSubKeyName).GetValue(valueName));
        }

        public float GetFloatValue(string valueName, string regSubKeyName)
        {
            return Convert.ToSingle(progSubKey.CreateSubKey(regSubKeyName).GetValue(valueName));
        }

        public int GetIntValue(string valueName, string regSubKeyName)
        {
            return Convert.ToInt32(progSubKey.CreateSubKey(regSubKeyName).GetValue(valueName));
        }

        public bool GetBoolValue(string valueName, string regSubKeyName)
        {
            object res = progSubKey.CreateSubKey(regSubKeyName).GetValue(valueName);
            if ((string)res == "true" || (string)res == "True")
                return true;
            else
                return false;
        }

        public RegistryKey CreateProgramSubKey(RegistryKey systemRegKey, string programSubKeyName)
        {
            RegistryKey regKey = systemRegKey.CreateSubKey(programSubKeyName);
            return regKey;
        }
    }
}
