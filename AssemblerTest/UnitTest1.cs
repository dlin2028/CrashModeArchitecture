using System;
using DavLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Assembler
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void NoOpCodeGreaterThan255()
        {
            string[] values = Enum.GetNames(typeof(OpCodes));
            foreach (var value in values)
            {
                OpCodes code = (OpCodes)Enum.Parse(typeof(OpCodes), value);
                int codeInt = (int)code;
                Assert.IsTrue(codeInt <= 255, $"{code} was too large, {codeInt} > 255");
            }
        }
    }
}
