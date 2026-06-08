using GLMS.API.Models;

namespace GLMS.API.Services.Creational
{
    //create interface - references the contract class which acts as a our product interface
    public interface IContractFactory
    {
        Contract CreateContract(Client client);

    }
}
