/////////////////////////////////////////////////////////////////////////////
//  NavigatorServer.cs ver 1.0                                             //
//  Language:     C#, VS 2017                                              //
//  Platform:     AlienWare 17R5, WIN10, Visual Studio 2017                //
//  Application:  CSE681 Project4                                          //
//  Author:       Xiao Chen, Computer Engineering Syracuse University      //
//                (315) 560-7375, xchen149@syr.edu                         //
//  Source: NavigatorServer.cs created by Dr.Jim Fawcett                   //
/////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines a single NavigatorServer class that returns file
 * and directory information about its rootDirectory subtree.  It uses
 * a message dispatcher that handles processing of all incoming and outgoing
 * messages.
 * 
 * 
 * public interface:
 * NavigatorServer()
 * 
 * Maintanence History:
 * --------------------
 * ver 2.0 - 24 Oct 2017
 * - added message dispatcher which works very well - see below
 * - added these comments
 * ver 1.0 - 22 Oct 2017
 * - first release
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using FileUtilities;
using TypeTables;
using Navigator;
using DepAnalysis;
using StrongConpo;

namespace Navigator
{
    public class NavigatorServer
    {
        IFileMgr localFileMgr { get; set; } = null;
        Comm comm { get; set; } = null;
        private List<string> selected { get; set; } = new List<string>();
        private List<string> type_tab { get; set; } = new List<string>();
        Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher =
          new Dictionary<string, Func<CommMessage, CommMessage>>();

        /*----< initialize server processing >-------------------------*/

        public NavigatorServer()
        {
            initializeEnvironment();
            Console.Title = "Navigator Server";
            localFileMgr = FileMgrFactory.create(FileMgrType.Local);
        }
        /*----< set Environment properties needed by server >----------*/

        void initializeEnvironment()
        {
            Environment.root = ServerEnvironment.root;
            Environment.address = ServerEnvironment.address;
            Environment.port = ServerEnvironment.port;
            Environment.endPoint = ServerEnvironment.endPoint;
        }
        /*----< define how each message will be processed >------------*/

        void initializeDispatcher()
        {
            Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopFiles"] = getTopFiles;

            Func<CommMessage, CommMessage> getTopDirs = (CommMessage msg) =>
            {

                if (localFileMgr.pathStack.Count != 1)
                {
                    localFileMgr.pathStack.Clear();
                }

                Console.WriteLine(localFileMgr.currentPath);
                localFileMgr.currentPath = "";
                localFileMgr.pathStack.Push(localFileMgr.currentPath);
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopDirs"] = getTopDirs;

            Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
            {

                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;

            Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                localFileMgr.pathStack.Push(localFileMgr.currentPath);
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;

            Func<CommMessage, CommMessage> moveUpDirs = (CommMessage msg) =>
            {
                if (localFileMgr.pathStack.Count != 1)
                {
                    localFileMgr.pathStack.Pop();
                    localFileMgr.currentPath = localFileMgr.pathStack.Peek();
                }
                else
                {
                    localFileMgr.pathStack.Pop();
                    localFileMgr.currentPath = Environment.root;
                    localFileMgr.pathStack.Push(localFileMgr.currentPath);
                    localFileMgr.currentPath = localFileMgr.pathStack.Peek();
                }
                Console.WriteLine(localFileMgr.currentPath);
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveUpDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                //Console.WriteLine(localFileMgr.pathStack.Count);
                return reply;
            };
            messageDispatcher["moveUpDirs"] = moveUpDirs;

            Func<CommMessage, CommMessage> moveUpDirsFile = (CommMessage msg) =>
            {

                if (localFileMgr.pathStack.Count != 0)
                {
                    localFileMgr.currentPath = localFileMgr.pathStack.Peek();
                }
                else
                {
                    localFileMgr.currentPath = Environment.root;
                }
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveUpDirsFile";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                Console.WriteLine(localFileMgr.pathStack.Count);
                return reply;
            };
            messageDispatcher["moveUpDirsFile"] = moveUpDirsFile;

            Func<CommMessage, CommMessage> moveSelectedFiles = (CommMessage msg) =>
            {
                int temp = 0;
                foreach (var item in selected)
                {
                    if (item == msg.arguments[0])
                    {
                        temp = 1;
                    }
                }
                if (temp == 0 && Path.GetExtension(msg.arguments[0]) == ".cs")
                {
                    selected.Add(msg.arguments[0]);
                }

                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveSelectedFiles";
                foreach (var i in selected)
                {
                    reply.arguments.Add(i);
                }
                return reply;
            };
            messageDispatcher["moveSelectedFiles"] = moveSelectedFiles;

            Func<CommMessage, CommMessage> Add_all = (CommMessage msg) =>
            {
                foreach (var item in msg.arguments)
                {
                    int temp = 0;
                    foreach (var elem in selected)
                    {
                        if (elem == item)
                        {
                            temp = 1;
                        }
                    }
                    if (temp == 0 && Path.GetExtension(item) == ".cs")
                    {
                        selected.Add(item);
                    }
                }

                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "Add_all";
                foreach (var i in selected)
                {
                    reply.arguments.Add(i);
                }
                // reply.arguments = selected;
                return reply;
            };
            messageDispatcher["Add_all"] = Add_all;

            Func<CommMessage, CommMessage> removeSelectedFiles = (CommMessage msg) =>
            {
                for (var i = 0; i < selected.Count; i++)
                {
                    Console.WriteLine(selected[i]);
                    if (selected[i] == msg.arguments[0])
                    {
                        selected.Remove(selected[i]);
                    }
                }
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "removeSelectedFiles";
                foreach (var i in selected)
                {
                    reply.arguments.Add(i);
                }
                // reply.arguments = selected;
                return reply;
            };
            messageDispatcher["removeSelectedFiles"] = removeSelectedFiles;

            Func<CommMessage, CommMessage> clearSelectedFiles = (CommMessage msg) =>
            {
                selected.Clear();
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "clearSelectedFiles";

                reply.arguments = selected;
                return reply;
            };
            messageDispatcher["clearSelectedFiles"] = clearSelectedFiles;

            Func<CommMessage, CommMessage> Generate_TT = (CommMessage msg) =>{
                List<string> file_list = new List<string>();
                string ttt = Path.GetFullPath(ServerEnvironment.root);
                for (var i = 0; i < msg.arguments.Count; i++)
                {
                    file_list.Add(ttt + msg.arguments[i]);
                }
                TypeTable tt = new TypeTable();
                tt = tt.GetTable(file_list.ToArray());
                for (var i = 0; i < file_list.Count; i++)
                {
                    string temp = Path.GetFileNameWithoutExtension(file_list[i]);

                    file_list[i] = temp;
                    Console.WriteLine(file_list[i]);
                }
                //List<string> type_table = tt.print();
                string type_table = tt.tt_to_string();
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "Generate_TT";

                List<string> tt_t = new List<string>();
                tt_t.Add("");
                if (type_table.Length > 1024)
                {
                    string pp = "";
                    for (int i = 0; i < type_table.Length; i++){
                        pp += type_table[i];
                        if (pp.Length >= 1024){
                            reply.arguments.Add(pp);
                            pp = "";
                        }
                    }
                    reply.arguments.Add(pp);
                    pp = "";
                }
                else{
                    reply.arguments.Add(type_table);
                }
                return reply;
            };
            messageDispatcher["Generate_TT"] = Generate_TT;

            Func<CommMessage, CommMessage> Generate_DT = (CommMessage msg) =>{
                List<string> file_list = new List<string>();
                string ttt = Path.GetFullPath(ServerEnvironment.root);
                for (var i = 0; i < msg.arguments.Count; i++){
                    file_list.Add(ttt + msg.arguments[i]);
                }
                DepenAnalysis dp = new DepenAnalysis();
                dp = dp.DepenAnalysises(file_list.ToArray());
                for (var i = 0; i < file_list.Count; i++)
                {
                    string temp = Path.GetFileNameWithoutExtension(file_list[i]);

                    file_list[i] = temp;
                    Console.WriteLine(file_list[i]);
                }

                //List<string> dep_table = dp.asign_table();
                string dep_table = dp.dep_tostring();
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "Generate_DT";
                if (dep_table.Length > 1024)
                {
                    string pp = "";
                    for (int i = 0; i < dep_table.Length; i++)
                    {
                        pp += dep_table[i];
                        if (pp.Length >= 1024)
                        {
                            reply.arguments.Add(pp);
                            pp = "";
                        }

                    }
                    reply.arguments.Add(pp);
                    pp = "";
                }
                else
                {
                    reply.arguments.Add(dep_table);
                }
                return reply;
            };
            messageDispatcher["Generate_DT"] = Generate_DT;

            Func<CommMessage, CommMessage> Generate_SC = (CommMessage msg) =>
            {
                List<string> file_list = new List<string>();
                string ttt = Path.GetFullPath(ServerEnvironment.root);
                for (var i = 0; i < msg.arguments.Count; i++)
                {
                    file_list.Add(ttt + msg.arguments[i]);
                }

                CsGraph<string, string> csGraph = new CsGraph<string, string>("Dep_Table");
                csGraph = csGraph.Creat_Graph(file_list.ToArray());
                csGraph.sc_finder();
                for (var i = 0; i < file_list.Count; i++)
                {
                    string temp = Path.GetFileNameWithoutExtension(file_list[i]);

                    file_list[i] = temp;
                    Console.WriteLine(file_list[i]);
                }
                string SC = csGraph.SC_tostring();
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "Generate_SC";
                if (SC.Length > 1024)
                {
                    string pp = "";
                    for (int i = 0; i < SC.Length; i++)
                    {
                        pp += SC[i];
                        if (pp.Length >= 1024)
                        {
                            reply.arguments.Add(pp);
                            pp = "";
                        }

                    }
                    reply.arguments.Add(pp);
                    pp = "";
                }
                else
                {
                    reply.arguments.Add(SC);
                }
                return reply;
            };
            messageDispatcher["Generate_SC"] = Generate_SC;

            Func<CommMessage, CommMessage> importone = (CommMessage msg) => {
                string path = Path.GetFullPath(ServerEnvironment.root);
                List<string> dirs_list = new List<string>();
                string ttt = Path.GetFullPath(ServerEnvironment.root) + msg.arguments[0];
                Console.WriteLine(msg.arguments[0] + "debuggogogo");
                string judge = msg.arguments[0] + "debuggogogo";
                List<string> file_list = new List<string>();
                file_list.AddRange(Directory.GetFiles(ttt).ToList<string>());
                dirs_list.AddRange(Directory.GetDirectories(ttt).ToList<string>());
                int temp1 = path.Length;
                foreach (var i in dirs_list)
                {
                    file_list.AddRange(Directory.GetFiles(i).ToList<string>());
                }
                for (var i = 0; i < file_list.Count; i++)
                {
                    if (judge == "debuggogogo")
                    {
                        file_list[i] = file_list[i].Remove(temp1);
                        Console.WriteLine(file_list[i]);
                        file_list[i] = Path.GetFileName(file_list[i]);
                    }
                    else
                    {
                        file_list[i] = file_list[i].Remove(0, temp1);
                    }

                }
                if (msg.arguments.Count == 0)
                {
                    selected = selected;
                }
                else
                {
                    foreach (var item in file_list)
                    {
                        int temp = 0;
                        foreach (var elem in selected)
                        {
                            if (elem == item)
                            {
                                temp = 1;
                            }
                        }
                        if (temp == 0 && Path.GetExtension(item) == ".cs")
                        {
                            selected.Add(item);
                        }
                    }
                }
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "importone";
                foreach (var i in selected)
                {
                    reply.arguments.Add(i);
                }
                return reply;
            };
            messageDispatcher["importone"] = importone;

            Func<CommMessage, CommMessage> importall = (CommMessage msg) =>
            {
                localFileMgr.all_files.Clear();
                if (localFileMgr.pathStack.Count == 1)
                {
                    localFileMgr.currentPath = ServerEnvironment.root;
                }
                else
                {

                    localFileMgr.currentPath = ServerEnvironment.root + localFileMgr.pathStack.Peek();
                }
                Console.WriteLine(localFileMgr.currentPath);

                string temp = Path.GetFullPath(ServerEnvironment.root);
                int temp2 = temp.Length;


                string dirPath = Path.GetFullPath(localFileMgr.currentPath);
                List<string> file_sets = new List<string>();



                localFileMgr.FindFile(dirPath);
                file_sets = localFileMgr.all_files;
                for (var i = 0; i < file_sets.Count; i++)
                {
                    file_sets[i] = file_sets[i].Remove(0, temp2);
                }
                selected = file_sets;



                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "importall";
                reply.arguments = file_sets;
                return reply;



            };
            messageDispatcher["importall"] = importall;
        }

        /*----< Server processing >------------------------------------*/
        /*
         * - all server processing is implemented with the simple loop, below,
         *   and the message dispatcher lambdas defined above.
         */
        static void Main(string[] args)
        {
            //TestUtilities.title("Starting Navigation Server", '=');
            try
            {
                NavigatorServer server = new NavigatorServer();
                server.initializeDispatcher();
                server.comm = new MessagePassingComm.Comm(ServerEnvironment.address, ServerEnvironment.port);

                while (true)
                {
                    CommMessage msg = server.comm.getMessage();
                    if (msg.type == CommMessage.MessageType.closeReceiver)
                        break;
                    msg.show();
                    if (msg.command == null)
                        continue;
                    CommMessage reply = server.messageDispatcher[msg.command](msg);
                    reply.show();
                    server.comm.postMessage(reply);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:\n{0}\n\n", ex.Message);
            }
        }
    }
}
