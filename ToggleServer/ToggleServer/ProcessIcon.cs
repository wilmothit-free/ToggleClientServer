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
using ToggleServer.Properties;
using System.Net.Sockets;
using RedCorona.Net;

namespace ToggleServer
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
        System.Timers.Timer updateClientsTimer;
        System.Timers.Timer countDownTimer;
        Server server;
        string status = "true";

        XmlDocument configFile;
        XmlNode node;
        int server_port = 7404;
        int reset = 420000;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessIcon"/> class.
        /// </summary>
        public ProcessIcon()
        {
            // Instantiate the NotifyIcon object.
            ni = new NotifyIcon();

            try
            {
                configFile = new XmlDocument();
                configFile.Load("config.xml");
                node = configFile.DocumentElement.SelectSingleNode("/server/port");
                server_port = Int32.Parse(node.InnerText);
                node = configFile.DocumentElement.SelectSingleNode("/server/reset");
                reset = Int32.Parse(node.InnerText);
            }
            catch (FileNotFoundException fnfe)
            {
                ni.Icon = Resources.DisconnectedSml;
                ni.Text = "Cannot locate config.xml file. Using default values.";
                Console.WriteLine("Error! Cannot find config file. Using default values.");                
            }
            catch (NullReferenceException nre)
            {
                ni.Icon = Resources.DisconnectedSml;
                ni.Text = "Invalid config.xml file. Using default values.";
                Console.WriteLine("Error! Invalid config.xml file. Using default values.");                
            }
            catch (XmlException xe)
            {
                ni.Icon = Resources.DisconnectedSml;
                ni.Text = "Invalid config.xml file. Using default values.";
                Console.WriteLine("Error! Invalid config.xml file. Using default values.");
            }
        }

        /// <summary>
        /// Displays the icon in the system tray.
        /// </summary>
        public void Display()
        {
            // Put the icon in the system tray and allow it react to mouse clicks.			
            ni.MouseClick += new MouseEventHandler(ni_MouseClick);
            ni.Icon = Resources.GoSml;
            ni.Text = "Server: Available.";
            ni.Visible = true;

            // Attach a context menu.
            ni.ContextMenuStrip = new ContextMenus().Create();

            //Start the server
            server = new Server(2345, new ClientEvent(ClientConnect));

            //Start the timer that updates clients
            updateClientsTimer = new System.Timers.Timer(1000);
            updateClientsTimer.Elapsed += UpdateClients;
            updateClientsTimer.Start();

            //Prepare the timer that resets after x minutes
            countDownTimer = new System.Timers.Timer(1000*60*7);
            countDownTimer.Elapsed += new ElapsedEventHandler(ChangeStatus);            
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
            // Handle mouse button clicks.
            if (e.Button == MouseButtons.Left)
            {
                //On the server we don't do anything yet when a user clicks it, but we could do something here.

                // Start Windows Explorer.
                //Process.Start("explorer", null);
                //string text = "Toggle";
                //server.Broadcast(Encoding.UTF8.GetBytes(text));
            }

        }

        bool ClientConnect(Server serv, ClientInfo new_client)
        {
            new_client.Delimiter = "\n";
            new_client.OnRead += new ConnectionRead(ChangeStatus);
            new_client.OnClose += new ConnectionClosed(DisconnectClient);
            Console.WriteLine("New client connection: " + new_client.ID);            
            return true; // allow this connection
        }

        void UpdateClients(object sender, ElapsedEventArgs e)
        {
            server.Broadcast(Encoding.UTF8.GetBytes(status));
        }

        private void ChangeStatus(object sender, ElapsedEventArgs e)
        {
            ChangeStatus(null, null);
        }

        void ChangeStatus(ClientInfo ci, String text)
        {
            if (status.CompareTo("true") == 0)
            {
                status = "false";
                ni.Icon = Resources.NoEntrySml;
                ni.Text = "Server: Unavailable.";
                countDownTimer.Start();
            }
            else
            {
                status = "true";
                ni.Icon = Resources.GoSml;
                ni.Text = "Server: Available.";
                countDownTimer.Stop();                
            }
        }

        void DisconnectClient(ClientInfo ci)
        {
            Console.WriteLine("Client disconnected: " + ci.ID);
        }
    }
}