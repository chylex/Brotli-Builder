using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace BrotliBuilder.Utils.Compat.Legacy{
    class LegacyMainMenu : MainMenuBase{
        private readonly MainMenu mainMenu;

        public LegacyMainMenu(Form form, IContainer container){
            this.mainMenu = form.Menu = new MainMenu(container);
        }

        public override Item AddItem(string caption){
            return new InternalItem(mainMenu.MenuItems.Add(caption));
        }

        private class InternalItem : Item{
            private readonly MenuItem menuItem;

            public InternalItem(MenuItem item){
                this.menuItem = item;
            }

            public override bool Enabled{
                get => menuItem.Enabled;
                set => menuItem.Enabled = value;
            }

            public override Item Add(string caption, Action onClick, Shortcut shortcut = Shortcut.None, bool isEnabled = true){
                var item = menuItem.MenuItems.Add(caption, (_, e) => onClick());
                item.Shortcut = shortcut;
                item.Enabled = isEnabled;
                return new InternalItem(item);
            }

            public override Item AddCheckBox(string caption, bool initialState, Action<bool> onToggle){
                var item = menuItem.MenuItems.Add(caption, (obj, e) => {
                    bool enable = !((MenuItem)obj!).Checked;
                    menuItem.Checked = enable;
                    onToggle(enable);
                });

                item.Checked = initialState;
                return new InternalItem(item);
            }

            public override void AddRadioOptions(string caption, string[] options, string initialState, Action<string> onChange){
                var parent = menuItem.MenuItems.Add(caption);
                var parentItems = parent.MenuItems;

                foreach(var option in options){
                    parentItems.Add(option, (obj, e) => {
                        foreach(var other in parentItems){
                            ((MenuItem)other!).Checked = ReferenceEquals(obj, other);
                        }

                        onChange(option);
                    }).RadioCheck = true;
                }

                parent.MenuItems[Array.IndexOf(options, initialState)].Checked = true;
            }

            public override void AddSeparator(){
                menuItem.MenuItems.Add("-");
            }
        }
    }
}
