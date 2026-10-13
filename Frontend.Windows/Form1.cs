using System.Globalization;
using Backend;

namespace Frontend.Windows
{
    public partial class Form1 : Form
    {
        private readonly Label _display = new();
        private readonly Font _buttonFont = new("Segoe UI", 16F);
        private readonly Font _displayFontLarge = new("Segoe UI", 20F);
        private readonly Font _displayFontMedium = new("Segoe UI", 15F);
        private readonly Font _displayFontSmall = new("Segoe UI", 11F);

        private string _expression = string.Empty;
        private string _result = string.Empty;
        private bool _resultIsNumber;
        private bool _showingResult;

        public Form1()
        {
            InitializeComponent();
            BuildCalculator();
        }

        private void BuildCalculator()
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 5,
                BackColor = Color.Black,
                Padding = new Padding(6),
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 13F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
            for (var row = 0; row < 5; row++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            }

            _display.Dock = DockStyle.Fill;
            _display.Margin = new Padding(3);
            _display.BackColor = Color.Green;
            _display.ForeColor = Color.White;
            _display.BorderStyle = BorderStyle.FixedSingle;
            _display.TextAlign = ContentAlignment.MiddleLeft;
            _display.AutoSize = false;
            _display.Font = _displayFontLarge;
            table.Controls.Add(_display, 0, 0);
            table.SetColumnSpan(_display, 6);

            var buttons = new (string Text, int Row, int Column, int ColumnSpan)[]
            {
                ("7", 1, 0, 1), ("8", 1, 1, 1), ("9", 1, 2, 1), ("(", 1, 3, 1), (")", 1, 4, 1), ("Delete", 1, 5, 1),
                ("4", 2, 0, 1), ("5", 2, 1, 1), ("6", 2, 2, 1), ("*", 2, 3, 1), ("/", 2, 4, 1), ("Clear", 2, 5, 1),
                ("1", 3, 0, 1), ("2", 3, 1, 1), ("3", 3, 2, 1), ("+", 3, 3, 1), ("-", 3, 4, 1), ("^", 3, 5, 1),
                ("0", 4, 0, 2), (".", 4, 2, 1), ("=", 4, 3, 3),
            };

            foreach (var (text, row, column, columnSpan) in buttons)
            {
                var isNumberButton = char.IsAsciiDigit(text[0]) || text == ".";
                var button = CreateButton(text, isNumberButton ? Color.White : Color.DarkOrange);
                table.Controls.Add(button, column, row);
                table.SetColumnSpan(button, columnSpan);
            }

            Controls.Add(table);
            UpdateDisplay();
        }

        private Button CreateButton(string text, Color backColor)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                BackColor = backColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = _buttonFont,
                TabStop = false,
            };
            button.FlatAppearance.BorderColor = Color.Gainsboro;
            button.Click += Button_Click;
            return button;
        }

        private void Button_Click(object? sender, EventArgs e)
        {
            var text = ((Button)sender!).Text;
            switch (text)
            {
                case "Clear":
                    Clear();
                    break;
                case "Delete":
                    DeleteLast();
                    break;
                case "=":
                    Evaluate();
                    break;
                default:
                    Append(text);
                    break;
            }

            UpdateDisplay();
        }

        private void Append(string text)
        {
            if (_showingResult)
            {
                var isOperator = text is "+" or "-" or "*" or "/" or "^";
                _expression = isOperator && _resultIsNumber ? _result + text : text;
                _showingResult = false;
            }
            else
            {
                _expression += text;
            }
        }

        private void DeleteLast()
        {
            if (_showingResult)
            {
                _showingResult = false;
            }
            else if (_expression.Length > 0)
            {
                _expression = _expression[..^1];
            }
        }

        private void Clear()
        {
            _expression = string.Empty;
            _result = string.Empty;
            _resultIsNumber = false;
            _showingResult = false;
        }

        private void Evaluate()
        {
            if (_showingResult || _expression.Length == 0)
            {
                return;
            }

            try
            {
                var value = ExpressionEvaluator.Evalute(_expression);
                _result = value.ToString(CultureInfo.InvariantCulture);
                _resultIsNumber = true;
            }
            catch (Exception)
            {
                _result = "Error";
                _resultIsNumber = false;
            }

            _showingResult = true;
        }

        private void UpdateDisplay()
        {
            var text = _showingResult ? $"{_expression}={_result}" : _expression;
            _display.Text = text;
            _display.Font = text.Length > 30 ? _displayFontSmall : text.Length > 18 ? _displayFontMedium : _displayFontLarge;
        }
    }
}