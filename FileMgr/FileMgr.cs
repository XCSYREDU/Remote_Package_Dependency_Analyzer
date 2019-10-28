/////////////////////////////////////////////////////////////////////////////
//  FileMgr.cs -- Package used to find path of collections of files        //
//  ver 1.0                                                                //
//  Language:     C#, VS 2017                                              //
//  Platform:     AlienWare 17R5, WIN10, Visual Studio 2017                //
//  Application:  CSE681 Project3                                          //
//  Author:       Xiao Chen, Computer Engineering Syracuse University      //
//                (315) 560-7375, xchen149@syr.edu                         //
//  Source: FileMgr.cs created by Dr.Jim Fawcett                           //
/////////////////////////////////////////////////////////////////////////////
/*
 *  Module Operations:
 *  ==================
 *  Recursively displays the contents of a directory tree
 *  rooted at a specified path, with specified file pattern.
 *
 *  This version uses delegates to avoid embedding application
 *  details in the Navigator.  Navigate now is reusable.  Clients
 *  simply register event handlers for Navigate events newDir 
 *  and newFile.
 *  
 *  Class FileFinder provide a function of get_cs to get .cs file from 
 *  every folder from the solution .
 * 
 *  Public Interface:
 *  =================
 *  LocalFileMgr();
 *  LocalFileMgr getFiles();
 *  LocalFileMgr getDirs();
 *  LocalFileMgr setDir(string dir)
 *  
 *  FindFile(string dirPath) 
 * 
 *  Maintenance History:
 *  ====================
 *  
 *  ver 1.0 : 1 Novenber 2018
 *  - first release
 */
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Navigator;
using Environment = Navigator.Environment;

namespace FileUtilities
{
    ///////////////////////////////////////////////////////////////////
    // Navigate class
    // - uses public event properties to avoid binding directly
    //   to application processing

    public enum FileMgrType { Local, Remote }

    ///////////////////////////////////////////////////////////////////
    // NavigatorClient uses only this interface and factory

    public interface IFileMgr
    {
        IEnumerable<string> getFiles();
        IEnumerable<string> getDirs();
        bool setDir(string dir);
        Stack<string> pathStack { get; set; }
        string currentPath { get; set; }
        List<string> all_files { get; set; }
        void FindFile(string dirPath);
    }

    public class FileMgrFactory
    {
        static public IFileMgr create(FileMgrType type)
        {
            if (type == FileMgrType.Local)
                return new LocalFileMgr();
            else
                return null;  // eventually will have remote file Mgr
        }
    }

    ///////////////////////////////////////////////////////////////////
    // Concrete class for managing local files

    public class LocalFileMgr : IFileMgr
    {
        public string currentPath { get; set; } = "";
        public Stack<string> pathStack { get; set; } = new Stack<string>();
        public List<string> all_files { get; set; } = new List<string>();
        public LocalFileMgr()
        {
            pathStack.Push(currentPath);  // stack is used to move to parent directory
        }
        //----< get names of all files in current directory >------------

        public IEnumerable<string> getFiles()
        {
            List<string> files = new List<string>();
            string path = Path.Combine(Environment.root, currentPath);
            string absPath = Path.GetFullPath(path);
            files = Directory.GetFiles(path).ToList<string>();
            
            for (int i = 0; i < files.Count(); ++i)
            {
                files[i] = Path.Combine(currentPath, Path.GetFileName(files[i]));
            }
            return files;
        }
        //----< get names of all subdirectories in current directory >---

        public IEnumerable<string> getDirs()
        {
            List<string> dirs = new List<string>();
            string path = Path.Combine(Environment.root, currentPath);
            dirs = Directory.GetDirectories(path).ToList<string>();
            
            for (int i = 0; i < dirs.Count(); ++i)
            {
                string dirName = new DirectoryInfo(dirs[i]).Name;
                dirs[i] = Path.Combine(currentPath, dirName);
            }
            return dirs;
        }
        //----< sets value of current directory - not used >-------------

        public bool setDir(string dir)
        {
            if (!Directory.Exists(dir))
                return false;
            currentPath = dir;
            return true;
        }

        public void FindFile(string dirPath) 
        {
            DirectoryInfo Dir = new DirectoryInfo(dirPath);
            try
            {
                foreach (DirectoryInfo d in Dir.GetDirectories())
                {
                    if (d.GetFiles() != null)
                    {
                        FindFile(Path.Combine(Dir.ToString(), d.ToString()));
                    }
                }
                foreach (FileInfo f in Dir.GetFiles("*.cs"))
                {
                    all_files.Add(Path.Combine(Dir.ToString(), f.ToString())); 
                }
 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }

    class TestFileMgr
    {
        static void Main(string[] args)
        {

            IFileMgr test = FileMgrFactory.create(FileMgrType.Local);
            test.getFiles();
            
        }
    }

    ///////////////////////////////////////////////////////////////////
    // FileFinder class
    // - implement a function to get .cs file from folder

    public class FileFinder
    {
        ///////////////////////////////////////////////////////////////////
        // This get_cs function was implemented to get .cs file in folder 
        // of solution. 

        public string[] get_cs()
        {
            List<string> temp_file_container = new List<string>(); // temp file container used to store gernal path of file
            List<string> cs_file_container = new List<string>(); // container to collect final .cs file

            string paths = Directory.GetCurrentDirectory();
            paths = Path.GetFullPath(paths + "../../../../");
            string[] sub_dic = Directory.GetDirectories(paths);
            string[] root_path = Directory.GetFileSystemEntries(paths);

            foreach (var member in root_path)
            {
                if (member.Contains(".cs"))
                {
                    temp_file_container.Add(member);
                }
            }

            foreach (var item in sub_dic)
            {
                string[] folder_path = Directory.GetFileSystemEntries(item);
                foreach (var elem in folder_path)
                {
                    if (elem.Contains(".cs"))
                    {
                        temp_file_container.Add(elem);
                    }
                }
            }

            foreach (var item_1 in temp_file_container)
            {
                string extention = Path.GetExtension(item_1);
                if (extention == ".cs")
                {
                    cs_file_container.Add(item_1);
                }
            }

            string[] final = cs_file_container.ToArray();
            return final;
        }




       
    }


}
