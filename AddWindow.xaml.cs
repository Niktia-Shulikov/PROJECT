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
using AGSS.Repositories;

namespace AGSS
{
    /// <summary>
    /// Логика взаимодействия для AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        public AddWindow()
        {
            InitializeComponent();
            SpecialistCombo.ItemsSource = SpecialistRepository.GetFreeSpecialists();
            SpecialistCombo.DisplayMemberPath = "FullName";
            CustomerCombo.ItemsSource = CustomerRepository.GetCustomers();
            CustomerCombo.DisplayMemberPath = "OrganizationName";
        }

        private void AddBTN_Click(object sender, RoutedEventArgs e)
        {
            string Name = ProjectNameBox.Text.Trim();
            string Note = NotesBox.Text.Trim();
            if(Name != string.Empty && Note != string.Empty && CustomerCombo.SelectedItem != null && SpecialistCombo.SelectedItem != null)
            {
                LeadSpecialist lead = (LeadSpecialist)SpecialistCombo.SelectedItem;
                Customer customer = (Customer)CustomerCombo.SelectedItem;
                Project project = new Project { ProjectName = Name, Notes = Note, LeadSpecialistId = lead.LeadSpecialistId, CustomerId = customer.CustomerId, ChiefEnginnerId = EngineerRepository.GetEngineer() };
                using(var context = new GravitySurveyOnDeleteNoAction())
                {
                    context.Projects.Add(project);
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

        private void AddCustomerBTN_Click(object sender, RoutedEventArgs e)
        {
            AddCustomerWindow addCustomer = new AddCustomerWindow();
            addCustomer.ShowDialog();
            if(addCustomer.DialogResult == true)
            {
                CustomerCombo.ItemsSource = CustomerRepository.GetCustomers();
            }
        }
    }
}
