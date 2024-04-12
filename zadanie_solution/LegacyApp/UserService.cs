using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidName(firstName, lastName) || !IsValidEmail(email) || !IsOldEnough(dateOfBirth))
            {
                return false;
            }

            // infrastruktura - komunikacja z bazą
            var client = GetClientById(clientId);
            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            // logika biznesowa + infrastruktura
            SetUserCredit(user, client);
            
            
            // logika biznesowa
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            // infrastruktura - zapis do bazy danych
            UserDataAccess.AddUser(user);
            return true;
        }

        private void SetUserCredit(User user, Client client)
        {
            using (var userCreditService = new UserCreditService())
            {
                if (client.Type == "VeryImportantClient")
                {
                    user.HasCreditLimit = false;
                }
                else if (client.Type == "ImportantClient")
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    creditLimit *= 2;
                    user.CreditLimit = creditLimit;
                }
                else
                {
                    user.HasCreditLimit = true;
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }
        }

        private bool IsValidName(string firstName, string lastName)
        {
            return !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName);
        }

        private bool IsValidEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }

        private bool IsOldEnough(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }
            return age >= 21;
        }
        
        private Client GetClientById(int clientId)
        {
            var clientRepository = new ClientRepository();
            return clientRepository.GetById(clientId);
        }
        
        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }
    }
}
