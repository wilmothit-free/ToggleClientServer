/**
        MIT License

		Copyright (c) 2017 James Wilmoth

		Permission is hereby granted, free of charge, to any person obtaining a copy
		of this software and associated documentation files (the "Software"), to deal
		in the Software without restriction, including without limitation the rights
		to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
		copies of the Software, and to permit persons to whom the Software is
		furnished to do so, subject to the following conditions:

		The above copyright notice and this permission notice shall be included in all
		copies or substantial portions of the Software.

		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
		IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
		FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
		AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
		LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
		OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
		SOFTWARE.
*/
using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Timers;
using System.Windows.Forms;
using ToggleClient.Properties;
using System.Net.Sockets;
using RedCorona.Net;

namespace ToggleClient
{
    /// <summary>
    /// 
    /// </summary>
    class ProcessIcon : IDisposable
    {
        /// <summary>
        /// The NotifyIcon object.
        /// </summary>
        NotifyIcon ni;
        System.Timers.Timer timer;
        ClientInfo client;

        XmlDocument configFile;
        XmlNode node;

        string server_name;                
        int server_port = 2345;

        bool connected = false;        

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIcon"/> class.
        /// </summary>
        public ProcessIcon()
        {
            // Instantiate the NotifyIcon object.
            ni = new NotifyIcon();
            //timer = new System.Timers.Timer(1000);
            //timer.Elapsed += Start;
        }

        /// <summary>
        /// Displays the icon in the system tray.
        /// </summary>
        public void Display()
        {
            // Put the icon in the system tray and allow it react to mouse clicks.			
            ni.MouseClick += new MouseEventHandler(ni_MouseClick);
            ni.Icon = Resources.DisconnectedSml;
            ni.Text = "Offline. Connecting...";
            ni.Visible = true;
            // Attach a context menu.
            ni.ContextMenuStrip = new ContextMenus().Create();

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += CheckConnection;
            timer.Start();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            // When the application closes, this will remove the icon from the system tray immediately.
            ni.Dispose();
        }

        /// <summary>
        /// Handles the MouseClick event of the ni control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        void ni_MouseClick(object sender, MouseEventArgs e)
        {
            if (connected)
            {
                // Handle mouse button clicks.
                if (e.Button == MouseButtons.Left)
                {
                    client.Send("toggle\n");
                }
            }
        }

        private void ConnectToServer()
        {
            try
            {
                configFile = new XmlDocument();
                configFile.Load("config.xml");
                node = configFile.DocumentElement.SelectSingleNode("/server/name");
                server_name = node.InnerText;
                node = configFile.DocumentElement.SelectSingleNode("/server/port");
                server_port = Int32.Parse(node.InnerText);
            }
            catch (FileNotFoundException fnfe)
            {
                ni.Icon = Resources.DisconnectedSml;
                ni.Text = "Cannot locate config.xml file. Not sure who is server...";
                Console.WriteLine("Error! Cannot find config file.");
                return;
            }
            catch(NullReferenceException nre)
            {
                ni.Icon = Resources.DisconnectedSml;
                ni.Text = "Invalid config.xml file. <server><name>server</name></server>";
                Console.WriteLine("Error! Invalid config.xml file. Needs contents: <server><name>server</name></server>");
                return;
            }

            Socket sock = null;
            try
            {
                sock = Sockets.CreateTCPSocket(server_name, server_port);
                client = new ClientInfo(sock, false); // Don't start receiving yet
                client.OnReadBytes += new ConnectionReadBytes(UpdateStatus);
                client.BeginReceive();
                client.OnClose += DisconnectedFromServer;
                connected = true;
                timer.Stop();            
            }
            catch
            {
                ni.Icon = Resources.DisconnectedSml;
                ni.Text = "Offline. Connecting...";
                Console.WriteLine("Error! Cannot reach server " + server_name + ".");                
            }
        }

        private void DisconnectedFromServer(ClientInfo ci)
        {
            connected = false;
            ni.Icon = Resources.DisconnectedSml;
            ni.Text = "Offline. Connecting...";
            timer.Start();
        }
        
        void CheckConnection(object sender, ElapsedEventArgs e)
        {
            ConnectToServer();
        }

        void UpdateStatus(ClientInfo ci, byte[] data, int len)
        {
            Console.WriteLine("Received update from server.");
            string status = Encoding.UTF8.GetString(data, 0, len);
            if (status.CompareTo("true") == 0)
            {
                ni.Icon = Resources.GoSml;
                ni.Text = "Available. Click to reserve.";
            }
            else
            {
                ni.Icon = Resources.NoEntrySml;
                ni.Text = "Unavailable. Click to reset or allow auto-expire.";
            }
        }
    }
}