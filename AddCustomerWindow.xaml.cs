using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AGSS.Entities;

namespace AGSS
{
    /// <summary>
    /// Логика взаимодействия для AddCustomerWindow.xaml
    /// </summary>
    public partial class AddCustomerWindow : Window
    {
        public AddCustomerWindow()
        {
            InitializeComponent();
        }

        private void AddBTN_Click(object sender, RoutedEventArgs e)
        {
            string Name = CustomerNameBox.Text.Trim();
            string Person = PersonBox.Text.Trim();
            string Address = AddressBox.Text.Trim();
            string Phone = PhoneBox.Text.Trim();
            string Email = EmailBox.Text.Trim();
            string Login = LoginBox.Text.Trim();
            string Password = PasswordBox.Text.Trim();

            if(Name != string.Empty &&
                Person != string.Empty &&
                Address != string.Empty &&
                Phone != string.Empty &&
                Email != string.Empty &&
                Login != string.Empty &&
                Password != string.Empty)
            {
                Customer customer = new Customer { OrganizationName = Name, Password = Password, Address = Address, ContactPerson = Person, Email = Email, Login = Login, Phone = Phone };
                using(var context = new GravitySurveyOnDeleteNoAction())
                {
                    context.Customers.Add(customer);
                    context.SaveChanges();
                    DialogResult = true;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста заполните все поля!");
            }
        }
    }
}
