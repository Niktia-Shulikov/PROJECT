using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AGSS.Entities;

namespace AGSS
{
    public static class CustomerRepository
    {
        public static int GetCustomerID(string login, string password)
        {
            using (var context = new GravitySurveyOnDeleteNoAction())
            {
                return context.Customers.FirstOrDefault(c => c.Login == login && c.Password == password).CustomerId;
            }
        }

        public static List<Customer> GetCustomers()
        {
            using(var context = new GravitySurveyOnDeleteNoAction())
            {
                return context.Customers.ToList();
            }
        }
    }
}
