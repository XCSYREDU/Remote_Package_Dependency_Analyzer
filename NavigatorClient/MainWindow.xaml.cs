/////////////////////////////////////////////////////////////////////////////
//  MainWindow.xaml.cs                                                     //
//  ver 1.0                                                                //
//  Language:     C#, VS 2017                                              //
//  Platform:     AlienWare 17R5, WIN10, Visual Studio 2017                //
//  Application:  CSE681 Project4                                          //
//  Author:       Xiao Chen, Computer Engineering Syracuse University      //
//                (315) 560-7375, xchen149@syr.edu                         //
//  Source: MainWindow.xaml.cs created by Dr.Jim Fawcett                   //
/////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client.  The client
 * displays a local FileFolder view, and a remote FileFolder view.  It supports
 * navigating into subdirectories, both locally and in the remote Server.
 * 
 * It also supports viewing local files.
 * 
 * public interface:
 * Mainwindow()
 * 
 * Maintenance History:
 * --------------------
 * ver 2.1 : 26 Oct 2017
 * - relatively minor modifications to the Comm channel used to send messages
 *   between NavigatorClient and NavigatorServer
 * ver 2.0 : 24 Oct 2017
 * - added remote processing - Up functionality not yet implemented
 *   - defined NavigatorServer
 *   - added the CsCommMessagePassing prototype
 * ver 1.0 : 22 Oct 2017
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using MessagePassingComm;
using FileUtilities;
using Path = System.IO.Path;

namespace Navigator
{
    public partial class MainWindow : Window
    {
        
        private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
        Comm comm { get; set; } = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;

        public MainWindow()
        {
            InitializeComponent();
            initializeEnvironment();
            Console.Title = "file_browser Client";
            fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
            getTopFiles();
            comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            CommMessage try_con = new CommMessage(CommMessage.MessageType.connect);
            try_con.from = ClientEnvironment.endPoint;
            try_con.to = ServerEnvironment.endPoint;
            comm.postMessage(try_con);
        }
        //----< make Environment equivalent to ClientEnvironment >-------

        void initializeEnvironment()
        {
            Environment.root = ClientEnvironment.root;
            Environment.address = ClientEnvironment.address;
            Environment.port = ClientEnvironment.port;
            Environment.endPoint = ClientEnvironment.endPoint;
        }

        //----< define how to process each message command >-------------

        void initializeMessageDispatcher()
        {
            // load remoteFiles listbox with files from root

            messageDispatcher["getTopFiles"] = (CommMessage msg) =>
            {
                remoteFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    remoteFiles.Items.Add(file);
                }
            };
            // load remoteDirs listbox with dirs from root

            messageDispatcher["getTopDirs"] = (CommMessage msg) =>
            {
                remoteDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
            };
            // load remoteFiles listbox with files from folder

            messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
            {
                remoteFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    remoteFiles.Items.Add(file);
                }
            };
            // load remoteDirs listbox with dirs from folder

            messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
            {
                remoteDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
            };

            messageDispatcher["moveUpDirs"] = (CommMessage msg) =>
            {
                remoteDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
            };

            messageDispatcher["moveUpDirsFile"] = (CommMessage msg) =>
            {
                remoteFiles.Items.Clear();
                foreach (string files in msg.arguments)
                {
                    remoteFiles.Items.Add(files);
                }
            };

            messageDispatcher["moveSelectedFiles"] = (CommMessage msg) =>
            {
                File_sets.Items.Clear();
                foreach(var item in msg.arguments)
                {
                    File_sets.Items.Add(item);
                }
            };

            messageDispatcher["Add_all"] = (CommMessage msg) =>
            {
                File_sets.Items.Clear();
                foreach (var item in msg.arguments)
                {
                    File_sets.Items.Add(item);
                }
            };

            messageDispatcher["importone"] = (CommMessage msg) =>
            {
                File_sets.Items.Clear();
                foreach (var item in msg.arguments)
                {
                    File_sets.Items.Add(item);
                }
            };

            messageDispatcher["importall"] = (CommMessage msg) =>
            {
                File_sets.Items.Clear();
                foreach (var item in msg.arguments)
                {
                    File_sets.Items.Add(item);
                }
            };

            messageDispatcher["removeSelectedFiles"] = (CommMessage msg) =>
            {
                File_sets.Items.Clear();
                foreach (var item in msg.arguments)
                {
                    File_sets.Items.Add(item);
                }
            };

            messageDispatcher["clearSelectedFiles"] = (CommMessage msg) =>
            {
                File_sets.Items.Clear();
            };

            messageDispatcher["Generate_TT"] = (CommMessage msg) =>
            {
                //Result1.Items.Clear();
                Result1.Clear();
                foreach (var item in msg.arguments)
                {
                    Result1.AppendText(item);
                    //Result1.Items.Add(item);
                }
            };

            messageDispatcher["Generate_DT"] = (CommMessage msg) =>
            {
                //Result1.Items.Clear();
                Result1.Clear();
                foreach (var item in msg.arguments)
                {
                    Result1.AppendText(item);
                    //Result1.Items.Add(item);
                }
            };

            messageDispatcher["Generate_SC"] = (CommMessage msg) =>
            {
                //Result1.Items.Clear();
                Result1.Clear();
                foreach (var item in msg.arguments)
                {
                    Result1.AppendText(item);
                    //Result1.Items.Add(item);
                }
            };
        }
        //----< define processing for GUI's receive thread >-------------

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }
        //----< shut down comm when the main window closes >-------------

        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();

            // The step below should not be nessary, but I've apparently caused a closing event to 
            // hang by manually renaming packages instead of getting Visual Studio to rename them.

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        //----< not currently being used >-------------------------------

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        //----< show files and dirs in root path >-----------------------

        public void getTopFiles()
        {
            List<string> files = fileMgr.getFiles().ToList<string>();
            localFiles.Items.Clear();
            foreach (string file in files)
            {
                localFiles.Items.Add(file);
            }
            List<string> dirs = fileMgr.getDirs().ToList<string>();
            localDirs.Items.Clear();
            foreach (string dir in dirs)
            {
                localDirs.Items.Add(dir);
            }
        }
        //----< move to directory root and display files and subdirs >---

        private void localTop_Click(object sender, RoutedEventArgs e)
        {
            fileMgr.currentPath = "";
            getTopFiles();
        }
        //----< show selected file in code popup window >----------------

        private void localFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = localFiles.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.root, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        //----< move to parent directory and show files and subdirs >----

        private void localUp_Click(object sender, RoutedEventArgs e)
        {
            if (fileMgr.currentPath == "")
                return;
            Console.WriteLine(fileMgr.currentPath);
            fileMgr.currentPath = fileMgr.pathStack.Peek();
            fileMgr.pathStack.Pop();
            getTopFiles();
        }
        //----< move into subdir and show files and subdirs >------------

        private void localDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
            string dirName = localDirs.SelectedValue as string;
            
            fileMgr.pathStack.Push(fileMgr.currentPath);
            fileMgr.currentPath = dirName;
            getTopFiles();
        }
        //----< move to root of remote directories >---------------------
        /*
         * - sends a message to server to get files from root
         * - recv thread will create an Action<CommMessage> for the UI thread
         *   to invoke to load the remoteFiles listbox
         */
        private void RemoteTop_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Jim Fawcett";
            msg1.command = "getTopDirs"; 
            msg1.arguments.Add("");
            comm.postMessage(msg1);

            CommMessage msg2 = msg1.clone();
            msg2.command = "getTopFiles";
            comm.postMessage(msg2);
        }

        private void addAll_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "Add_all";
            for (var i = 0; i < remoteFiles.Items.Count; i++)
            {
                msg1.arguments.Add(remoteFiles.Items[i].ToString());
                Console.WriteLine(remoteFiles.Items[i]);
            }
            comm.postMessage(msg1);
        }

        //----< download file and display source in popup window >-------

        private void remoteFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = remoteFiles.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.root, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

        }
        //----< move to parent directory of current remote path >--------

        private void RemoteUp_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "moveUpDirs";
            /*
             for(var i = 0; i < remoteDirs.Items.Count; i++)
             {
                 msg1.arguments.Add(remoteDirs.Items[i].ToString());
             }
          */
            msg1.arguments.Add("");
            comm.postMessage(msg1);
            
            CommMessage msg2 = msg1.clone();
            msg2.command = "moveUpDirsFile";
            comm.postMessage(msg2);
        }
        //----< move into remote subdir and display files and subdirs >--
        /*
         * - sends messages to server to get files and dirs from folder
         * - recv thread will create Action<CommMessage>s for the UI thread
         *   to invoke to load the remoteFiles and remoteDirs listboxs
         */
        private void remoteDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "moveIntoFolderFiles";
            msg1.arguments.Add(remoteDirs.SelectedValue as string);
           // Console.WriteLine(msg1.arguments[0]);
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "moveIntoFolderDirs";
            comm.postMessage(msg2);
        }

        private void select_button(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "moveSelectedFiles";
            msg1.arguments.Add(remoteFiles.SelectedValue as string);
            // Console.WriteLine(msg1.arguments[0]);
            comm.postMessage(msg1);

        }

        private void remove_button(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "removeSelectedFiles";
            msg1.arguments.Add(File_sets.SelectedValue as string);
            // Console.WriteLine(msg1.arguments[0]);
            comm.postMessage(msg1);

        }

        private void clear_button(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "clearSelectedFiles";
            //msg1.arguments.Add();
            // Console.WriteLine(msg1.arguments[0]);
            comm.postMessage(msg1);

        }

        private void TypeTable_button(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "Generate_TT";
            for(var i = 0; i < File_sets.Items.Count; i++)
            {
                msg1.arguments.Add(File_sets.Items[i].ToString());
                Console.WriteLine(File_sets.Items[i]);
            }
            comm.postMessage(msg1);
        }

        private void Depen_button(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "Generate_DT";
            for (var i = 0; i < File_sets.Items.Count; i++)
            {
                msg1.arguments.Add(File_sets.Items[i].ToString());
                Console.WriteLine(File_sets.Items[i]);
            }
            comm.postMessage(msg1);
        }

        private void SC_button(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "Generate_SC";
            for (var i = 0; i < File_sets.Items.Count; i++)
            {
                msg1.arguments.Add(File_sets.Items[i].ToString());
                Console.WriteLine(File_sets.Items[i]);
            }
            comm.postMessage(msg1);
        }

        private void importone(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "importone";
            msg1.arguments.Add(remoteDirs.SelectedValue as string);
            comm.postMessage(msg1);
        }

        private void importAll(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "importall";
            for (var i = 0; i < remoteDirs.Items.Count; i++)
            {
                msg1.arguments.Add(remoteDirs.Items[i].ToString());
                Console.WriteLine(remoteDirs.Items[i]);
            }
            comm.postMessage(msg1);
        }

    }
}
