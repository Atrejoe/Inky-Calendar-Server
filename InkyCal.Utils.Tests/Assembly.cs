using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
[assembly: CaptureConsole]
