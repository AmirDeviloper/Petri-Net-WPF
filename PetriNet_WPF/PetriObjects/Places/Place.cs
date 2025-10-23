using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;


namespace AppGUI_WPF.PetriObjects
{
    public class Place : Item
    {
        public TextBlock TokensCountText { get; private set; }
        public Brush TokensCountColor { get; private set; }

        private int _tokensCount;

        public Place(ref int last_id, double doubles, Canvas canvas) : base(ref last_id, doubles, canvas)
        {
            Shape = new Ellipse();
            TokensCountText = new TextBlock();

            Shape.Width = 100;
            Shape.Height = 100;

            _tokensCount = 1;

            SetTokensCount(_tokensCount);
        }

        
        private void GenerateTokensCountText()
        {
            TokensCountText.Text = _tokensCount.ToString();
            TokensCountText.FontSize = 20;
            TokensCountText.Foreground = Brushes.Black;
            TokensCountText.HorizontalAlignment = HorizontalAlignment.Center;
            TokensCountText.VerticalAlignment = VerticalAlignment.Center;

            //Canvas.SetLeft(TokensCountText, Canvas.GetLeft(Shape) + Shape.Width / 2 - TokensCountText.ActualWidth / 2);
            //Canvas.SetTop(TokensCountText, Canvas.GetTop(Shape) + Shape.Height / 2 - TokensCountText.ActualHeight / 2);
        }

        public void SetTokensCount(int tokensCount)
        {
            _tokensCount = Math.Abs(tokensCount);
            TokensCountText.Text = _tokensCount.ToString();
            TokensCountText.FontSize = 20;
        }

        public override void Render()
        {

            MainRender();

            GenerateTokensCountText();
            Children.Add(TokensCountText);

            Canvas.Children.Add(this);
        }


        public override void Resize(int value)
        {
            this.Width = value * 12;
            this.Height = value * 20;

            Shape.Width = value * 8;
            Shape.Height = value * 8;

            TokensCountText.FontSize = (value * 10 / 2) - (TokensCountText.Text.Length * 1.2 * (value / 3));
            NickNameText.FontSize = value * 2;

            NickNameText.Margin = new Thickness(this.Width - 50, this.Height / 2 - NickNameText.ActualHeight + 20, 0, 0);
            //Canvas.SetLeft(TokensCountText, newX + Shape.Width / 2 - TokensCountText.ActualWidth / 2);
            //Canvas.SetTop (TokensCountText, newY + Shape.Height / 2 - TokensCountText.ActualHeight / 2);

            //Canvas.SetLeft(NickNameText, newX - this.Width - 50);
            //Canvas.SetTop(NickNameText, newY + this.Height / 4 - NickNameText.ActualHeight);
        }

        public override string ToString()
        {
            return $"{Name} [{_tokensCount}]";
        }

        public override Point GetArrowPoints(bool isStartPoint)
        {
            throw new NotImplementedException();
        }

        public override string GetXAML()
        {
            return $"<Place|{GetItemXAML()}|{_tokensCount} ~ Connected Items|{GetConnectedItemNamesXAML()}/>";
        }

        public override char GetItemType => 'P';

    }
}
