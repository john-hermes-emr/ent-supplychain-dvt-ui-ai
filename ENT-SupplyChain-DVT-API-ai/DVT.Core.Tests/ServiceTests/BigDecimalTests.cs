using DVT.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DVT.Core.Tests.ServiceTests
{
    public class BigDecimalTests
    {
        [Theory]
        [MemberData(nameof(DecimalTestData.Data), MemberType=typeof(DecimalTestData))]
        public void SmallDecimalTest(string value1, string value2)
        {         
            // Act
            BigDecimal result = value1;

            // Assert        
            Assert.Equal(value2, result.ToString());
        }
    }
    public class DecimalTestData
    {
        public static IEnumerable<object[]> Data =>
            new List<Object[]>
            {
                    new object[]{"6e-005","6E-5"},
                    new object[]{"7.2e-005","72E-6"},
                    new object[]{"2.2e5","22E4"},
                    new object[]{"2.2e-5","22E-6"},
                    new object[]{"55e4","55E4"},
                    new object[]{"2.333e5","2333E2"},
                    new object[]{"7.2e-005","72E-6"},
                    new object[]{"1e-005","1E-5"},
                    new object[]{"1.32e-005","132E-7"},
                    new object[]{"6e-005","6E-5"},
                    new object[]{"5.3e-005","53E-6"},
                    new object[]{"1e-006","1E-6"},
                    new object[]{"9.3e-005","93E-6"},
                    new object[]{"8e-005","8E-5"},
                    new object[]{"7.7e-005","77E-6"},
                    new object[]{"7e-005","7E-5"},
                    new object[]{"5e-005","5E-5"},
                    new object[]{"4e-005","4E-5"},
                    new object[]{"9e-005","9E-5"},
                    new object[]{"2e-005","2E-5"},
                    new object[]{"6.3e-005","63E-6"},
                    new object[]{"2.2e-005","22E-6"},
                    new object[]{"3e-005","3E-5"},
                    new object[]{"1.5e-005","15E-6"},
                    new object[]{"2.4e-005","24E-6"},
                    new object[]{"5.2e-005","52E-6"},
                    new object[]{"5.6e-005","56E-6"},
                    new object[]{"9.1e-005","91E-6"},
                    new object[]{"5.5e-005","55E-6"},
                    new object[]{"2.9e-005","29E-6"},
                    new object[]{"8.8e-005","88E-6"},
                    new object[]{"7.4e-005","74E-6"},
                    new object[]{"3.9e-005","39E-6"},
                    new object[]{"7e-006","7E-6"},
                    new object[]{"9.8e-005","98E-6"},
                    new object[]{"3.12e-005","312E-7"},
                    new object[]{"1.1e-005","11E-6"},
                    new object[]{"4.4e-005","44E-6"},
                    new object[]{"7.8e-005","78E-6"},
                    new object[]{"7.6e-005","76E-6"},
                    new object[]{"9.6e-005","96E-6"},
                    new object[]{"7.5e-005","75E-6"},
                    new object[]{"8.5e-005","85E-6"},
                    new object[]{"6.6e-005","66E-6"},
                    new object[]{"8.5e-006","85E-7"},
                    new object[]{"9.07185e-005","907185E-10"}
            };
    }
}
