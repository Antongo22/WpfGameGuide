using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp_Book.Chapters.Chapter3_Keyboard.Demo
{
    public partial class KeyboardPage : Page
    {
        public KeyboardPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            Key pressedKey = e.Key == Key.System ? e.SystemKey : e.Key;
            KeyPressedText.Text = $"Нажатая клавиша: {pressedKey}";
            
            ModifierKeys mods = Keyboard.Modifiers;
            if (mods == ModifierKeys.None)
                ModifiersText.Text = "Модификаторы: —";
            else
            {
                var list = new System.Collections.Generic.List<string>();
                if (mods.HasFlag(ModifierKeys.Control)) list.Add("Ctrl");
                if (mods.HasFlag(ModifierKeys.Shift)) list.Add("Shift");
                if (mods.HasFlag(ModifierKeys.Alt)) list.Add("Alt");
                ModifiersText.Text = $"Модификаторы: {string.Join(" + ", list)}";
            }
        }
    }
}
