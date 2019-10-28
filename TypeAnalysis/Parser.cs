/////////////////////////////////////////////////////////////////////////////
//  Parser.cs -- Paser detects code constructs defined by rules            //
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
 *   Parser  - a collection of IRules
 *   BuildCodeAnalyzer - capture constructs defined by rules
 *   TestParser  -  contains testtub
 *   
 *   public interface:
 *   Parser()
 *   add() // add string to paerser table
 *   parser()
 *   build() // get parser table
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files: RulesAndActions.cs Semi.cs IRuleandAction.cs 
 *     Display.cs ITokenCollection.cs Element.cs FileMgr.cs
 *     
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 29 October 2018
 *     - first release
 *   ver 1.1 : 1 November 2018
 *     - Add rules to detect aliases
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lexer;
using FileUtilities;

namespace CodeAnalysis
{
    /////////////////////////////////////////////////////////
    // rule-based parser used for code analysis

    public class Parser
    {
        private List<IRule> Rules;

        public Parser()
        {
            Rules = new List<IRule>();
        }
        public void add(IRule rule)
        {
            Rules.Add(rule);
        }
        public void parse(Lexer.ITokenCollection semi)
        {
            // Note: rule returns true to tell parser to stop
            //       processing the current semiExp

            Display.displaySemiString(semi.ToString());

            foreach (IRule rule in Rules)
            {
                if (rule.test(semi))
                    break;
            }
        }
    }
    ///////////////////////////////////////////////////////////////////
    // BuildCodeAnalyzer class
    ///////////////////////////////////////////////////////////////////
    
    public class BuildCodeAnalyzer
    {
        Repository repo = new Repository();

        public BuildCodeAnalyzer(Lexer.ITokenCollection semi)
        {
            repo.semi = semi;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();
            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // false is default
            // action used for namespaces, classes, and functions
            PushStack push = new PushStack(repo);
            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);
            // capture class info
            DetectClass detectCl = new DetectClass();
            detectCl.add(push);
            parser.add(detectCl);
            // capture delegate info
            DetectDelegate detectDE = new DetectDelegate();
            SaveDeclar print_1 = new SaveDeclar(repo);
            detectDE.add(print_1);
            parser.add(detectDE);
            // capture function info
            DetectFunction detectFN = new DetectFunction();
            detectFN.add(push);
            parser.add(detectFN);
            // capture DetectAlias info
            DetectAlias detectAL = new DetectAlias();
            SaveDeclar print_2 = new SaveDeclar(repo);
            detectAL.add(print_2);
            parser.add(detectAL);
            // handle entering anonymous scopes, e.g., if, while, etc.
            DetectAnonymousScope anon = new DetectAnonymousScope();
            anon.add(push);
            parser.add(anon);
            // handle leaving scopes
            DetectLeavingScope leave = new DetectLeavingScope();
            PopStack pop = new PopStack(repo);
            leave.add(pop);
            parser.add(leave);
            // parser configured
            return parser;
        }
    }

    class TestParser
    {
        //----< Test Stub >--------------------------------------------------

#if (TEST_PARSER)

        static void Main(string[] args){
            Console.Write("\n  Demonstrating Parser");
            Console.Write("\n ======================\n");
            FileFinder filef = new FileFinder();
            args = filef.get_cs();
            foreach (string file in args){
                Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));
                ITokenCollection semi = Factory.create();
                if (!semi.open(file as string)){
                    Console.Write("\n  Can't open {0}\n\n", args[0]); return;
                }
                Console.Write("\n  Type and Function Analysis");
                Console.Write("\n ----------------------------");

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();
                try{
                    while (semi.get().Count > 0)
                        parser.parse(semi);
                }
                catch (Exception ex){
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                Display.showMetricsTable(table);
                Console.Write("\n");
                semi.close();
            }
            Console.Write("\n\n");
            Console.ReadKey();
        }
#endif
    }
}
