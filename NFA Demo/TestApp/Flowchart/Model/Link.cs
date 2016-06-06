using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace TestApp.Flowchart
{
	class Link: INotifyPropertyChanged
	{
		[Browsable(false)]
		public FlowNode Source { get; private set; }
		[Browsable(false)]
		public PortKinds SourcePort { get; private set; }
		[Browsable(false)]
		public FlowNode Target { get; private set; }
		[Browsable(false)]
		public PortKinds TargetPort { get; private set; }
        [Browsable(false)]
        public Point? ControlPoint1 { get; private set; }
        [Browsable(false)]
        public Point? ControlPoint2 { get; private set; }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }

        private string _action;
        public string Action
        {
            get { return _action; }
            set
            {
                _action = value;
                OnPropertyChanged("Action");
            }
        }

        public Link(FlowNode source, PortKinds sourcePort, FlowNode target, PortKinds targetPort,
            Point? controlPoint1, Point? controlPoint2, string message, string action)
		{
			Source = source;
			SourcePort = sourcePort;
			Target = target;
			TargetPort = targetPort;
            ControlPoint1 = controlPoint1;
            ControlPoint2 = controlPoint2;
            Message = message;
            Action = action;
        }

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion
	}

	enum PortKinds { Top, Bottom, Left, Right }
}
