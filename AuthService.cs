using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AGSS.Entities;

namespace AGSS
{
    public static class AuthService
    {
        public static bool AuthenticateCustomer(string username, string password)
        {
            using (var context = new GravitySurveyOnDeleteNoAction())
            {
                return context.Customers.Any(c => c.Login == username && c.Password == password);
            }
        }

        public static bool AuthenticateEnginner(string username, string password)
        {
            using (var context = new GravitySurveyOnDeleteNoAction())
            {
                return context.ChiefEnginners.Any(c => c.Login == username && c.Password == password);
            }
        }

        public static bool AuthenticateOperator(string username, string password)
        {
            using (var context = new GravitySurveyOnDeleteNoAction())
            {
                return context.Operators.Any(c => c.Login == username && c.Password == password);
            }
        }

        public static bool AuthenticateSpecialist(string username, string password)
        {
            using (var context = new GravitySurveyOnDeleteNoAction())
            {
                return context.LeadSpecialists.Any(c => c.Login == username && c.Password == password);
            }
        }

    }
}
