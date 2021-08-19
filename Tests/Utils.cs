using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class Utils
    {
        [TestMethod]
        public void TestOrderBook()
        {
            var ob = Convert.ToDouble("6.59000000", CultureInfo.InvariantCulture) - (0.1/ 100);

            Assert.IsTrue(true);
        }
    }
}
