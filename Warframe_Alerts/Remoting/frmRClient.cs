using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace RemotableObjects
{


	public class frmRCleint
	{
		private System.Windows.Forms.TextBox textBox1;
		
		MyRemotableObject remoteObject;
		
		private System.ComponentModel.Container components = null;

		public frmRCleint()
		{
			//************************************* TCP *************************************//
			// using TCP protocol
			// running both client and server on same machines
			TcpChannel chan = new TcpChannel();
			ChannelServices.RegisterChannel(chan, false);
			// Create an instance of the remote object
			remoteObject = (MyRemotableObject) Activator.GetObject(typeof(MyRemotableObject),"tcp://localhost:8080/HelloWorld");
			// if remote object is on another machine the name of the machine should be used instead of localhost.
			//************************************* TCP *************************************//
		}

        public void setMessage(string text)
        {
            remoteObject.SetMessage(text);
        }
	}
}
