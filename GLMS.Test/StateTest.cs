using Global_Logistics_Management_System___ST10439898.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLMS.Test
{
    public class StateTest
    {
        [Fact]
        public void Contract_DefaultsToDraft()
        {
            // Arrange & Act
            var contract = new ContractViewModel
            {
                signedAgreementPath = "agreement.pdf"
            };

            // Assert
            Assert.Equal("0", contract.contractStatus.ToString());
        }
    }
}
