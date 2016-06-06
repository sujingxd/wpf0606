﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aga.Diagrams;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using Aga.Diagrams.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;

namespace TestApp.Flowchart
{
	class Controller : IDiagramController
	{
		private class UpdateScope : IDisposable
		{
			private Controller _parent;
			public bool IsInprogress { get; set; }

			public UpdateScope(Controller parent)
			{
				_parent = parent;
			}

			public void Dispose()
			{
				IsInprogress = false;
				_parent.UpdateView();
			}
		}

		private DiagramView _view;
		private FlowchartModel _model;
		private UpdateScope _updateScope;

		public Controller(DiagramView view, FlowchartModel model)
		{
			_view = view;
			_model = model;
			_model.Nodes.CollectionChanged += NodesCollectionChanged;
			_model.Links.CollectionChanged += LinksCollectionChanged;
			_updateScope = new UpdateScope(this);

			foreach (var t in _model.Nodes)
				t.PropertyChanged += NodePropertyChanged;

			UpdateView();
		}

		void NodesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
				foreach (var t in e.NewItems.OfType<INotifyPropertyChanged>())
					t.PropertyChanged += NodePropertyChanged;

			if (e.OldItems != null)
				foreach (var t in e.OldItems.OfType<INotifyPropertyChanged>())
					t.PropertyChanged -= NodePropertyChanged;
			UpdateView();
		}

		void LinksCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateView();
		}

		void NodePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var fn = sender as FlowNode;
			var n = _view.Children.OfType<Node>().FirstOrDefault(p => p.ModelElement == fn);
			if (fn != null && n != null)
				UpdateNode(fn, n);
		}

		private void UpdateView()
		{
			if (!_updateScope.IsInprogress)
			{
				_view.Children.Clear();

				foreach (var node in _model.Nodes)
					_view.Children.Add(UpdateNode(node, null));

				foreach (var link in _model.Links)
					_view.Children.Add(CreateLink(link));
			}
		}

		private Node UpdateNode(FlowNode node, Node item)
		{
			if (item == null)
			{
				item = new Node();
				item.ModelElement = node;
				CreatePorts(node, item);
				item.Content = CreateContent(node);
			}
            //item.Width = _view.GridCellSize.Width - 50;
            item.Width = 50;
            //item.Height = _view.GridCellSize.Height - 50;
            item.Height = 50;
            item.CanResize = false;
			item.SetValue(Canvas.LeftProperty, node.Column * _view.GridCellSize.Width + 10);
			item.SetValue(Canvas.TopProperty, node.Row * _view.GridCellSize.Height + 25);
			return item;
		}

		public static FrameworkElement CreateContent(FlowNode node)
		{
			var textBlock = new TextBlock()
			{
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center
			};
			var b = new Binding("Text");
			b.Source = node;
			textBlock.SetBinding(TextBlock.TextProperty, b);
            
			if (node.Kind == NodeKinds.Start || node.Kind == NodeKinds.End)
			{
// 				    var ui = new Border();
// 				    ui.CornerRadius = new CornerRadius(50);
// 				    ui.BorderBrush = Brushes.Black;
// 				    ui.BorderThickness = new Thickness(1);
// 				    ui.Background = Brushes.Yellow;
// 				    ui.Child = textBlock;

                var ui = new Ellipse();
                ui.Width = ui.Height = 50;
                ui.Stroke = Brushes.Black;
                ui.Fill = Brushes.Yellow;

                var grid = new Grid();
                grid.Children.Add(ui);
                
                if (node.Kind == NodeKinds.End)
                {
                    var ui2 = new Ellipse();
                    ui2.Width = ui2.Height = 40;
                    ui2.Stroke = Brushes.Black;
                    ui2.Fill = Brushes.Yellow;
                    grid.Children.Add(ui2);
                    if (null == node.Text) node.Text = "end";
                }
                else
                {
                    if (null == node.Text) node.Text = "state";
                }
                grid.Children.Add(textBlock);
                return grid;
			}
			else if (node.Kind == NodeKinds.Action)
			{
				var ui = new Border();
				ui.BorderBrush = Brushes.Black;
				ui.BorderThickness = new Thickness(1);
				ui.Background = Brushes.Lime; ;
				ui.Child = textBlock;
				return ui;
			}
			else
			{
				var ui = new Path();
				ui.Stroke = Brushes.Black;
				ui.StrokeThickness = 1;
				ui.Fill = Brushes.Pink;
				var converter = new GeometryConverter();
				ui.Data = (Geometry)converter.ConvertFrom("M 0,0.25 L 0.5 0 L 1,0.25 L 0.5,0.5 Z");
				ui.Stretch = Stretch.Uniform;

				var grid = new Grid();
				grid.Children.Add(ui);
				grid.Children.Add(textBlock);

				return grid;
			}
		}

		private void CreatePorts(FlowNode node, Node item)
		{
			foreach (var kind in node.GetPorts())
			{
				var port = new Aga.Diagrams.Controls.EllipsePort();
				port.Width = 6;
				port.Height = 6;
				port.Margin = new Thickness(-5);
				port.Visibility = Visibility.Visible;
				port.VerticalAlignment = ToVerticalAligment(kind);
				port.HorizontalAlignment = ToHorizontalAligment(kind);
				port.CanAcceptIncomingLinks = true /*(kind == PortKinds.Top)*/;
				port.CanAcceptOutgoingLinks = true /*!port.CanAcceptIncomingLinks*/;
				port.Tag = kind;
				port.Cursor = Cursors.Cross;
				port.CanCreateLink = true;
				item.Ports.Add(port);
			}
		}

		private Control CreateLink(Link link)
		{
			var item = new OrthogonalLink();
			item.ModelElement = link;
			item.EndCap = true;
			item.Source = FindPort(link.Source, link.SourcePort);
			item.Target = FindPort(link.Target, link.TargetPort);
            item.ControlPoint1 = link.ControlPoint1;
            item.ControlPoint2 = link.ControlPoint2;
            item.Message = link.Message;
            item.Action = link.Action;

			var b = new Binding("Message");
			b.Source = link;
			item.SetBinding(LinkBase.LabelProperty, b);

			return item;
		}


		private Aga.Diagrams.Controls.IPort FindPort(FlowNode node, PortKinds portKind)
		{
			var inode = _view.Items.FirstOrDefault(p => p.ModelElement == node) as Aga.Diagrams.Controls.INode;
			if (inode == null)
				return null;
			var port = inode.Ports.OfType<FrameworkElement>().FirstOrDefault(
				p => p.VerticalAlignment == ToVerticalAligment(portKind)
					&& p.HorizontalAlignment == ToHorizontalAligment(portKind)
				);
			return (Aga.Diagrams.Controls.IPort)port;
		}

		private VerticalAlignment ToVerticalAligment(PortKinds kind)
		{
			if (kind == PortKinds.Top)
				return VerticalAlignment.Top;
			if (kind == PortKinds.Bottom)
				return VerticalAlignment.Bottom;
			else
				return VerticalAlignment.Center;
		}

		private HorizontalAlignment ToHorizontalAligment(PortKinds kind)
		{
			if (kind == PortKinds.Left)
				return HorizontalAlignment.Left;
			if (kind == PortKinds.Right)
				return HorizontalAlignment.Right;
			else
				return HorizontalAlignment.Center;
		}

		private void DeleteSelection()
		{
			using (BeginUpdate())
			{
				var nodes = _view.Selection.Select(p => p.ModelElement as FlowNode).Where(p => p != null);
				var links = _view.Selection.Select(p => p.ModelElement as Link).Where(p => p != null);
				_model.Nodes.RemoveRange(p => nodes.Contains(p));
				_model.Links.RemoveRange(p => links.Contains(p));
				_model.Links.RemoveRange(p => nodes.Contains(p.Source) || nodes.Contains(p.Target));
			}
		}

		private IDisposable BeginUpdate()
		{
			_updateScope.IsInprogress = true;
			return _updateScope;
		}

		#region IDiagramController Members

		public void UpdateItemsBounds(Aga.Diagrams.Controls.DiagramItem[] items, Rect[] bounds)
		{
			for (int i = 0; i < items.Length; i++)
			{
				var node = items[i].ModelElement as FlowNode;
				if (node != null)
				{
					node.Column = (int)(bounds[i].X / _view.GridCellSize.Width);
					node.Row = (int)(bounds[i].Y / _view.GridCellSize.Height);
				}
			}
		}

		public void UpdateLink(LinkInfo initialState, Aga.Diagrams.Controls.ILink link)
		{
			using (BeginUpdate())
			{
				var sourcePort = link.Source as PortBase;
				var source = VisualHelper.FindParent<Node>(sourcePort);
				var targetPort = link.Target as PortBase;
				var target = VisualHelper.FindParent<Node>(targetPort);
                var oldLink = (link as LinkBase).ModelElement as Link;
                var message = "";
                var action = "";
                if (null != oldLink)
                {
                    message = ((link as LinkBase).ModelElement as Link).Message;
                    action = ((link as LinkBase).ModelElement as Link).Action;
                }

				_model.Links.Remove((link as LinkBase).ModelElement as Link);
				_model.Links.Add(
					new Link((FlowNode)source.ModelElement, (PortKinds)sourcePort.Tag, 
						(FlowNode)target.ModelElement, (PortKinds)targetPort.Tag,
                        link.ControlPoint1,link.ControlPoint2,message,action
						));
			}
		}

		public void ExecuteCommand(System.Windows.Input.ICommand command, object parameter)
		{
			if (command == ApplicationCommands.Delete)
				DeleteSelection();
		}

		public bool CanExecuteCommand(System.Windows.Input.ICommand command, object parameter)
		{
			if (command == ApplicationCommands.Delete)
				return true;
			else
				return false;
		}

		#endregion
	}
}
