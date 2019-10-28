/////////////////////////////////////////////////////////////////////////////
//  TestUnits.cs --  Test Unit for dependency analysis                     //
//  ver 1.0                                                                //
//  Language:     C#, VS 2017                                              //
//  Platform:     AlienWare 17R5, WIN10, Visual Studio 2017                //
//  Application:  CSE681 Project3                                          //
//  Author:       Xiao Chen, Computer Engineering Syracuse University      //
//                (315) 560-7375, xchen149@syr.edu                         //                     
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This package was built to test if my program can meet all of
 *   requirements of project 3
 *   
 *   public interface:
 *   file_test()
 *   type_analysis_test()
 *   type_table_test()
 *   dependency_analysis_test()
 *   dependency_graph_test()
 *   Strong_Conponent_test()
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files: parser.cs RulesAndActions.cs ScopeStack.cs Semi.cs StrongConpo.cs 
 *     ITokenCollection.cs Element.cs FileMgr.cs TypeTable.cs Display.cs
 *     
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 1 November 2018
 *     - first release
 *     
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexer;
using FileUtilities;
using CodeAnalysis;
using StrongConpo;
using TypeTables;
using DepAnalysis;

namespace TestUbit
{
    ///////////////////////////////////////////////////////////////////
    // Test Class
    // - test if program can meet the requirement of project 3
    class TestUnits
    {
        //----< method to get files from whole solution >---------------------------------
        public void file_test(string[] args)
        {
            Console.Write("\n " + "File Included in Test: \n");
            Console.Write("\n " + "==================================================== \n");
            Console.Write("\n");
            foreach (var item in args)
            {
                Console.WriteLine(item);
            }
        }
        //----< test if I can analyze all files in the solution >---------------------------------

        public void type_analysis_test(string[] args)
        {
            Console.Write("\n  Test for Type analysis:");
            Console.Write("\n ==================================\n");
            foreach (string file in args)
            {
                Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));
                ITokenCollection semi = Factory.create();
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                    return;
                }
                Console.Write("\n  Type and Function Analysis");
                Console.Write("\n ----------------------------");
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();
                try
                {
                    while (semi.get().Count > 0)
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                Display.showMetricsTable(table);
                Console.Write("\n");

                semi.close();
            }
            Console.Write("\n\n");
        }
        //----< Gerate a type table of the solution >---------------------------------

        public void type_table_test(string[] args)
        {
            Console.Write("\n " + "Type Table for project 3 shows below: \n");
            Console.Write("\n " + "==================================================== \n");
            Console.Write("\n ");
            TypeTable tt = new TypeTable();
            tt = tt.GetTable(args);
            tt.show();
            Console.Write("\n\n");
        }
        //----< Show Dependency Relationship between files in Solution >---------------------------------

        public void dependency_analysis_test(string[] args)
        {
            Console.Write("\n " + "Dependency Analysis for project 3 shows below: \n");
            Console.Write("\n " + "==================================================== \n");
            Console.Write("\n ");
            DepenAnalysis dp = new DepenAnalysis();
            dp = dp.DepenAnalysises(args);
            //dp.show();
            dp.showFormal();
            Console.Write("\n\n");
        }
        //----< Generate Dependency Graph of the solution >---------------------------------

        public void dependency_graph_test(string[] args)
        {
            Console.Write("\n " + "Dependency Graph for project 3 shows below: \n");
            Console.Write("\n " + "==================================================== \n");
            Console.Write("\n ");
            
            CsGraph<string, string> csGraph = 
                new CsGraph<string, string>("Dep_Table");
            csGraph = csGraph.Creat_Graph(args);
            csGraph.showDependencies();
        }
        //----< Generate Strong Conponent of the solution >---------------------------------

        public void Strong_Conponent_test(string[] args)
        {
            Console.Write("\n " );
            Console.Write("\n " + "Strong Conponents for project 3 shows below: \n");
            Console.Write("\n " + "==================================================== \n");
            Console.Write("\n ");

            CsGraph<string, string> csGraph =
                new CsGraph<string, string>("Dep_Table");
            csGraph = csGraph.Creat_Graph(args);
            csGraph.sc_finder();
        }

        static void Main(string[] args)
        {
            FileFinder filef = new FileFinder();
            args = filef.get_cs();
            TestUnits test = new TestUnits();
            test.file_test(args);
            test.type_analysis_test(args);
            test.type_table_test(args);
            test.dependency_analysis_test(args);
            test.dependency_graph_test(args);
            test.Strong_Conponent_test(args);
            Console.Write("\n ");
            Console.Write("\n end of Test");
            Console.Write("\n If you need more test, please add folder into root folder of the solution");
            Console.ReadKey();
        }
    }
}
