using System;
using System.Windows.Forms;

namespace CommLibrarys
{
    public class DialogBox
    {
        public enum DialogType
        {
            Message,
            Warn,
            Error
        }
        public static void Message(string msg, DialogBox.DialogType type)
        {
            if (type == DialogBox.DialogType.Message)
            {
                MessageBox.Show(msg, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            if (type == DialogBox.DialogType.Warn)
            {
                MessageBox.Show(msg, "警告信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            if (type == DialogBox.DialogType.Error)
            {
                MessageBox.Show(msg, "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
    }
}
