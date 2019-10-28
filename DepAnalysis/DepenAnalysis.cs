/////////////////////////////////////////////////////////////////////////////
//  DepenAnalysis.cs -- find dependency between two files and return a     //
//  dependency table                                                       //
//  ver 1.0                                                                //
//  Language:     C#, VS 2017                                              //
//  Platform:     AlienWare 17R5, WIN10, Visual Studio 2017                //
//  Application:  CSE681 Project3                                          //
//  Author:       Xiao Chen, Computer Engineering Syracuse University      //
//                (315) 560-7375, xchen149@syr.edu                         //
//  Source: Parser.cs created by Dr.Jim Fawcett                            //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module defines the following class:
 *   DepenAnalysis  - creat a container for dependency analysis and get 
 *   dependency table by conparing typetable with semiexpression from 
 *   target files
 *   
 *   Public Interface
 * ======================
 * showFormal()            // show dependency table
 * show()           // show dependency graph
 * dep_tostring()   // transform dep table into string
 *   decrease() // decrease function complexity
 *   getttt() // decrease function complexity
 */
/*
 *   Build Process
 *   -------------
 *   - Required files: semi.cs  Typetable.cs ITokenCollection.cs  FileMgr.cs
 *     
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 31 October 2018
 *     - first release
 *     
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeTables;
using FileUtilities;
using Lexer;

namespace DepAnalysis
{
    /////////////////////////////////////////////////////////////////////////
    // Dependency Class was made to generate a dependency table and implement 
    // a function to show result of dependency analysis

    public class DepenAnalysis
    {
        public Dictionary<String, List<String>> DepTable { get; set; } = new Dictionary<String, List<String>>();

        //------<add was used to add file into container>------

        public void add(String file1, String file2)
        {
            if (DepTable.ContainsKey(file1))
            {
                DepTable[file1].Add(file2);
            }
            else
            {
                List<String> temp = new List<String>();
                temp.Add(file2);
                DepTable.Add(file1, temp);
            }
        }
        //------<show was used to show dependency graph>------

        public void show()
        {
            Console.Write("\n");
            Console.WriteLine("This is Graph of Dependency Analysis");
            Console.WriteLine("====================================");
            foreach (var item in DepTable)
            {
                Console.Write("\n  {0}", item.Key);
                foreach (var elem in item.Value)
                {
                    Console.Write("\n    {0}", elem);
                }
            }
            Console.Write("\n");
        }
        //------<showFormal was used to show dependency relationship>------

        public void showFormal()
        {
            foreach (var item in DepTable)
            {
                Console.Write(" \n");
                Console.Write("\n  {0}", item.Key);
                Console.Write(" Dependency:");
                Console.Write(" \n ============================= \n");
                foreach (var elem in item.Value)
                {

                    Console.Write("\n  {0}", item.Key);
                    Console.Write("   depends on   ");
                    Console.Write("{0}", elem);
                }
            }
            Console.Write("\n");
        }

        public string dep_tostring()
        {
            string dep_table = "Dependency Analysis";
            foreach(var item in DepTable)
            {
                dep_table = dep_table + " \n ====================";
                dep_table = dep_table + " \n"+item.Key;
                dep_table = dep_table + " Dependence: ";
                dep_table = dep_table + "\n ";
                foreach(var elem in item.Value)
                {
                    dep_table = dep_table + "\n" + item.Key + " depends on " + elem;
                }
            }
            return dep_table;
        }

        public List<string> asign_table()
        {
            List<string> print = new List<string>();
            print.Add(" Dependence Analysis: \n");
            foreach (var ele in DepTable)
            {
                print.Add(" ============================= ");
                print.Add(" File Name: ");
                print.Add(ele.Key);
                print.Add(" Dependency: ");
                foreach (var item in ele.Value)
                {
                    print.Add(ele.Key);
                    print.Add("   depends on   ");
                    print.Add(item);
                }
                print.Add(" ============================= ");
            }
            return print;
        }
        public void decrease(List<TypeItem> i, TypeTable tt, ITokenCollection semi, DepenAnalysis dp, string file)
        {
            foreach (var item in i)
            {
                if (item.file != System.IO.Path.GetFileName(file))
                {
                    if (!dp.DepTable.Keys.Contains(System.IO.Path.GetFileName(file)))
                    {
                        dp.add(System.IO.Path.GetFileName(file), item.file);
                    }
                    else
                    {
                        if (dp.DepTable[System.IO.Path.GetFileName(file)].Contains(item.file)) break;
                        else dp.DepTable[System.IO.Path.GetFileName(file)].Add(item.file);
                    }
                }
            }
        }

        public void getttt(TypeTable tt, ITokenCollection semi, DepenAnalysis dp, string file)
        {
            foreach (var elem in tt.table)
            {
                if (semi.contains(elem.Key))
                {
                    if (semi.contains("class") && semi.contains(":"))
                    {
                        decrease(elem.Value, tt, semi, dp, file);
                    }
                    if (!semi.contains("enum") && !semi.contains("class") && !semi.contains("struct") && !semi.contains("interface") && !semi.contains("delegate"))
                    {
                        decrease(elem.Value, tt, semi, dp, file);
                    }
                    else continue;
                }
                else continue;
            }
        }

        //------<DepenAnalysises was used to compare semi with what inside typetable to get dependency relations>------

        public DepenAnalysis DepenAnalysises(string[] args){
            TypeTable tt = new TypeTable();
            tt = tt.GetTable(args);
            DepenAnalysis dp = new DepenAnalysis();
            foreach (string file in args){
                ITokenCollection semi = Factory.create();
                if (!semi.open(file)){
                    Console.Write("\n  Can't open {0}\n\n", args[0]); break;
                }
                while (!semi.isDone()){
                    semi.get();
                    getttt(tt,semi,dp,file);
                }
                semi.close();
            }
            return dp;
        }
    }

    public class testdep { 
#if (TEST_DepTable)

        static void Main(string[] args)
        {
            FileFinder filef = new FileFinder();
            args = filef.get_cs();
            DepenAnalysis dp = new DepenAnalysis();
            dp = dp.DepenAnalysises(args);
            dp.show();
            dp.showFormal();
            Console.Write("\n\n");
            Console.ReadKey();
        }
#endif
    }

}