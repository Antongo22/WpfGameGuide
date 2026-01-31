using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp_Book.Chapters.Chapter1_Introduction.Answers
{
    /// <summary>
    /// Калькулятор - решение задания Главы 1
    /// Демонстрирует: Button, TextBox, TextBlock, обработку событий Click
    /// </summary>
    public partial class CalculatorAnswer : Page
    {
        // Текущая выбранная операция (+, -, *, /)
        private string currentOperation = "+";

        public CalculatorAnswer()
        {
            // InitializeComponent() загружает XAML и создает все элементы
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик нажатия на кнопки операций (+, -, *, /)
        /// sender - это кнопка, которая была нажата
        /// </summary>
        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            // Приводим sender к типу Button
            Button btn = (Button)sender;
            
            // Получаем текст с кнопки
            currentOperation = btn.Content.ToString()!;
            OperatorText.Text = currentOperation;
            ErrorText.Text = "";
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Вычислить"
        /// </summary>
        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            // ШАГ 1: Получаем и валидируем первое число
            string num1Text = TrimLeadingZeros(Number1Box.Text.Trim());
            Number1Box.Text = num1Text;
            
            if (!double.TryParse(num1Text, out double num1))
            {
                ErrorText.Text = "Ошибка: первое число некорректно";
                return;
            }

            // ШАГ 2: Получаем и валидируем второе число
            string num2Text = TrimLeadingZeros(Number2Box.Text.Trim());
            Number2Box.Text = num2Text;
            
            if (!double.TryParse(num2Text, out double num2))
            {
                ErrorText.Text = "Ошибка: второе число некорректно";
                return;
            }

            // ШАГ 3: Выполняем вычисление
            double result = 0;
            string expression = $"{num1} {currentOperation} {num2}";

            switch (currentOperation)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "-":
                    result = num1 - num2;
                    break;
                case "*":
                    result = num1 * num2;
                    break;
                case "/":
                    if (num2 == 0)
                    {
                        ErrorText.Text = "Ошибка: деление на ноль!";
                        ResultText.Text = "inf";
                        return;
                    }
                    result = num1 / num2;
                    break;
            }

            // ШАГ 4: Отображаем результат
            ExpressionText.Text = expression + " =";
            ResultText.Text = result.ToString("G10");
        }

        /// <summary>
        /// Обработчик кнопки "Очистить"
        /// </summary>
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Number1Box.Text = "0";
            Number2Box.Text = "0";
            ResultText.Text = "0";
            ExpressionText.Text = "";
            ErrorText.Text = "";
            currentOperation = "+";
            OperatorText.Text = "+";
        }

        /// <summary>
        /// Убирает ведущие нули из строки числа
        /// "007" -> "7", "000" -> "0", "0.5" -> "0.5"
        /// </summary>
        private string TrimLeadingZeros(string input)
        {
            if (string.IsNullOrEmpty(input)) return "0";

            bool isNegative = input.StartsWith("-");
            if (isNegative) input = input.Substring(1);

            input = input.TrimStart('0');

            if (string.IsNullOrEmpty(input)) return "0";
            if (input.StartsWith(".") || input.StartsWith(",")) input = "0" + input;

            return isNegative ? "-" + input : input;
        }
    }
}

/*
================================================================================
                           КАК ЭТО РАБОТАЕТ
================================================================================

СТРУКТУРА ПРИЛОЖЕНИЯ:
---------------------
1. XAML-ФАЙЛ (CalculatorAnswer.xaml)
   - Описывает внешний вид: кнопки, поля ввода, текстовые блоки
   - Каждый элемент имеет имя (x:Name) для доступа из кода
   - События (Click) связаны с методами в этом файле

2. C#-ФАЙЛ (CalculatorAnswer.xaml.cs) - этот файл
   - Содержит логику работы калькулятора
   - Обрабатывает нажатия кнопок
   - Выполняет вычисления и отображает результат


КЛЮЧЕВЫЕ КОНЦЕПЦИИ:
-------------------
1. ОБРАБОТЧИКИ СОБЫТИЙ
   - Метод вызывается автоматически при действии пользователя
   - Сигнатура: private void ИмяМетода(object sender, RoutedEventArgs e)
   - sender - элемент, вызвавший событие

2. ПРИВЕДЕНИЕ ТИПОВ
   - (Button)sender - преобразуем object в Button
   - Позволяет получить доступ к свойствам конкретного типа

3. ПАРСИНГ ЧИСЕЛ
   - double.TryParse(строка, out результат) - безопасное преобразование
   - Возвращает true если успешно, false если строка не число

4. SWITCH-ВЫРАЖЕНИЕ
   - Выбирает действие в зависимости от значения переменной
   - Каждый case заканчивается break

5. ИНТЕРПОЛЯЦИЯ СТРОК
   - $"текст {переменная}" - вставляет значение переменной в строку


ПОТОК ВЫПОЛНЕНИЯ:
-----------------
1. Пользователь вводит числа в TextBox
2. Нажимает кнопку операции -> Operation_Click сохраняет операцию
3. Нажимает "Вычислить" -> Calculate_Click:
   a) Валидирует ввод
   b) Убирает ведущие нули
   c) Выполняет операцию
   d) Показывает результат

================================================================================
*/
