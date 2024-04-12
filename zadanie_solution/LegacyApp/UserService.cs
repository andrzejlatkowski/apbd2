using System;

namespace LegacyApp

{
    public interface IClientRepository
    {
        Client GetById(int idClient);
    }

    public interface ICreditLimitService
    {
        int GetCreditLimit(string lastName, DateTime birthdate);
    }
    public class UserService
    {
        // IoT - Inversion of Control
        private IClientRepository _clientRepository;
        private ICreditLimitService _creditLimitService;
        
        [Obsolete]
        public UserService()
        {
            _clientRepository = new ClientRepository();
            _creditLimitService = new UserCreditService();
        }

        // Inject / wstrzykiwanie zależności
        public UserService(IClientRepository clientRepository, ICreditLimitService creditLimitService)
        {
            _clientRepository = clientRepository;
            _creditLimitService = creditLimitService;
        }
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            // logika biznesowa - walidacja
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            // logika biznesowa - walidacja maila
            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }
            
            // logika biznesowa 
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }

            // infrastruktura - komunikacja z bazą
            var client = _clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            
            // logika biznesowa + infrastruktura
            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (client.Type == "ImportantClient")
            {
                
                int creditLimit = _creditLimitService.GetCreditLimit(user.LastName, user.DateOfBirth);
                creditLimit = creditLimit * 2;
                user.CreditLimit = creditLimit;
                
            }
            else
            {
                user.HasCreditLimit = true;
               
                int creditLimit = _creditLimitService.GetCreditLimit(user.LastName, user.DateOfBirth);
                user.CreditLimit = creditLimit;
            
            }
            
            // logika biznesowa
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            // infrastruktura - zapis do bazy danych
            UserDataAccess.AddUser(user);
            return true;
        }
    }
}
