using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Aga.Diagrams;
using Aga.Diagrams.Controls;
using System.Windows.Media;
using System.Windows.Input;




namespace TestApp.Flowchart
{
    public partial class FlowchartEditor : UserControl
    {
        private ItemsControlDragHelper _dragHelper;        
        private Dictionary<string,string> messsageMap;
        private FlowchartModel model;

        public FlowchartEditor()
        {
            InitializeComponent();

            model = CreateModel();
            _editor.Controller = new Controller(_editor, model);
          //  _editor.DragDropTool = new DragDropTool(_editor, model);
           

        _editor.DragTool = new CustomMoveResizeTool(_editor, model)
            {
                // MoveGridCell = _editor.GridCellSize
                //MoveGridCell = "2,2"
            };
            _editor.LinkTool = new CustomLinkTool(_editor);
            _editor.Selection.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Selection_PropertyChanged);
           // _dragHelper = new ItemsControlDragHelper(_toolbox, this);

          //  FillToolbox();

            messsageMap = new Dictionary<string, string>();
            messsageMap["*"] = "";
        }
        private void Ellips_Create(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Point point = Mouse.GetPosition(e.Source as FrameworkElement);
                NodeKinds nk = NodeKinds.Start;
                var node = new FlowNode(nk);
                node.Text = nk.ToString();
                var ui = Controller.CreateContent(node);
                ui.HorizontalAlignment = HorizontalAlignment.Right;
                
                ui.Width = 60;
                ui.Height = 60;
                ui.Margin = new Thickness(5);
                ui.Tag = nk;
                _editor.Children.Add(ui);
                model.Nodes.Add(node);


            }
        }

        public string GetCompiledCodeText()
        {
            string strCodeText = "";
            //compute declaration
            strCodeText += "public void compute(Iterator<";
            strCodeText += "msgType";//todo : replace with project name
            strCodeText += "> messages) throws IOException {\n";
            //initial super step
            try {
                Link initActionLink = model.Links.Where(p => p.Message == "*").First();
                strCodeText += "\tif (0 == getSuperStepCount(){\n";
                //actions
                string[] strActions = initActionLink.Action.Split(';');
                int nActions = strActions.Count();
                for (int i=0; i<nActions; i++){
                    strCodeText += ("\t\t" + strActions[i]);
                }
                strCodeText += "\n\t} else {\n";
            } catch (Exception) { ; }
            //cache the message
            strCodeText += "\t\tmsgType msg;\n";
            strCodeText += "\t\tList<msgType> msgBuf = new ArrayList<>();\n";
            strCodeText += "\t\twhile (messages.hasNext()){\n";
            strCodeText += "\t\t\tmsg = messages.next();\n";
            strCodeText += "\t\t\tmsgBuf.add(msg);\n";
            strCodeText += "\t\t}\n";
            //handle message
            strCodeText += "\t\tfor (int i = 0; i < msgBuf.size(); i++){\n";
            strCodeText += "\t\t\tmsg = msgBuf.get(i);\n";
            strCodeText += "\t\t\tswitch (msg.flag){\n";
            //message action
            foreach (var link in model.Links)
            {
                if (link.Message != "*")
                {
                    strCodeText += "\t\t\t\tcase " + Int32.Parse(link.Message.Substring(4, 1)) + ":\n";
                    strCodeText += "\t\t\t\t{\n";
                    //actions
                    string[] strActions = link.Action.Split(';');
                    int nActions = strActions.Count();
                    for (int i = 0; i < nActions; i++)
                    {
                        strCodeText += ("\t\t\t\t\t" + strActions[i]);
                    }
                    strCodeText += "\n\t\t\t\t\tbreak;" + "\n\t\t\t\t}\n";
                }
            }
            strCodeText += "\t\t\t\tdefault:\n";
            strCodeText += "\t\t\t\t\tbreak;\n";
            strCodeText += "\t\t\t}\n";
            strCodeText += "\t\t}\n";
            strCodeText += "\t}\n";
            strCodeText += "}\n";
            return strCodeText;
        }

        private void OnAddMessage(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.msgParam.Text))
            {
                Button msg = new Button();
                msg.Content = "type" + messsageMap.Count + " : " + this.msgParam.Text;
                this.msgList.Children.Add(msg);
                msg.Click += OnEditMessageContent;
                string strMsgKey = msg.Content as string;
                if (!messsageMap.ContainsKey(strMsgKey))
                {
                    messsageMap.Add(strMsgKey, "");
                }
                else 
                { 
                    messsageMap[strMsgKey] = ""; 
                }
                _propertiesView.MessageMap = messsageMap;
            }
            else
                MessageBox.Show("Please input message name!");
        }

        private void OnEditMessageContent(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string strMsgKey = btn.Content as string;
            if (!messsageMap.ContainsKey(strMsgKey)){
                messsageMap.Add(strMsgKey,"");
            }
            string strMsgContent = "";
            messsageMap.TryGetValue(strMsgKey, out strMsgContent);

            var content = new TextBox()
            {
                BorderBrush = Brushes.SteelBlue,
                BorderThickness = new Thickness(1),
                SnapsToDevicePixels = true,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Padding = new Thickness(10),
               // Margin = new Thickness(10),
             
                Text = strMsgContent
            };
            windows1 w = new windows1()
            {
                Title = "Message defining",
                SnapsToDevicePixels = true,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Content = content,
                Height = 300,
                Width = 300
            };
            w.ShowDialog();
            messsageMap[strMsgKey] = content.Text;
            _propertiesView.MessageMap = messsageMap;
        }

        private void dgButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.btnList1.Children.Count != 0)

            {
                for (int i = this.btnList1.Children.Count - 1; i >= 0; i--)
                    this.btnList1.Children.RemoveAt(i);
            }

            else
                MessageBox.Show("no variable!");
        }

        private void dmButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.msgList.Children.Count != 0)

            {
                for (int i = this.msgList.Children.Count - 1; i >= 0; i--)
                    this.msgList.Children.RemoveAt(i);
            }

            else
                MessageBox.Show("no message!");
        }
        private void dlButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.btnList2.Children.Count != 0)

            {
                for (int i = this.btnList2.Children.Count - 1; i >= 0; i--)
                    this.btnList2.Children.RemoveAt(i);
            }

            else
                MessageBox.Show("no variable!");
        }


        private void gButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.btnName1.Text))
            {
                Button btn = new Button();
                btn.Content = this.btnName1.Text;
                this.btnList1.Children.Add(btn);
            }
            else
                MessageBox.Show("please input button name!");
        }
        private void lButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.btnName2.Text))
            {
                Button btn = new Button();
                btn.Content = this.btnName2.Text;
                this.btnList2.Children.Add(btn);
            }
            else
                MessageBox.Show("please input button name!");
        }
        private void FillToolbox()
        {
            foreach (NodeKinds nk in Enum.GetValues(typeof(NodeKinds)))
            {
                var node = new FlowNode(nk);
                node.Text = nk.ToString();
                var ui = Controller.CreateContent(node);
                ui.HorizontalAlignment = HorizontalAlignment.Right;
                ui.Width = 60;
                ui.Height = 60;
                ui.Margin = new Thickness(5);
                ui.Tag = nk;
                _toolbox.Items.Add(ui);
                // if (2 == _toolbox.Items.Count) break;
                if (1 == _toolbox.Items.Count) break;
            }
        }

        private FlowchartModel CreateModel()
        {
            var model = new FlowchartModel();

            // 			var start = new FlowNode(NodeKinds.Start);
            // 			start.Row = 0;
            // 			start.Column = 1;
            // 			start.Text = "Start";
            // 
            // 			var ac0 = new FlowNode(NodeKinds.Action);
            // 			ac0.Row = 1;
            // 			ac0.Column = 1;
            // 			ac0.Text = "i = 0";
            // 
            // 			var cond = new FlowNode(NodeKinds.Condition);
            // 			cond.Row = 2;
            // 			cond.Column = 1;
            // 			cond.Text = "i < n";
            // 
            // 			var ac1 = new FlowNode(NodeKinds.Action);
            // 			ac1.Row = 3;
            // 			ac1.Column = 1;
            // 			ac1.Text = "do something";
            // 
            // 			var ac2 = new FlowNode(NodeKinds.Action);
            // 			ac2.Row = 4;
            // 			ac2.Column = 1;
            // 			ac2.Text = "i++";
            // 
            // 			var end = new FlowNode(NodeKinds.End);
            // 			end.Row = 3;
            // 			end.Column = 2;
            // 			end.Text = "End";
            // 
            // 			model.Nodes.Add(start);
            // 			model.Nodes.Add(cond);
            // 			model.Nodes.Add(ac0);
            // 			model.Nodes.Add(ac1);
            // 			model.Nodes.Add(ac2);
            // 			model.Nodes.Add(end);
            // 
            // 			model.Links.Add(new Link(start, PortKinds.Bottom, ac0, PortKinds.Top));
            // 			model.Links.Add(new Link(ac0, PortKinds.Bottom, cond, PortKinds.Top));
            // 			
            // 			model.Links.Add(new Link(cond, PortKinds.Bottom, ac1, PortKinds.Top) { Text = "True" });
            // 			model.Links.Add(new Link(cond, PortKinds.Right, end, PortKinds.Top) { Text = "False" });
            // 
            // 			model.Links.Add(new Link(ac1, PortKinds.Bottom, ac2, PortKinds.Top));
            // 			model.Links.Add(new Link(ac2, PortKinds.Bottom, cond, PortKinds.Top));

            return model;
        }

        void Selection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var p = _editor.Selection.Primary;
            _propertiesView.SelectedObject = p != null ? p.ModelElement : null;
        }

     
    }
}
