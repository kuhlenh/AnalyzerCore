using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using Analyzer1Core;

namespace Analyzer1Core.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void DoStuff() => { var a = new string[0]; }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "Analyzer1Core",
                Message = String.Format("Reduce allocations and use Array.Empty", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 41)
                        }
            };

            VerifyCSharpDiagnostic(test,expected);

            //        var fixtest = @"
            //using System;
            //using System.Collections.Generic;
            //using System.Linq;
            //using System.Text;
            //using System.Threading.Tasks;
            //using System.Diagnostics;

            //namespace ConsoleApplication1
            //{
            //    class TypeName
            //    {   
            //        void DoStuff() => { var a = Array.Empty<string>(); }
            //    }
            //}";
            //        VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void TestMethod3() {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void DoStuff() => { var a = new string[]{}; }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "Analyzer1Core",
                Message = String.Format("Reduce allocations and use Array.Empty", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 41)
                        }
            };

            VerifyCSharpDiagnostic(test,expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void DoStuff() => { var a = Array.Empty; }
        }
    }";
            //VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void TestMethod4() {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void DoStuff() => { var a = new string[]{""a"", ""b""}; }
        }
    }";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestMethod5() {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void DoStuff() => { var a = new string[4]; }
        }
    }";
            VerifyCSharpDiagnostic(test);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new Analyzer1CoreCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new Analyzer1CoreAnalyzer();
        }
    }
}
