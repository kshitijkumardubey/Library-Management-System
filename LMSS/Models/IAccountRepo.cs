using System.Collections.Generic;
namespace LMSS.Models
{
    public interface IAccountRepo
    {
        Account GetAccountByName(string username);
    }
}
