using AppGUI_WPF.PetriObjects;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace AppGUI_WPF
{
    public partial class MainWindow : Window
    {
        // change name undo has bug.

        private List<Item> _items;
        private Stack<List<Item>> _undoHistory;
        private Stack<List<Item>> _redoHistory;
        private Grid _chessGrid;


        private int playersTurn;
        private int lastPlaceID;
        private int lastTransitionID;
        private int lastThinRectangleID;
        private int isSimultaneous;
        private int isChessGridActive;

        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        private void ResetIds()
        {
            lastPlaceID = 0;
            lastTransitionID = 0;
            lastThinRectangleID = 0;
        }
        public MainWindow()
        {
            _items = new List<Item>();
            _undoHistory = new Stack<List<Item>>();
            _redoHistory = new Stack<List<Item>>();
            _chessGrid = new Grid();

            _undoHistory.Push(new List<Item>(_items));
            _redoHistory.Push(new List<Item>(_items));

            playersTurn = -1;
            isSimultaneous = -1;
            isChessGridActive = -1;

            ResetIds();

            InitializeComponent();
            InitilizeChessGrid();

            items_name_list.FontSize = 17.5D;

            players_turn_button_Click(null, null);
            mode_button_Click(null, null);
            chess_mode_button_Click(null, null);

            var dispatcherTimer = new DispatcherTimer();

            KeyDown += MainWindow_KeyDown;

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            UpdateArrows();
        }

        private void InitilizeChessGrid()
        {
            for (int i = 31; i < 265; i++)
            {
                for (int j = 6; j < 180; j++)
                {
                    var cell = new Border();
                    cell.Margin = new Thickness(i * 5, j * 5, 185, 10);
                    cell.Background = (i + j) % 2 == 0 ? Brushes.DarkGray : Brushes.WhiteSmoke;

                    Grid.SetColumn(cell, i);
                    Grid.SetRow(cell, j);
                    _chessGrid.Children.Add(cell);
                }
            }

            canvas.Children.Add(_chessGrid);
        }

        public void UpdateArrows()
        {
            var lines = canvas.Children.OfType<Line>().ToList();
            foreach (var line in lines)
            {
                canvas.Children.Remove(line);
            }

            var polygons = canvas.Children.OfType<Polygon>().ToList();
            foreach (var polygon in polygons)
            {
                canvas.Children.Remove(polygon);
            }

            foreach (var item in _items)
            {
                foreach (var connected_item in item.ConnectedItems)
                {

                    var shape1 = item.TransformToAncestor(this).Transform(new Point(0, 0));
                    var shape2 = connected_item.TransformToAncestor(this).Transform(new Point(0, 0));

                    shape1.X = shape1.X + item.ActualWidth / 2;
                    shape1.Y = shape1.Y + item.ActualHeight / 2;

                    shape2.X = shape2.X + connected_item.Width / 2;
                    shape2.Y = shape2.Y + connected_item.Height / 2;

                    int a = (int)size_slider.Value;

                    if(item.IsPlace)
                    {
                        shape1 = new Point(shape1.X + (a * 3.90), shape1.Y);
                    }
                    else
                    {
                        shape1 = new Point(shape1.X + (a * 2.10), shape1.Y);
                    }

                    if (connected_item.IsPlace)
                    {
                        shape2 = new Point(shape2.X - (a * 3.90), shape2.Y);
                    }
                    else
                    {
                        shape2 = new Point(shape2.X - (a * 2.10), shape2.Y);
                    }

                    DrawArrow(shape1, shape2);
                }
            }
        }
        public void DrawArrow(Point startPoint, Point endPoint)
        {
            int a = (int)size_slider.Value;

            //startPoint = new Point(startPoint.X + (a * 3), startPoint.Y);
            //endPoint = new Point(endPoint.X - (a * 3), endPoint.Y);

            Line line = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1.3
            };

            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
            Polygon arrowHead = new Polygon
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Black
            };
            arrowHead.Points.Add(new Point(endPoint.X, endPoint.Y));
            arrowHead.Points.Add(new Point(endPoint.X - a * Math.Cos(angle - Math.PI / 6), endPoint.Y - a * Math.Sin(angle - Math.PI / 6)));
            arrowHead.Points.Add(new Point(endPoint.X - a * Math.Cos(angle + Math.PI / 6), endPoint.Y - a * Math.Sin(angle + Math.PI / 6)));

            canvas.Children.Add(line);
            canvas.Children.Add(arrowHead);
        }

        private void ActiveChessGrid()
        {
            canvas.Children.Add(_chessGrid);
        }
        private void DeactiveChessGrid()
        {
            canvas.Children.Remove(_chessGrid);
        }

        private void SelectAllItems()
        {
            foreach (var item in _items)
            {
                item.Select();
            }
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.A)) 
            {
                SelectAllItems();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z)) 
            {
                undo_button_Click(null, null);
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.Z)) 
            {
                redo_button_Click(null, null);
            }

            if (Keyboard.IsKeyDown(Key.Delete))
            {
                RemoveSelectedItems();
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.V))
            {
                copy_items_button_Click(null, null);
            }

            if (Keyboard.IsKeyDown(Key.F2))
            {
                like_transition_button_Click(null, null);
            }
            if (Keyboard.IsKeyDown(Key.F3))
            {
                transition_button_Click(null, null);
            }
            if (Keyboard.IsKeyDown(Key.F4))
            {
                place_button_Click(null, null);
            }
            if (Keyboard.IsKeyDown(Key.F5))
            {
                connect_arc_Click(null, null);
            }

        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            place_arc_text.IsEnabled = _items.Any(i => i.IsSelected && i.IsPlace);
            WriteItemsName();
        }

        private void AddNewItem(Item item)
        {
            item.Render();
            _items.Add(item);
            WriteItemsName();
            _undoHistory.Push(new List<Item>(_items));
        }

        private void DrawLine(Point startPoint, Point endPoint)
        {
            Line line = new Line()
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1.3D
            };

            canvas.Children.Add(line);

            Polygon triangle = new Polygon()
            {
                Fill = Brushes.Black,
                StrokeThickness = 5,
                Points = new PointCollection() { new Point(endPoint.X, endPoint.Y - 5), new Point(endPoint.X + 5, endPoint.Y), new Point(endPoint.X, endPoint.Y + 5) }
            };

            canvas.Children.Add(triangle);
        }

        private void size_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            foreach (var item in _items)
            {
                int silider_size = (int)size_slider.Value;
                item.Resize(silider_size);
            }
            UpdateArrows();
        }

        private void WriteItemsName()
        {
            items_name_list.Items.Clear();
            foreach (var item in _items)
            {
                var listBoxItem = new ListBoxItem();
                listBoxItem.Foreground = item.Shape.Stroke;
                listBoxItem.Content = item.ToString();
                listBoxItem.Background = item.IsFirstPlayer ? Brushes.LightSkyBlue : Brushes.IndianRed;
                items_name_list.Items.Add(listBoxItem);
            }
        }

        private void WriteArcsName()
        {
            arcs_name_list.Items.Clear();
            foreach (var item1 in _items)
            {
                foreach (var item2 in item1.ConnectedItems)
                {
                    var listBoxItem = new ListBoxItem();
                    listBoxItem.Foreground = Brushes.Black;
                    listBoxItem.Content = $"{item1} -> {item2}";
                    listBoxItem.Background = Brushes.BlueViolet;
                    arcs_name_list.Items.Add(listBoxItem);
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var loc_1 = _items[0].Shape.PointToScreen(new Point(0, 0));
            var loc_2 = _items[1].Shape.PointToScreen(new Point(0, 0));

            DrawLine(loc_1, loc_2);
        }

        private bool IsYourTurn()
        {
            return playersTurn == 1;
        }

        private void ChangePlayTurn()
        {
            playersTurn *= -1;
        }

        private void players_turn_button_Click(object sender, RoutedEventArgs e)
        {
            ChangePlayTurn();
            players_turn_button.Content = IsYourTurn() ? "Player 1 Turn": "Player 2 Turn";
            players_turn_button.Foreground = IsYourTurn() ? Brushes.Blue : Brushes.Red;
        }

        private void transition_button_Click (object sender, RoutedEventArgs e)
        {
            if (IsYourTurn())
            {
                AddNewItem(new NoneDottedRoundTransition(ref lastTransitionID, canvas));
            }
            else
            {
                AddNewItem(new DottedRoundTransition(ref lastTransitionID, canvas));
            }

            size_slider_ValueChanged(null, null);

            if(!IsSimultaneous())
            {
                players_turn_button_Click(null, null);
            }
        }

        private void place_button_Click (object sender, RoutedEventArgs e)
        {
            if (IsYourTurn())
            {
                AddNewItem(new NoneDottedRoundPlace(ref lastPlaceID, canvas));
            }
            else
            {
                AddNewItem(new DottedRoundPlace(ref lastPlaceID, canvas));
            }

            size_slider_ValueChanged(null, null);
            if (!IsSimultaneous())
            {
                players_turn_button_Click(null, null);
            }
        }


        private void place_arc_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //var selected_places = _items.Where(i => i.IsSelected && i.IsPlace).ToList();
            //if (selected_places.Count > 0)
            //{
            //    foreach (var item in selected_places)
            //    {
            //        (item as Place).SetTokensCount((int)place_arc_slider.Value);
            //    }
            //    size_slider_ValueChanged(null, null);
            //    WriteItemsName();
            //}
        }

        private void set_new_name_button_Click(object sender, RoutedEventArgs e)
        {
            var selected_items_count = _items.Count(i => i.IsSelected);
            
            if(string.IsNullOrWhiteSpace(new_item_name_text.Text))
            {
                MessageBox.Show("Enter Valid Name For It.");
            }
            else if (selected_items_count == 0)
            {
                MessageBox.Show("Select At Least One Item To Change It's Name.");
            }
            else if (selected_items_count > 1)
            {
                MessageBox.Show("Select Just ONE Item To Change It's Name.");
            }
            else if (!CodeDomProvider.CreateProvider("C#").IsValidIdentifier(new_item_name_text.Text))
            {
                MessageBox.Show($"[{new_item_name_text.Text}] is Not Valid Identifer Name. Choose Another Name.");
            }
            else if(_items.Any(i => i.Name == new_item_name_text.Text))
            {
                MessageBox.Show($"[{new_item_name_text.Text}] Already Exists. Choose Another Name.");
            }
            else if(new_item_name_text.Text.Contains("|") || new_item_name_text.Text.Contains("~"))
            {
                MessageBox.Show("You Can Not Use Charecters \'|\' and \'~\'.");
            }
            else
            {
                _undoHistory.Push(new List<Item>(_items));
                var selected_item = _items.FirstOrDefault(i => i.IsSelected);
                var prev_name = selected_item.Name;
                //selected_item.PrevName = prev_name;
                selected_item.SetNewName(new_item_name_text.Text);
                WriteItemsName();
                MessageBox.Show($"Item [{prev_name}] Names Changed To [{selected_item.Name}]");
                new_item_name_text.Text = string.Empty;
                selected_item.Unselect();
            }

            size_slider_ValueChanged(null, null);
        }

        private void RemoveItems(List<Item> selected_items)
        {
            if (selected_items.Count > 0)
            {
                var message = $"Are You Sure To Remove Selected Elements Below?\n[{string.Join(", ", selected_items)}]";
                if (MessageBox.Show(message, "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    foreach (var item in selected_items)
                    {
                        _items.Remove(item);
                        canvas.Children.Remove(item);
                    }
                    size_slider_ValueChanged(null, null);
                    WriteItemsName();
                    _undoHistory.Push(new List<Item>(_items));
                }
            }
            else
            {
                MessageBox.Show("Select At Least One Item To Delete.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void RemoveSelectedItems()
        {
            RemoveItems(_items.Where(i => i.IsSelected).ToList());
        }


        private void remove_item_button_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedItems();
        }

        private void unselect_items_button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items.Where(i => i.IsSelected))
            {
                item.Unselect();
            }
        }

        private void select_items_button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items.Where(i => !i.IsSelected))
            {
                item.Select();
            }
        }

        private void reverse_items_button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
            {
                if (item.IsSelected) item.Unselect();
                else item.Select();
            }
        }

        private bool IsSimultaneous()
        {
            return isSimultaneous == 1;
        }

        private void mode_button_Click(object sender, RoutedEventArgs e)
        {
            isSimultaneous *= -1;
            players_turn_button.IsEnabled = IsSimultaneous() ? true: false;
            mode_button.Content = IsSimultaneous() ? "Mode: Simultaneous" : "Mode: Turn Base";
        }

        private void select_all_arcs_button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items.Where(i => !i.IsSelected && i.IsPlace))
            {
                item.Select();
            }
        }

        private void items_name_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected_item = (items_name_list.SelectedItem as ListBoxItem);
            if(selected_item != null)
            {
                var name = selected_item.Content.ToString();
                _items.FirstOrDefault(i => i.ToString() == name).Toggle();
            }
        }

        private void select_player_1_all_iems_button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items.Where(i => i.IsFirstPlayer))
            {
                item.Select();
            }
        }

        private void uselect_player_1_all_iems_button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items.Where(i => i.IsFirstPlayer))
            {
                item.Unselect();
            }
        }

        private void select_player_2_all_iems_button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items.Where(i => !i.IsFirstPlayer))
            {
                item.Select();
            }
        }

        private void uselect_player_2_all_iems_button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items.Where(i => !i.IsFirstPlayer))
            {
                item.Unselect();
            }
        }

        private void undo_button_Click(object sender, RoutedEventArgs e)
        {
            if (_undoHistory.Count > 1)
            {
                foreach (var item in _items)
                {
                    canvas.Children.Remove(item);
                }

                _redoHistory.Push(new List<Item>(_undoHistory.Pop()));
                _items = new List<Item>(_undoHistory.Peek());
                
                foreach (var item in _items)
                {
                    //item.UndoName();
                    canvas.Children.Add(item);
                }

                size_slider_ValueChanged(null, null);

                undo_button.IsEnabled = _undoHistory.Count > 1;
                redo_button.IsEnabled = _redoHistory.Count > 0;
            }
        }

        private void redo_button_Click(object sender, RoutedEventArgs e)
        {
            if (_redoHistory.Count > 1)
            {
                foreach (var item in _items)
                {
                    canvas.Children.Remove(item);
                }

                var items = _redoHistory.Pop();
                _undoHistory.Push(new List<Item>(items));
                _items = new List<Item>(items);

                foreach (var item in _items)
                {
                    canvas.Children.Add(item);
                }

                size_slider_ValueChanged(null, null);

                undo_button.IsEnabled = _undoHistory.Count > 1;
                redo_button.IsEnabled = _redoHistory.Count > 1;
            }
        }

        private void copy_items_button_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = _items.Where(i => i.IsSelected);
            var copyItems = new List<Item>();

            if(selectedItems.Any())
            {
                foreach (var selectedItem in selectedItems)
                {
                    if (selectedItem.GetType() == typeof(NoneDottedRoundPlace))
                    {
                        copyItems.Add(new NoneDottedRoundPlace(ref lastPlaceID, canvas));
                    }
                    else if (selectedItem.GetType() == typeof(DottedRoundPlace))
                    {
                        copyItems.Add(new DottedRoundPlace(ref lastPlaceID, canvas));
                    }
                    else if (selectedItem.GetType() == typeof(NoneDottedRoundTransition))
                    {
                        copyItems.Add(new NoneDottedRoundTransition(ref lastTransitionID, canvas));
                    }
                    else if (selectedItem.GetType() == typeof(DottedRoundTransition))
                    {
                        copyItems.Add(new DottedRoundTransition(ref lastTransitionID, canvas));
                    }

                    selectedItem.Unselect();
                }

                foreach (var copyItem in copyItems)
                {
                    copyItem.MoveObject();
                    AddNewItem(copyItem);
                }

                size_slider_ValueChanged(null, null);
            }

            else
            {
                MessageBox.Show("Select At Least One Item To Copy.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }


        }

        private void chess_mode_button_Click(object sender, RoutedEventArgs e)
        {
            if(isChessGridActive == 1)
            {
                ActiveChessGrid();
            }
            else
            {
                DeactiveChessGrid();
            }
            chess_mode_button.Content = isChessGridActive == 1 ? "Chess Mode: Active" : "Chess Mode: Not Active";
            isChessGridActive *= -1;

            foreach (var item in _items)
            {
                canvas.Children.Remove(item);
            }

            foreach (var item in _items)
            {
                canvas.Children.Add(item);
            }

        }

        private void clear_page_button_Click(object sender, RoutedEventArgs e)
        {
            SelectAllItems();
            RemoveItems(_items.Where(i => i.IsSelected).ToList());
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(int.TryParse(place_arc_text.Text, out int place_arcs))
            {
                var selected_places = _items.Where(i => i.IsSelected && i.IsPlace).ToList();
                if (selected_places.Count > 0)
                {
                    foreach (var item in selected_places)
                    {
                        (item as Place).SetTokensCount(place_arcs);
                    }
                    size_slider_ValueChanged(null, null);
                    WriteItemsName();
                    WriteArcsName();
                }
            }
            else
            {
                place_arc_text.Text = string.Empty;
            }
        }

        private void place_arc_slider_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void like_transition_button_Click(object sender, RoutedEventArgs e)
        {
            if (IsYourTurn())
            {
                AddNewItem(new NoneDottedRoundThinRectangle(ref lastThinRectangleID, canvas));
            }
            else
            {
                AddNewItem(new DottedThinRectangle(ref lastThinRectangleID, canvas));
            }

            size_slider_ValueChanged(null, null);

            if (!IsSimultaneous())
            {
                players_turn_button_Click(null, null);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }

        private void ConnectArcs(List<Item> selected_items)
        {
            if (selected_items.Count != 2)
            {
                MessageBox.Show("Please Selected Just 2 Items");
                return;
            }

            var type_1 = selected_items[0].GetType().BaseType;
            var type_2 = selected_items[1].GetType().BaseType;


            if ((type_1 == typeof(Place) && type_2 == typeof(Transition)) || (type_1 == typeof(Transition) && type_2 == typeof(Place)))
            {
                selected_items[0].ConnectNewItem(selected_items[1]);
                WriteArcsName();
                UpdateArrows();
                unselect_items_button_Click(null, null);
            }
            else
            {
                MessageBox.Show("You Should Connect Place To Transition OR Transintion To Place");
                return;
            }
        }

        private void connect_arc_Click (object sender, RoutedEventArgs e)
        {
            var selected_items = _items.Where(i => i.IsSelected).OrderBy(i => i.SelectedDate).ToList();
            ConnectArcs(selected_items);
        }

        private void remove_arcs_button_Click(object sender, RoutedEventArgs e)
        {
            
            arcs_name_list.Items.Clear();
            foreach (var item1 in _items)
            {
                var items = item1.ConnectedItems;
                try
                {
                    foreach (var item2 in items)
                    {
                        item1.ConnectNewItem(item2);
                    }
                }
                catch (Exception)
                {

                }
            }
            
            WriteArcsName();
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog.Title = "Save To XML File";

            if (saveFileDialog.ShowDialog() == true)
            {
                var text_result = $"Size: {size_slider.Value}\n";
                foreach (var item in _items)
                {
                    text_result += $"{item.GetXAML()}\n";
                }

                try
                {
                    File.WriteAllText(saveFileDialog.FileName, text_result);
                    MessageBox.Show("File saved successfully!", "Success", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"File not saved because:\n{ex.Message}", "Failed", MessageBoxButton.OK);
                }
            }
        }

        private void load_button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Select an XML File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                ReadXmlFile(filePath);
            }
        }

        private void ReadXmlFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length == 0) 
                {
                    MessageBox.Show("The File is Empty.");
                }
                else
                {
                    ResetIds();
                    clear_page_button_Click(null, null);

                    var objects_size = float.Parse(lines[0].Split()[1]);
                    size_slider.Value = objects_size;

                    var connected_items = new Dictionary<string, List<string>>();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = lines[i].Replace("<", "").Replace("/>", "");
                        var parts = line.Split('~');
                        
                        var item_info_part = parts[0].Split('|');

                        var item_type = item_info_part[0];
                        var item_subtype = item_info_part[1];
                        var item_name = item_info_part[2];

                        var location_xy = item_info_part[3].Replace(")", "").Replace("(", "").Split(',');
                        var location_x = int.Parse(location_xy[0]);
                        var location_y = int.Parse(location_xy[1]);
                        var location = new Point(location_x, location_y);

                        var tokens_count = -1;
                        if(item_type == "Place")
                        {
                            tokens_count = int.Parse(item_info_part[4]);
                        }

                        if (item_subtype == "DottedRoundPlace") 
                        {
                            AddNewItem(new DottedRoundPlace(ref lastThinRectangleID, canvas));
                            (_items[_items.Count - 1] as DottedRoundPlace).SetTokensCount(tokens_count);
                        }
                        else if (item_subtype == "NoneDottedRoundPlace")
                        {
                            AddNewItem(new NoneDottedRoundPlace(ref lastThinRectangleID, canvas));
                            (_items[_items.Count - 1] as NoneDottedRoundPlace).SetTokensCount(tokens_count);
                        }
                        else if (item_subtype == "DottedThinRectangle")
                        {
                            AddNewItem(new DottedThinRectangle(ref lastThinRectangleID, canvas));
                        }
                        else if (item_subtype == "NoneDottedRoundThinRectangle")
                        {
                            AddNewItem(new NoneDottedRoundThinRectangle(ref lastThinRectangleID, canvas));
                        }
                        else if (item_subtype == "DottedRoundTransition")
                        {
                            AddNewItem(new DottedRoundTransition(ref lastTransitionID, canvas));
                        }
                        else if (item_subtype == "NoneDottedRoundTransition")
                        {
                            AddNewItem(new NoneDottedRoundTransition(ref lastTransitionID, canvas));
                        }
                        else
                        {
                            MessageBox.Show($"[{item_subtype}] Not Found.");
                        }

                        _items[_items.Count - 1].SetNewName(item_name);
                        _items[_items.Count - 1].SetLocation(location);
                        _items[_items.Count - 1].MoveObject();

                        var connected_items_part = parts[1].Split('|');
                        if (connected_items_part.Length > 1 && connected_items_part[1] != "") 
                        {
                            var temp_connected_items = new List<string>();

                            for (int j = 1; j < connected_items_part.Length; j++)
                            {
                                temp_connected_items.Add(connected_items_part[j]);
                            }
                            connected_items.Add(item_name, temp_connected_items);
                        }

                    }

                    
                    size_slider.Value--;
                    size_slider.Value++;

                    foreach (var connected_item in connected_items)
                    {
                        foreach (var destination_name in connected_item.Value)
                        {
                            var source_item = _items.FirstOrDefault(i => i.Name == connected_item.Key);
                            var destination_item = _items.FirstOrDefault(i => i.Name == destination_name);

                            ConnectArcs(new List<Item>() { source_item , destination_item });
                        }
                    }
                    
                }

            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Error reading file: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
        }
    

        private void remove_selected_arcs_button_Click(object sender, RoutedEventArgs e)
        {
            arcs_name_list.Items.Clear();
            foreach (var item1 in _items.Where(i => i.IsSelected))
            {
                try
                {
                    foreach (var item2 in item1.ConnectedItems)
                    {
                        item1.ConnectNewItem(item2);
                    }
                }
                catch (Exception)
                {

                }
            }

            WriteArcsName();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void clear_tokens_count_button_click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _items)
            {
                item.GetXAML();
            }
            place_arc_text.Text = string.Empty;

            foreach (var item in _items.Where(i => !i.IsSelected && i.IsPlace))
            {
                item.Unselect();
            }

        }
    }
}

