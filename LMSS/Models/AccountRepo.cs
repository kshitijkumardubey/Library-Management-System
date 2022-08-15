using System.Collections.Generic;
using System.Linq;

namespace LMSS.Models
{
    public class AccountRepo : IAccountRepo
    {
        private readonly LibraryManagementSystemContext _libraryManagementSystemContext;

        public AccountRepo(LibraryManagementSystemContext libraryManagementSystemContext)
        {
            _libraryManagementSystemContext = libraryManagementSystemContext;
        }
        public Account GetAccountByName(string username)
        {
            return _libraryManagementSystemContext.Accounts.FirstOrDefault(s => s.UserName == username);


        }
    }
}
