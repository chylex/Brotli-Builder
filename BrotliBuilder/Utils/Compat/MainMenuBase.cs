using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace BrotliBuilder.Utils.Compat{
    abstract class MainMenuBase : Component{
        public abstract Item AddItem(string caption);

        public abstract class Item{
            public abstract bool Enabled { get; set; }

            public abstract Item Add(string caption, Action onClick, Shortcut shortcut = Shortcut.None, bool isEnabled = true);
            public abstract Item AddCheckBox(string caption, bool initialState, Action<bool> onToggle);
            public abstract void AddRadioOptions(string caption, string[] options, string initialState, Action<string> onChange);
            public abstract void AddSeparator();
        }
    }
}
