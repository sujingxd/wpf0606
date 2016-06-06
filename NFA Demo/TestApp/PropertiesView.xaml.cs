using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Reflection;

namespace TestApp
{
	/// <summary>
	/// Interaction logic for PropertiesView.xaml
	/// </summary>
	public partial class PropertiesView : UserControl
	{
		private object _selectedObject;
        private Dictionary<string, string> _messsageMap;

		public object SelectedObject
		{
			get { return _selectedObject; }
			set
			{
				if (_selectedObject != value)
				{
					var obj = _selectedObject as INotifyPropertyChanged;
					if (obj != null)
						obj.PropertyChanged -= PropertyChanged;

					_selectedObject = value;
					DisplayProperties();

					obj = _selectedObject as INotifyPropertyChanged;
					if (obj != null)
						obj.PropertyChanged += PropertyChanged;
				}
			}
		}

        public Dictionary<string, string> MessageMap
        {
            set { _messsageMap = value; DisplayProperties(); }
        }

		public PropertiesView()
		{
			InitializeComponent();
			DisplayProperties();
		}

		void PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			DisplayProperties();
		}

		private void DisplayProperties()
		{
			_panel.Children.Clear();
			ClearGrid();
			if (SelectedObject != null)
			{
				int row = 0;
				foreach (var prop in SelectedObject.GetType().GetProperties())
				{
					var attr = prop.GetCustomAttributes(typeof(BrowsableAttribute), true);
					if (attr.Length == 0 || (attr[0] as BrowsableAttribute).Browsable)
					{
						DisplayProperty(prop, row);
						row+=2;
					}
				}
				_panel.Children.Add(_gridContainer);
			}
			else
			{
				_panel.Children.Add(_label);
			}
		}

		private void ClearGrid()
		{
			_grid.RowDefinitions.Clear();
			for (int i = _grid.Children.Count - 1; i >= 0; i--)
			{
				if (_grid.Children[i] != _vLine && _grid.Children[i] != _splitter)
					_grid.Children.RemoveAt(i);
			}
		}

		private void DisplayProperty(PropertyInfo prop, int row)
		{            
            var rowLabel = new RowDefinition();
            rowLabel.Height = new GridLength(Math.Max(20, this.FontSize * 2));
			_grid.RowDefinitions.Add(rowLabel);
            var tb = new TextBlock() { Text = prop.Name };
            tb.Margin = new Thickness(4);
		    Grid.SetRow(tb, _grid.RowDefinitions.Count - 1);

            var line1 = new Line();
            line1.Style = (Style)Resources["gridHorizontalLineStyle"];
            Grid.SetRow(line1, row);
            ScrollViewer s = new ScrollViewer();

            _grid.Children.Add(tb);
            _grid.Children.Add(line1);

            var rowContent = new RowDefinition();
            _grid.RowDefinitions.Add(rowContent);
            if (prop.Name != "Message")
            {
                var ed = new TextBox();
                ed.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                ed.TextWrapping = TextWrapping.Wrap;
                ed.PreviewKeyDown += new KeyEventHandler(ed_KeyDown);
                ed.Margin = new Thickness(4);
                ed.BorderThickness = new Thickness(0);
                ed.AcceptsReturn = true;

                var binding = new Binding(prop.Name);
                binding.Source = SelectedObject;
                binding.ValidatesOnExceptions = true;
                binding.Mode = BindingMode.OneWay;
                if (prop.CanWrite)
                {
                    var mi = prop.GetSetMethod();
                    if (mi != null && mi.IsPublic)
                        binding.Mode = BindingMode.TwoWay;
                }
                ed.SetBinding(TextBox.TextProperty, binding);

                Grid.SetRow(ed, _grid.RowDefinitions.Count - 1);
                _grid.Children.Add(ed);
            }
            else
            {
                rowContent.Height = new GridLength(Math.Max(20, this.FontSize * 2));
                var cb = new ComboBox();
                cb.ItemsSource = _messsageMap;
                cb.DisplayMemberPath = "Key";
                cb.SelectedValuePath = "Key";

                var binding = new Binding(prop.Name);
                binding.Source = SelectedObject;
                binding.ValidatesOnExceptions = true;
                binding.Mode = BindingMode.TwoWay;
                cb.SetBinding(ComboBox.TextProperty, binding);

                Grid.SetRow(cb, _grid.RowDefinitions.Count - 1);
                _grid.Children.Add(cb);
            }
			
            var line2 = new Line();
            line2.Style = (Style)Resources["gridHorizontalLineStyle"];
            Grid.SetRow(line2, row+1);
            _grid.Children.Add(line2);
           

//          var template = (ControlTemplate)Resources["validationErrorTemplate"];
// 			Validation.SetErrorTemplate(ed, template);

        }

		void ed_KeyDown(object sender, KeyEventArgs e)
		{
			var ed = sender as TextBox;
			if (ed != null)
			{
// 				if (e.Key == Key.Enter)
// 				{
// 					ed.GetBindingExpression(TextBox.TextProperty).UpdateSource();
// 					e.Handled = true;
// 				}
// 				else 
                if (e.Key == Key.Escape)
					ed.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
			}
		}
	}
}
