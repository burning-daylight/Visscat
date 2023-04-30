/****************************************************************************************
 * Module:      MessageDialog.cs
 * Description: The MessageDialog static class provides functionality to call dialog
 *              messages for users, i.e. message messages, error messages or messages
 *              requiring user's answer.
 * Author:      Ilya Galaktionov,
 *              Email: ilya_galaktionov@live.ru
 *              (с) All rights reserved.
****************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScatteringDiagrams
{
    public static class MessageDialog
    {
#region Error message
        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, 
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1);
        }

        public static void ShowErrorMessage(string message, string specificInfo)
        {
            if (specificInfo != "")
            {
                message += "\n\nAdditional message:" + specificInfo;
                ShowErrorMessage(message);
            }
            else
                ShowErrorMessage(message);
        }
#endregion

#region Warning message
        public static void ShowWarningMessage(string message)
        {
            MessageBox.Show(message,
                               "Warning",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Exclamation,
                               MessageBoxDefaultButton.Button1);
        }

        public static void ShowWarningMessage(string message, string specificInfo)
        {
            if (specificInfo != "")
            {
                message += "\n\nAdditional message:" + specificInfo;
                ShowWarningMessage(message);
            }                
            else
                ShowWarningMessage(message);
        }
#endregion

#region Info message
        public static void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "messagermation",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information,
                               MessageBoxDefaultButton.Button1);
        }

        public static void ShowInfoMessage(string message, string specificInfo)
        {
            if (specificInfo != "")
            {
                message += "\n\nAdditional message:" + specificInfo;
                ShowInfoMessage(message);
            }
            else
                ShowInfoMessage(message);
        }
#endregion

#region Question YesNo message
        public static DialogResult ShowYesNoMessage(string message)
        {
            DialogResult dlgRes;

            dlgRes = MessageBox.Show(message,
                                   "Request",
                                   MessageBoxButtons.YesNo,
                                   MessageBoxIcon.Question,
                                   MessageBoxDefaultButton.Button1);
            return dlgRes;
        }

        public static DialogResult ShowYesNoMessage(string message, string specificInfo)
        {
            DialogResult dlgRes;

            if (specificInfo != "")
            {
                message += "\n\nAdditional message:" + specificInfo;
                dlgRes = ShowYesNoMessage(message);
            }
            else
                dlgRes =  ShowYesNoMessage(message);

            return dlgRes;
        }
#endregion

#region Question YesNoCancel message
        public static DialogResult ShowYesNoCancelMessage(string message)
        {
            DialogResult dlgRes;

            dlgRes = MessageBox.Show(message, "Request",
                                        MessageBoxButtons.YesNoCancel,
                                        MessageBoxIcon.Question,
                                        MessageBoxDefaultButton.Button1);
            return dlgRes;
        }

        public static DialogResult ShowYesNoCancelMessage(string message, string specificInfo)
        {
            DialogResult dlgRes;

            if (specificInfo != "")
            {
                message += "\n\nAdditional info:" + specificInfo;
                dlgRes = ShowYesNoCancelMessage(message);
            }
            else
                dlgRes = ShowYesNoCancelMessage(message);

            return dlgRes;
        }
#endregion
    }
}
