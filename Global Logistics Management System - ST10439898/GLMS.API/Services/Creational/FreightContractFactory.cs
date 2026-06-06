using GLMS.API.Models;

namespace GLMS.API.Services.Creational
{
    public class FreightContractFactory : IContractFactory
    {
        public Contract CreateContract (Client client)
        {
            return new Contract
            {
                clientID = client.clientID,
                startDate = DateTime.Now,
                endDate = DateTime.Now.AddYears(1),
                contractStatus = Contract.Status.Draft
            };
        }
    }
}
