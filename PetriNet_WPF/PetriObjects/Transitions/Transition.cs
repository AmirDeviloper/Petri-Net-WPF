using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace AppGUI_WPF.PetriObjects
{
    public class Transition : Item
    {
        public Transition(ref int last_id, double doubles, Canvas canvas) : base(ref last_id, doubles, canvas)
        {
            Shape = new Rectangle();
            Shape.Width = 8;
            Shape.Height = 70;
        }


        public override void Render()
        {
            MainRender();
            Canvas.Children.Add(this);
        }

        public override void Resize(int value)
        {
            this.Width = value * 12;
            this.Height = value * 20;

            Shape.Width = value * 4;
            Shape.Height = value * 12;

            NickNameText.FontSize = value * 2;

            NickNameText.Margin = new Thickness(this.Width - 55, this.Height / 2 - NickNameText.ActualHeight + 20, 0, 0);
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public override Point GetArrowPoints(bool isStartPoint)
        {
            throw new NotImplementedException();

            //return new Point(Shape.Width);
        }

        public override string GetXAML()
        {
            return $"<Transition|{GetItemXAML()} ~ Connected Items|{GetConnectedItemNamesXAML()}/>";
        }

        public override char GetItemType => 'T';
    }
}
