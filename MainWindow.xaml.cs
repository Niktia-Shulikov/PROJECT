using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AGSS.Entities;
using AGSS.Repositories;
using Microsoft.Data.SqlClient;

namespace AGSS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<string> positions = new List<string> { "Заказчик", "Главный инженер", "Ведущий специалист", "Оператор" };
            LoginCombo.ItemsSource = positions;
        }

        private void LogInBTN_Click(object sender, RoutedEventArgs e)
        {
            if(LoginBox.Text.Trim() != string.Empty && PasBox.Password.Trim() != string.Empty)
            {
                if (LoginCombo.SelectedItem != null)
                {
                    string login = LoginBox.Text.Trim();
                    string password = PasBox.Password.Trim();
                    switch (LoginCombo.SelectedItem)
                    {
                        case "Заказчик":
                            if (AuthService.AuthenticateCustomer(login, password))
                            {
                                CustomerWindow customer = new CustomerWindow(CustomerRepository.GetCustomerID(login, password));
                                customer.Show();
                                this.Close();
                            }
                            else
                                MessageBox.Show("Неверный логин или пароль");
                            break;
                        case "Главный инженер":
                            if (AuthService.AuthenticateEnginner(login, password))
                            {
                                ChiefEngineerWindow engenner = new ChiefEngineerWindow();
                                engenner.Show();
                                this.Close();
                            }
                            else
                                MessageBox.Show("Неверный логин или пароль");
                            break;
                        case "Ведущий специалист":
                            if (AuthService.AuthenticateSpecialist(login, password))
                            {
                                LeadSpecialistWindow specialist = new LeadSpecialistWindow(SpecialistRepository.GetSpecialist(login, password));
                                specialist.Show();
                                this.Close();
                            }
                            else
                                MessageBox.Show("Неверный логин или пароль");
                            break;
                        case "Оператор":
                            if (AuthService.AuthenticateOperator(login, password))
                            {
                                OperatorWindow window = new OperatorWindow(OperatorRepository.GetOperator(login, password));
                                window.Show();
                                this.Close();
                            }
                            else
                                MessageBox.Show("Неверный логин или пароль");
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Выберите роль!");
                }
            }
            else
            {
                MessageBox.Show("Заполните все поля!");
            }
        }
    }
}