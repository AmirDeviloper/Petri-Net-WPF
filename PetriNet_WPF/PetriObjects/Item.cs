using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;

namespace AppGUI_WPF.PetriObjects
{
    public abstract class Item: Grid
    {
        protected Canvas Canvas { get; private set; }
        protected TextBlock NickNameText { get; private set; }
        private Brush SelectedColor { get { return Brushes.DarkOrange; } }
        private Brush UnSelectedColor { get { return Brushes.Black; } }
        public string PrevName { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<Item> ConnectedItems { get; private set; }
        private Brush SelectForConnectColor { get { return Brushes.DarkCyan; } }
        public Point LastLocation { get; set; }

        public bool IsFirstPlayer 
        { 
            get 
            {
                return this.GetType() == typeof(NoneDottedRoundPlace) || this.GetType() == typeof(NoneDottedRoundTransition);
            } 
        }

        public bool IsPlace
        {
            get { return this.GetType().BaseType == typeof(Place); }
        }

        public Shape Shape { get; set; }

        public bool IsSelected
        {
            get { return Shape.Stroke == SelectedColor; }
            private set { }
        }

        public bool IsSelectedForConnect
        {
            get { return Shape.Stroke == SelectForConnectColor; }
            private set { }
        }

        private bool _isClicked;
        private DoubleCollection _doublesCollection;
        
        public Item(ref int last_id, double doubles, Canvas canvas)
        {
            _doublesCollection = new DoubleCollection(new double[] { doubles, doubles });
            ConnectedItems = new List<Item>();

            Canvas = canvas;
            NickNameText = new TextBlock();

            SetNewName($"{GetItemType}{last_id++}");
            PrevName = "";

            Width = 120;
            Height = 160;

            MouseDown += Place_MouseDown;
            MouseUp += Place_MouseUp;
            MouseMove += Place_MouseMove;
        }

        public void SetLocation(Point point)
        {
            Canvas.SetLeft(this, point.X);
            Canvas.SetTop(this, point.Y);
        }

        public void ConnectNewItem(Item item)
        {
            if(!ConnectedItems.Contains(item))
            {
                ConnectedItems.Add(item);
            }
            else
            {
                ConnectedItems.Remove(item);
            }
        }

        

        public void CopyName()
        {
            SetNewName($"{NickNameText.Text}_Copy({new Random().Next()})");
        }

        public void MoveObject()
        {
            double left = Canvas.GetLeft(this);
            double top = Canvas.GetTop(this);
            Canvas.SetLeft(this, left + 50);
            Canvas.SetTop(this, top + 50);

            LastLocation = new Point(left, top);
        }

        public void UndoName()
        {
            if(PrevName == Name && PrevName != string.Empty)
            {
                PrevName = "";
            }
            else
            {
                SetNewName(PrevName);
            }
        }
        public void SetNewName(string newName)
        {
            NickNameText.Text = newName;
            Name = NickNameText.Text;
        }

        protected void GenerateShape()
        {
            Shape.Fill = Brushes.White;
            Shape.Stroke = Brushes.Black;
            Shape.StrokeDashArray = _doublesCollection;

            Canvas.SetLeft(Shape, 100);
            Canvas.SetTop(Shape, 100);
        }

        public void Toggle()
        {
            if(IsSelected) Unselect();
            else Select();
        }

        public void Unselect()
        {
            IsSelected = false;
            Shape.Stroke = UnSelectedColor;
            NickNameText.Foreground = UnSelectedColor;
            SelectedDate = DateTime.Now.AddYears(-2000);
        }

        public void Select()
        {
            IsSelected = true;
            Shape.Stroke = SelectedColor;
            NickNameText.Foreground = SelectedColor;
            SelectedDate = DateTime.Now;
        }

        public void SelectForConnect()
        {
            IsSelected = true;
            Shape.Stroke = SelectForConnectColor;
            NickNameText.Foreground = SelectForConnectColor;
        }

        protected void GenerateNickName()
        {
            NickNameText.FontSize = 5;
            NickNameText.Foreground = Brushes.Black;
            NickNameText.HorizontalAlignment = HorizontalAlignment.Right;
            NickNameText.VerticalAlignment = VerticalAlignment.Center;
        }


        public override string ToString()
        {
            return NickNameText.Text; 
        }

        private void Place_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isClicked)
            {
                double newX_Mouse = e.GetPosition(Canvas).X - base.Width / 2;
                double newY_Mouse = e.GetPosition(Canvas).Y - base.Height / 2;

                LastLocation = new Point(newX_Mouse, newY_Mouse);

                SetLocation(LastLocation);
            }
        }

        private void Place_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isClicked)
            {
                ReleaseMouseCapture();
                _isClicked = false;
            }
        }

        private void Place_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftAlt))
            {
                if(IsSelectedForConnect)
                {
                    Unselect();
                }
                else
                {
                    SelectForConnect();
                }
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || e.ClickCount == 2)
            {
                Toggle();
            }

            else
            {
                this.CaptureMouse();
                _isClicked = true;
            }
            
        }

        public void MainRender()
        {
            GenerateShape();
            Children.Add(Shape);

            GenerateNickName();
            Children.Add(NickNameText);
        }

        public abstract void Resize(int value);
        public abstract void Render();
        public abstract char GetItemType { get; }

        public abstract Point GetArrowPoints(bool isStartPoint);
        protected string GetItemXAML()
        {
            return $"{GetType().Name}|{NickNameText.Text}|({LastLocation.X},{LastLocation.Y})";
        }

        protected string GetConnectedItemNamesXAML()
        {
            var item_names = "";
            foreach (var item in ConnectedItems)
            {
                item_names += $"{item.NickNameText.Text}|";
            }

            return item_names.Length == 0 ? "" : item_names.Substring(0, item_names.Length - 1);
        }

        public abstract string GetXAML();
    }
}
