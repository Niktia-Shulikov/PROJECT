using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.IdentityModel.Tokens;
using OxyPlot.Series;
using OxyPlot;

namespace AGSS
{
    /// <summary>
    /// Логика взаимодействия для LeadSpecialistWindow.xaml
    /// </summary>
    public partial class LeadSpecialistWindow : Window
    {
        private LeadSpecialist LeadSpecialist;
        private int ProjectId;

        private ObservableCollection<Area> areaData = new ObservableCollection<Area>();
        private ObservableCollection<AreaCoordinate> areaCoordinates = new ObservableCollection<AreaCoordinate>();

        private ObservableCollection<Profile> profileData = new ObservableCollection<Profile>();
        private ObservableCollection<ProfileCoordinate> profileCoordinates = new ObservableCollection<ProfileCoordinate>();

        private ObservableCollection<MagnetChannel> channelData = new ObservableCollection<MagnetChannel>();

        private ObservableCollection<Flight> flightData = new ObservableCollection<Flight>();
        private ObservableCollection<Spectrometer> spectrometerData = new ObservableCollection<Spectrometer>();
        private ObservableCollection<Metadata> metadataData = new ObservableCollection<Metadata>();

        public LeadSpecialistWindow(LeadSpecialist specialist)
        {
            InitializeComponent();
            LeadSpecialist = specialist;
            ProjectCombo.ItemsSource = ProjectRepository.GetProjectBySpecialist(LeadSpecialist);
            ProjectCombo.DisplayMemberPath = "ProjectName";
            AddCombo.ItemsSource = new List<string> { "Метаданые", "Спектрометер", "Полет","Канал", "Координаты профиля", "Профиль", "Координаты площади", "Площадь" };
        }

        private void ProjectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ProjectCombo.SelectedItem != null)
            {
                Project project = (Project)ProjectCombo.SelectedItem;
                ProjectId = ProjectRepository.GetIDByProjectName(project.ProjectName);
                LoadProjectData();
            }
            else
            {
                MessageBox.Show("Выберите проект");
            }
        }

        private void LoadProjectData()
        {
            if (ProjectCombo.SelectedItem != null)
            {
                var Projects = new ObservableCollection<Project>(ProjectRepository.GetDataOfProject(ProjectId));
                ProjectView.ItemsSource = Projects;

                foreach (var pr in Projects)
                {
                    pr.PropertyChanged += (s, e) => ProjectRepository.SaveChanges((Project)s);
                }

                var Specialists = new ObservableCollection<LeadSpecialist>{ LeadSpecialist };
                SpecialistView.ItemsSource = Specialists;

                foreach (var pr in Specialists)
                {
                    pr.PropertyChanged += (s, e) => SpecialistRepository.SaveChanges((LeadSpecialist)s);
                }

                var Engineers = new ObservableCollection<ChiefEnginner>(EngineerRepository.GetDataOfEngineer());
                EngineerView.ItemsSource = Engineers;

                foreach (var pr in Engineers)
                {
                    pr.PropertyChanged += (s, e) => EngineerRepository.SaveChanges((ChiefEnginner)s);
                }

                var Operators = new ObservableCollection<Operator>(OperatorRepository.GetDataOfOperator());

                OperatorView.ItemsSource = Operators;

                foreach (var pr in Operators)
                {
                    pr.PropertyChanged += (s, e) => OperatorRepository.SaveChanges((Operator)s);
                }
            }
        }

        private void LoadAreaGraph()
        {
            var plotModel = new PlotModel { Title = "График координат площади" };

            plotModel.Series.Clear();

            var series = new LineSeries { Title = "Линия", MarkerType = MarkerType.Circle };

            foreach (var c in areaCoordinates)
            {
                series.Points.Add(new DataPoint((double)c.X, (double)c.Y));
            }

            if(areaCoordinates.Count >= 1)
                series.Points.Add(new DataPoint((double)areaCoordinates[0].X, (double)areaCoordinates[0].Y));

            plotModel.Series.Add(series);
            DataPlot.Model = plotModel;
        }

        private void LoadProfileGraph()
        {
            PlotModel model = new PlotModel { Title = "График профилей" };

            model.Series.Clear();

            List<LineSeries> lines = new List<LineSeries>();

            var line1 = new LineSeries { MarkerType = MarkerType.Circle };

            LoadDataAreaCoordinates();

            if (areaCoordinates.Count > 0)
            {
                foreach (var c in areaCoordinates)
                {
                    line1.Points.Add(new DataPoint((double)c.X, (double)c.Y));
                }
                line1.Points.Add(new DataPoint((double)areaCoordinates[0].X, (double)areaCoordinates[0].Y));

                lines.Add(line1);

                if (profileCoordinates.Count > 0)
                {
                    foreach (var p in profileData)
                    {
                        LineSeries line = new LineSeries { MarkerType = MarkerType.Circle, Title = $"Профиль {p.ProfileId}" };
                        foreach (var c in profileCoordinates)
                        {
                            if (c.ProfileId == p.ProfileId)
                            {
                                line.Points.Add(new DataPoint((double)c.X, (double)c.Y));
                            }
                        }
                        lines.Add(line);
                    }
                }

                foreach (var l in lines)
                    model.Series.Add(l);

                DataPlot.Model = model;
            }
        }

        private void LoadChannelGraph()
        {
            PlotModel model = new PlotModel { Title = "График измерений канала" };

            model.Series.Clear();

            List<LineSeries> lines = new List<LineSeries>();

            LoadDataProfile();

            if (channelData.Count > 0 && profileCoordinates.Count > 0 && profileData.Count > 0)
            {
                foreach (var p in profileData)
                {
                    LineSeries line = new LineSeries { MarkerType = MarkerType.Circle, Title = $"Профиль {p.ProfileId}" };
                    int i = 1;
                    foreach (var coord in profileCoordinates)
                    {
                        if (p.ProfileId == coord.ProfileId)
                        {
                            foreach (var c in channelData)
                            {
                                if (coord.ProfileCoordinatesId == c.ProfileCoordinatesId)
                                {
                                    line.Points.Add(new DataPoint(i++, (double)c.MeasurementResult));
                                }
                            }
                        }
                    }
                    lines.Add(line);
                }

                foreach (var l in lines)
                    model.Series.Add(l);

                DataPlot.Model = model;
            }
        }

       

        private void LoadDataArea()
        {
            areaData = new ObservableCollection<Area>(AreaRepository.GetDataOfArea(ProjectId));
        }

        private void LoadDataAreaCoordinates()
        {
            areaCoordinates = new ObservableCollection<AreaCoordinate>(AreaRepository.GetAreaCoordinates(AreaRepository.GetAreaIDByProjectID(ProjectId)));
        }

        private void LoadDataProfile()
        {
            profileData = new ObservableCollection<Profile>(ProfileRepository.GetProfiles(AreaRepository.GetAreaIDByProjectID(ProjectId)));
        }

        private void LoadDataProfileCoordinates()
        {
            profileCoordinates = new ObservableCollection<ProfileCoordinate>(ProfileRepository.GetProfileCoordinates(AreaRepository.GetAreaIDByProjectID(ProjectId)));
        }

        private void LoadDataChannel()
        {
            channelData = new ObservableCollection<MagnetChannel>(ChannelsRepository.GetChannels(
                                AreaRepository.GetAreaIDByProjectID(ProjectId)));
        }

        private void LoadDataFlight()
        {
            flightData = new ObservableCollection<Flight>(FlightRepository.GetDataOfFlight(ProjectId));
        }

        private void LoadDataSpectrometer()
        {
            spectrometerData = new ObservableCollection<Spectrometer>(SpectrometerRepository.GetDataOfSpectrometer(
                                FlightRepository.GetFlightIDByProjectID(ProjectId)));
        }

        private void LoadDataMetadata()
        {
            metadataData = new ObservableCollection<Metadata>(MetadataRepository.GetDataOfMetadata(
                                SpectrometerRepository.GetSpectrometerIDByFlightID(
                                FlightRepository.GetFlightIDByProjectID(ProjectId))));
        }

        private int ReturnNumberDataTree()
        {
            if (DataTree.SelectedItem is TreeViewItem selectedItem)
            {
                if (ProjectCombo.SelectedItem != null)
                {
                    switch (selectedItem.Header.ToString())
                    {
                        case "Площадь":
                            return 1;
                        case "Профиль":
                            return 2;
                        case "Канал":
                            return 3;
                        case "Полет":
                            return 4;
                        case "Спектрометр":
                            return 5;
                        case "Метаданные":
                            return 6;
                    }
                }
            }
            return 0;
        }

        private void DataTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int choise = ReturnNumberDataTree();
            switch (choise)
            {
                case 1:
                    AreaColumns();
                    AreaCoordinateColumns();
                    CoordinateView.Visibility = Visibility;

                    LoadDataArea();

                    Data.ItemsSource = areaData;

                    foreach (var pr in areaData)
                    {
                        pr.PropertyChanged += (s, e) => AreaRepository.SaveChanges((Area)s);
                    }

                    LoadDataAreaCoordinates();

                    CoordinateView.ItemsSource = areaCoordinates;

                    foreach (var pr in areaCoordinates)
                    {
                        pr.PropertyChanged += (s, e) => AreaRepository.SaveChangesCoord((AreaCoordinate)s);
                        pr.PropertyChanged += (s, e) => LoadAreaGraph();
                    }

                    LoadAreaGraph();
                    break;
                case 2:
                    ProfileColumns();
                    ProfileCoordinateColumns();
                    CoordinateView.Visibility = Visibility;

                    LoadDataProfile();

                    Data.ItemsSource = profileData;

                    foreach (var pr in profileData)
                    {
                        pr.PropertyChanged += (s, e) => ProfileRepository.SaveChanges((Profile)s);
                        LoadProfileGraph();
                    }

                    LoadDataProfileCoordinates();

                    CoordinateView.ItemsSource = profileCoordinates;

                    foreach (var pr in profileCoordinates)
                    {
                        pr.PropertyChanged += (s, e) => ProfileRepository.SaveChangesCoord((ProfileCoordinate)s);
                        pr.PropertyChanged += (s, e) => LoadProfileGraph();
                    }

                    LoadProfileGraph();
                    break;
                case 3:
                    ChannelsColumns();
                    ProfileCoordinateColumns();
                    CoordinateView.Visibility = Visibility;

                    LoadDataChannel();

                    Data.ItemsSource = channelData;

                    foreach (var pr in channelData)
                    {
                        pr.PropertyChanged += (s, e) => ChannelsRepository.SaveChanges((MagnetChannel)s);
                        pr.PropertyChanged += (s, e) => LoadChannelGraph();
                    }

                    LoadDataProfileCoordinates();

                    CoordinateView.ItemsSource = profileCoordinates;

                    foreach (var pr in profileCoordinates)
                    {
                        pr.PropertyChanged += (s, e) => ProfileRepository.SaveChangesCoord((ProfileCoordinate)s);
                    }

                    LoadChannelGraph();
                    break;
                case 4:
                    FlightColumns();
                    CoordinateView.Visibility = Visibility.Hidden;

                    LoadDataFlight();

                    Data.ItemsSource = flightData;

                    foreach (var pr in flightData)
                    {
                        pr.PropertyChanged += (s, e) => FlightRepository.SaveChanges((Flight)s);
                    }
                    break;
                case 5:
                    SpectrometerColumns();
                    CoordinateView.Visibility = Visibility.Hidden;

                    LoadDataSpectrometer();

                    Data.ItemsSource = spectrometerData;

                    foreach (var pr in spectrometerData)
                    {
                        pr.PropertyChanged += (s, e) => SpectrometerRepository.SaveChanges((Spectrometer)s);
                    }
                    break;
                case 6:
                    MetadataColumns();
                    CoordinateView.Visibility = Visibility.Hidden;

                    LoadDataMetadata();

                    Data.ItemsSource = metadataData;

                    foreach (var pr in metadataData)
                    {
                        pr.PropertyChanged += (s, e) => MetadataRepository.SaveChanges((Metadata)s);
                    }
                    break;
            }
        }

        private DataTemplate CreateTextBoxTemplate(string propertyName)
        {
            DataTemplate template = new DataTemplate();

            FrameworkElementFactory textBoxFactory = new FrameworkElementFactory(typeof(TextBox));
            textBoxFactory.SetBinding(TextBox.TextProperty, new Binding(propertyName)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            template.VisualTree = textBoxFactory;
            return template;
        }

        private void AreaColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "№ площади", DisplayMemberBinding = new Binding ("AreaId")},
                    new GridViewColumn { Header = "Геологическая информация", CellTemplate = CreateTextBoxTemplate("GeologicalInfo") },
                    new GridViewColumn { Header = "Площадь", CellTemplate = CreateTextBoxTemplate("Area1") },
                    new GridViewColumn { Header = "Количество изломов", CellTemplate = CreateTextBoxTemplate("BreaksCount") },
                    new GridViewColumn { Header = "Количество профилей", CellTemplate = CreateTextBoxTemplate("ProfileCount") }
                }
            };
        }

        private void AreaCoordinateColumns()
        {
            CoordinateView.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "X", CellTemplate = CreateTextBoxTemplate("X") },
                    new GridViewColumn { Header = "Y", CellTemplate = CreateTextBoxTemplate("Y") }
                }
            };
        }

        private void ProfileColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "№ Профиля", DisplayMemberBinding = new Binding("ProfileId") },
                    new GridViewColumn { Header = "Количество изломов", CellTemplate = CreateTextBoxTemplate("BreaksCount")}
                }
            };
        }

        private void ProfileCoordinateColumns()
        {
            CoordinateView.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "№ координат профиля", DisplayMemberBinding = new Binding("ProfileCoordinatesId") },
                    new GridViewColumn { Header = "X", CellTemplate = CreateTextBoxTemplate("X") },
                    new GridViewColumn { Header = "Y", CellTemplate = CreateTextBoxTemplate("Y") },
                    new GridViewColumn { Header = "№ профиля", DisplayMemberBinding = new Binding("ProfileId") }
                }
            };
        }

        private void ChannelsColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "Значение", CellTemplate = CreateTextBoxTemplate("MeasurementResult")},
                    new GridViewColumn {Header = "№ координат профиля", CellTemplate = CreateTextBoxTemplate("ProfileCoordinatesId")}
                }
            };
        }

        private void FlightColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "№ полёта", DisplayMemberBinding = new Binding("FlightId") },
                    new GridViewColumn { Header = "Дата начала полета", CellTemplate = CreateTextBoxTemplate("StartDateTime") },
                    new GridViewColumn { Header = "Дата конца полета", CellTemplate = CreateTextBoxTemplate("EndDateTime") },
                    new GridViewColumn { Header = "Высота над уровнем моря", CellTemplate = CreateTextBoxTemplate("AltitudeAboveSea") },
                    new GridViewColumn { Header = "Высота над уровнем земли", CellTemplate = CreateTextBoxTemplate("AltitudeAboveGround")},
                    new GridViewColumn { Header = "Средняя скорость", CellTemplate = CreateTextBoxTemplate("Speed")},
                    new GridViewColumn { Header = "№ оператора", DisplayMemberBinding = new Binding("OperatorId") }
                }
            };
        }

        private void SpectrometerColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn {Header = "№ спектрометра", DisplayMemberBinding = new Binding("SpectrometerId") },
                    new GridViewColumn { Header = "Время измерения", CellTemplate = CreateTextBoxTemplate("MeasurementTime") },
                    new GridViewColumn { Header = "Количество импульсов", CellTemplate = CreateTextBoxTemplate("PulseCount") },
                    new GridViewColumn { Header = "Общий счет", CellTemplate = CreateTextBoxTemplate("TotalCount") },
                    new GridViewColumn { Header = "Количество энергетических окон", CellTemplate = CreateTextBoxTemplate("EnergyWindowsCount")},
                    new GridViewColumn {Header = "№ полета", DisplayMemberBinding = new Binding("FlightId") },
                }
            };
        }

        private void MetadataColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "№ спектрометра", DisplayMemberBinding = new Binding("SpectrometerId")},
                    new GridViewColumn { Header = "Описание оборудования", CellTemplate = CreateTextBoxTemplate("EquipmentDescription") },
                    new GridViewColumn { Header = "Примечания", CellTemplate = CreateTextBoxTemplate("Notes") }
                }
            };
        }

        private void DeleteBTN_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectCombo.SelectedItem != null)
            {
                if (Data.SelectedItem != null)
                {
                    var select = Data.SelectedItem;
                    if (select is MagnetChannel)
                    {
                        using (var context = new GravitySurveyOnDeleteNoAction())
                        {
                            context.Remove(select);
                            context.SaveChanges();
                            channelData.Remove((MagnetChannel)select);
                        }
                        LoadChannelGraph();
                    }

                    if (select is Profile)
                    {
                        if (MessageBox.Show("Связанные данные так же будут удалены!", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                        {
                            using (var context = new GravitySurveyOnDeleteNoAction())
                            {
                                context.Profiles.Remove((Profile)select);
                                context.SaveChanges();
                                profileData.Remove((Profile)select);
                            }
                            AreaRepository.AreaProfileCountMinus(ProjectId);
                            LoadProfileGraph();
                        }
                    }

                    if (select is Area)
                    {
                        if (MessageBox.Show("Связанные данные так же будут удалены!", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                        {
                            using (var context = new GravitySurveyOnDeleteNoAction())
                            {
                                context.Areas.Remove((Area)select);
                                context.SaveChanges();
                                areaData.Remove((Area)select);
                            }
                        }
                    }

                    if (select is Flight)
                    {
                        if (MessageBox.Show("Связанные данные так же будут удалены!", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                        {
                            using (var context = new GravitySurveyOnDeleteNoAction())
                            {
                                context.Areas.Remove((Area)select);
                                context.SaveChanges();
                                areaData.Remove((Area)select);
                            }
                        }
                    }

                    if (select is Spectrometer)
                    {
                        if (MessageBox.Show("Связанные данные так же будут удалены!", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                        {
                            using (var context = new GravitySurveyOnDeleteNoAction())
                            {
                                context.Spectrometers.Remove((Spectrometer)select);
                                context.SaveChanges();
                                spectrometerData.Remove((Spectrometer)select);
                            }
                        }
                    }

                    if (select is Metadata)
                    {
                        using (var context = new GravitySurveyOnDeleteNoAction())
                        {
                            context.Metadata.Remove((Metadata)select);
                            context.SaveChanges();
                            metadataData.Remove((Metadata)select);
                        }
                    }
                }
                else
                {
                    if (CoordinateView.SelectedItem != null)
                    {
                        var select = CoordinateView.SelectedItem;
                        if (select is AreaCoordinate)
                        {
                            using (var context = new GravitySurveyOnDeleteNoAction())
                            {
                                context.AreaCoordinates.Remove((AreaCoordinate)select);
                                context.SaveChanges();
                                areaCoordinates.Remove((AreaCoordinate)select);
                            }
                            AreaRepository.AreaBreakCountMinus(ProjectId);
                            LoadDataArea();
                            LoadDataAreaCoordinates();
                            Data.ItemsSource = areaData;
                            CoordinateView.ItemsSource = areaCoordinates;
                            LoadAreaGraph();
                        }

                        if (select is ProfileCoordinate)
                        {
                            if (MessageBox.Show("Связанные данные так же будут удалены!", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                            {
                                using (var context = new GravitySurveyOnDeleteNoAction())
                                {
                                    context.ProfileCoordinates.Remove((ProfileCoordinate)select);
                                    context.SaveChanges();
                                    profileCoordinates.Remove((ProfileCoordinate)select);
                                }
                                ProfileCoordinate profile = (ProfileCoordinate)select;
                                int id = (int)profile.ProfileId;
                                ProfileRepository.ProfileBreakCountMinus(id);
                                int choise = ReturnNumberDataTree();
                                switch (choise)
                                {
                                    case 2:
                                        LoadDataProfile();
                                        Data.ItemsSource = profileData;
                                        LoadProfileGraph();
                                        break;
                                    case 3:
                                        LoadDataChannel();
                                        Data.ItemsSource = channelData;
                                        LoadChannelGraph();
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Выберите данные для удаления");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите проект!");
            }
        }

        private void AddBTN_Click(object sender, RoutedEventArgs e)
        {
            int choise = ReturnNumberDataTree();
            if (AddCombo.SelectedItem != null)
            {
                switch (AddCombo.SelectedItem.ToString())
                {
                    case "Площадь":
                        if (oneBox.Text.Trim().IsNullOrEmpty() &&
                            twoBox.Text.Trim().IsNullOrEmpty())
                        {
                            MessageBox.Show("Пожалуйста заполните все поля!");
                        }
                        else
                        {
                            Area area = new Area { GeologicalInfo = oneBox.Text, Area1 = double.Parse(twoBox.Text.Trim().Replace('.', ',')), ProfileCount = 0, BreaksCount = 0, ProjectId = ProjectId };
                            AreaRepository.Add(area);
                            oneBox.Text = "";
                            twoBox.Text = "";
                            areaData.Add(area);
                        }
                        break;
                    case "Координаты площади":
                        if (oneBox.Text.Trim().IsNullOrEmpty() &&
                            twoBox.Text.Trim().IsNullOrEmpty())
                        {
                            MessageBox.Show("Пожалуйста заполните все поля!");
                        }
                        else
                        {
                            AreaCoordinate areacoord = new AreaCoordinate { X = double.Parse(oneBox.Text.Trim().Replace('.', ',')), Y = double.Parse(twoBox.Text.Trim().Replace('.', ',')), AreaId = AreaRepository.GetAreaIDByProjectID(ProjectId) };
                            AreaRepository.AddCoordinate(areacoord);
                            AreaRepository.AreaBreakCount(ProjectId);
                            oneBox.Text = "";
                            twoBox.Text = "";
                            areaCoordinates.Add(areacoord);

                            switch (choise)
                            {
                                case 1:
                                    LoadDataArea();
                                    Data.ItemsSource = areaData;
                                    LoadAreaGraph();
                                    break;
                                case 2:
                                    LoadProfileGraph();
                                    break;
                            }
                        }
                        break;
                    case "Профиль":
                        Profile profile = new Profile { AreaId = AreaRepository.GetAreaIDByProjectID(ProjectId), BreaksCount = 0 };
                        ProfileRepository.Add(profile);
                        AreaRepository.AreaProfileCount(ProjectId);
                        profileData.Add(profile);
                        break;
                    case "Координаты профиля":
                        if (oneBox.Text.Trim().IsNullOrEmpty() &&
                            twoBox.Text.Trim().IsNullOrEmpty() &&
                            threeBox.Text.Trim().IsNullOrEmpty())
                        {
                            MessageBox.Show("Пожалуйста заполните все поля!");
                        }
                        else
                        {
                            if (ProfileRepository.CheckProfileId(int.Parse(threeBox.Text.Trim())))
                            {
                                ProfileCoordinate coordinate = new ProfileCoordinate { ProfileId = int.Parse(threeBox.Text.Trim()), X = double.Parse(oneBox.Text.Trim().Replace('.', ',')), Y = double.Parse(twoBox.Text.Trim().Replace('.', ',')) };
                                ProfileRepository.AddCoordinate(coordinate);
                                ProfileRepository.ProfileBreakCount(int.Parse(threeBox.Text.Trim()));
                                profileCoordinates.Add(coordinate);
                                oneBox.Text = "";
                                twoBox.Text = "";
                                threeBox.Text = "";

                                switch (choise)
                                {
                                    case 2:
                                        LoadDataProfile();
                                        Data.ItemsSource = profileData;
                                        LoadProfileGraph();
                                        break;
                                    case 3:
                                        LoadChannelGraph();
                                        break;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Профиль с таким номером не найден");
                                threeBox.Text = "";
                            }
                        }
                        break;
                    case "Канал 1":
                        if (oneBox.Text.Trim().IsNullOrEmpty() &&
                            twoBox.Text.Trim().IsNullOrEmpty())
                        {
                            MessageBox.Show("Пожалуйста заполните все поля!");
                        }
                        else
                        {
                            if (ProfileRepository.CheckProfileCoordinatesId(int.Parse(twoBox.Text.Trim())))
                            {
                                MagnetChannel channel1 = new MagnetChannel { MeasurementResult = double.Parse(oneBox.Text.Trim().Replace('.', ',')), ProfileCoordinatesId = int.Parse(twoBox.Text.Trim()) };
                                ChannelsRepository.AddChannel(channel1);
                                channelData.Add(channel1);

                                oneBox.Text = "";
                                twoBox.Text = "";

                                if (choise == 3)
                                    LoadChannelGraph();
                            }
                            else
                            {
                                MessageBox.Show("Координаты профиля с таким номером не найдены!");
                                twoBox.Text = "";
                            }
                        }
                        break;
                    case "Полет":
                        if (oneBox.Text.Trim().IsNullOrEmpty() &&
                            twoBox.Text.Trim().IsNullOrEmpty() &&
                            threeBox.Text.Trim().IsNullOrEmpty() &&
                            fourBox.Text.Trim().IsNullOrEmpty() &&
                            fiveBox.Text.Trim().IsNullOrEmpty())
                        {
                            MessageBox.Show("Пожалуйста заполните все поля!");
                        }
                        else
                        {
                            Flight flight = new Flight { StartDateTime = DateTime.Parse(oneBox.Text.Trim()), EndDateTime = DateTime.Parse(twoBox.Text.Trim()), AltitudeAboveSea = double.Parse(threeBox.Text.Trim().Replace('.', ',')), AltitudeAboveGround = double.Parse(fourBox.Text.Trim().Replace('.', ',')), Speed = double.Parse(fiveBox.Text.Trim().Replace('.', ',')), ProjectId = ProjectId, OperatorId = null };
                            FlightRepository.Add(flight);
                            flightData.Add(flight);

                            oneBox.Text = "";
                            twoBox.Text = "";
                            threeBox.Text = "";
                            fourBox.Text = "";
                            fiveBox.Text = "";
                        }
                        break;
                    case "Спектрометер":
                        if (oneBox.Text.Trim().IsNullOrEmpty() &&
                            twoBox.Text.Trim().IsNullOrEmpty() &&
                            threeBox.Text.Trim().IsNullOrEmpty() &&
                            fourBox.Text.Trim().IsNullOrEmpty() &&
                            fiveBox.Text.Trim().IsNullOrEmpty())
                        {
                            MessageBox.Show("Пожалуйста заполните все поля!");
                        }
                        else
                        {
                            if (FlightRepository.CheckFlightId(int.Parse(fiveBox.Text.Trim())))
                            {
                                Spectrometer spectrometer = new Spectrometer { MeasurementTime = double.Parse(oneBox.Text.Trim().Replace('.', ',')), PulseCount = int.Parse(twoBox.Text.Trim()), TotalCount = int.Parse(threeBox.Text.Trim()), EnergyWindowsCount = int.Parse(fourBox.Text.Trim()), FlightId = int.Parse(fiveBox.Text.Trim()) };
                                SpectrometerRepository.Add(spectrometer);
                                spectrometerData.Add(spectrometer);

                                oneBox.Text = "";
                                twoBox.Text = "";
                                threeBox.Text = "";
                                fourBox.Text = "";
                                fiveBox.Text = "";
                            }
                            else
                            {
                                MessageBox.Show("Полет с таким номером не найден!");
                                fiveBox.Text = "";
                            }
                        }
                        break;
                    case "Метаданые":
                        if (oneBox.Text.Trim().IsNullOrEmpty() &&
                            twoBox.Text.Trim().IsNullOrEmpty() &&
                            threeBox.Text.Trim().IsNullOrEmpty())
                        {
                            MessageBox.Show("Пожалуйста заполните все поля!");
                        }
                        else
                        {
                            if (SpectrometerRepository.CheckSpectrometerId(int.Parse(threeBox.Text.Trim())))
                            {
                                Metadata metadata = new Metadata { EquipmentDescription = oneBox.Text.Trim(), Notes = twoBox.Text.Trim(), SpectrometerId = int.Parse(threeBox.Text.Trim()) };
                                MetadataRepository.Add(metadata);
                                metadataData.Add(metadata);

                                oneBox.Text = "";
                                twoBox.Text = "";
                                threeBox.Text = "";
                            }
                            else
                            {
                                MessageBox.Show("Спектрометер с таким номером не найден!");
                                threeBox.Text = "";
                            }
                        }
                        break;
                }
            }
            else
            {
                MessageBox.Show("Выберите какие данные вы хотите добавить!");
            }
        }

        private void AddCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectCombo.SelectedItem != null)
            {
                if (AddCombo.SelectedItem != null)
                {
                    switch (AddCombo.SelectedItem.ToString())
                    {
                        case "Площадь":
                            if (AreaRepository.GetDataOfArea(ProjectId).Count == 0)
                            {
                                oneLabel.Visibility = Visibility.Visible;
                                oneLabel.Content = "Геологическая информация";
                                oneBox.Visibility = Visibility.Visible;

                                twoLabel.Visibility = Visibility.Visible;
                                twoLabel.Content = "Площадь";
                                twoBox.Visibility = Visibility.Visible;

                                threeLabel.Visibility = Visibility.Hidden;
                                fourLabel.Visibility = Visibility.Hidden;
                                fiveBox.Visibility = Visibility.Hidden;

                                threeBox.Visibility = Visibility.Hidden;
                                fourBox.Visibility = Visibility.Hidden;
                                fiveLabel.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                MessageBox.Show("Площадь уже добавлена!");
                                AddCombo.SelectedItem = null;
                            }
                            break;
                        case "Координаты площади":
                            if (AreaRepository.GetDataOfArea(ProjectId).Count == 0)
                            {
                                MessageBox.Show("Сначала добавьте площадь!");
                                AddCombo.SelectedItem = null;
                            }
                            else
                            {
                                oneLabel.Visibility = Visibility.Visible;
                                oneLabel.Content = "X";
                                oneBox.Visibility = Visibility.Visible;

                                twoLabel.Visibility = Visibility.Visible;
                                twoLabel.Content = "Y";
                                twoBox.Visibility = Visibility.Visible;

                                threeLabel.Visibility = Visibility.Hidden;
                                fourLabel.Visibility = Visibility.Hidden;
                                fiveLabel.Visibility = Visibility.Hidden;

                                threeBox.Visibility = Visibility.Hidden;
                                fourBox.Visibility = Visibility.Hidden;
                                fiveBox.Visibility = Visibility.Hidden;
                            }
                            break;
                        case "Профиль":
                            if (AreaRepository.GetDataOfArea(ProjectId).Count == 0)
                            {
                                MessageBox.Show("Сначала добавьте площадь!");
                                AddCombo.SelectedItem = null;
                            }
                            else
                            {
                                oneLabel.Visibility = Visibility.Hidden;
                                oneBox.Visibility = Visibility.Hidden;

                                twoLabel.Visibility = Visibility.Hidden;
                                threeLabel.Visibility = Visibility.Hidden;
                                fourLabel.Visibility = Visibility.Hidden;
                                fiveLabel.Visibility = Visibility.Hidden;

                                twoBox.Visibility = Visibility.Hidden;
                                threeBox.Visibility = Visibility.Hidden;
                                fourBox.Visibility = Visibility.Hidden;
                                fiveBox.Visibility = Visibility.Hidden;
                            }
                            break;
                        case "Координаты профиля":
                            if (AreaRepository.GetDataOfArea(ProjectId).Count == 0)
                            {
                                MessageBox.Show("Сначала добавьте площадь!");
                                AddCombo.SelectedItem = null;
                            }
                            else
                            {
                                if (ProfileRepository.GetProfiles(AreaRepository.GetAreaIDByProjectID(ProjectId)).Count == 0)
                                {
                                    MessageBox.Show("Сначала добавьте профиль!");
                                    AddCombo.SelectedItem = null;
                                }
                                else
                                {
                                    oneLabel.Visibility = Visibility.Visible;
                                    oneLabel.Content = "X";
                                    oneBox.Visibility = Visibility.Visible;

                                    twoLabel.Visibility = Visibility.Visible;
                                    twoLabel.Content = "Y";
                                    twoBox.Visibility = Visibility.Visible;

                                    threeLabel.Visibility = Visibility.Visible;
                                    threeLabel.Content = "Номер профиля";
                                    threeBox.Visibility = Visibility.Visible;

                                    fourLabel.Visibility = Visibility.Hidden;
                                    fiveLabel.Visibility = Visibility.Hidden;

                                    fourBox.Visibility = Visibility.Hidden;
                                    fiveBox.Visibility = Visibility.Hidden;
                                }
                            }
                            break;
                        case "Канал":
                            if (AreaRepository.GetDataOfArea(ProjectId).Count == 0)
                            {
                                MessageBox.Show("Сначала добавьте площадь!");
                                AddCombo.SelectedItem = null;
                            }
                            else
                            {
                                if (ProfileRepository.GetProfiles(AreaRepository.GetAreaIDByProjectID(ProjectId)).Count == 0)
                                {
                                    MessageBox.Show("Сначала добавьте профиль!");
                                    AddCombo.SelectedItem = null;
                                }
                                else
                                {
                                    if (ProfileRepository.GetProfileCoordinates(AreaRepository.GetAreaIDByProjectID(ProjectId)).Count == 0)
                                    {
                                        MessageBox.Show("Сначала добавьте координаты профиля!");
                                        AddCombo.SelectedItem = null;
                                    }
                                    else
                                    {
                                        oneLabel.Visibility = Visibility.Visible;
                                        oneLabel.Content = "Результат измерений";
                                        oneBox.Visibility = Visibility.Visible;

                                        twoLabel.Visibility = Visibility.Visible;
                                        twoLabel.Content = "Номер координат профиля";
                                        twoBox.Visibility = Visibility.Visible;

                                        threeLabel.Visibility = Visibility.Hidden;
                                        fourLabel.Visibility = Visibility.Hidden;
                                        fiveLabel.Visibility = Visibility.Hidden;

                                        threeBox.Visibility = Visibility.Hidden;
                                        fourBox.Visibility = Visibility.Hidden;
                                        fiveBox.Visibility = Visibility.Hidden;
                                    }
                                }
                            }
                            break;
                        case "Полет":
                            oneLabel.Visibility = Visibility.Visible;
                            oneLabel.Content = "Дата и время начала полета";
                            oneBox.Visibility = Visibility.Visible;

                            twoLabel.Visibility = Visibility.Visible;
                            twoLabel.Content = "Дата и время конца полета";
                            twoBox.Visibility = Visibility.Visible;

                            threeLabel.Visibility = Visibility.Visible;
                            threeLabel.Content = "Высота над уровнем моря";
                            threeBox.Visibility = Visibility.Visible;

                            fourLabel.Visibility = Visibility.Visible;
                            fourLabel.Content = "Высота над уровнем земли";
                            fourBox.Visibility = Visibility.Visible;

                            fiveLabel.Visibility = Visibility.Visible;
                            fiveLabel.Content = "Скорость";
                            fiveBox.Visibility = Visibility.Visible;
                            break;
                        case "Спектрометер":
                            if (FlightRepository.GetDataOfFlight(ProjectId).Count == 0)
                            {
                                MessageBox.Show("Сначала добавьте данные о полет");
                                AddCombo.SelectedItem = null;
                            }
                            else
                            {
                                oneLabel.Visibility = Visibility.Visible;
                                oneLabel.Content = "Время измерений";
                                oneBox.Visibility = Visibility.Visible;

                                twoLabel.Visibility = Visibility.Visible;
                                twoLabel.Content = "Количество импульсов";
                                twoBox.Visibility = Visibility.Visible;

                                threeLabel.Visibility = Visibility.Visible;
                                threeLabel.Content = "Общий счет";
                                threeBox.Visibility = Visibility.Visible;

                                fourLabel.Visibility = Visibility.Visible;
                                fourLabel.Content = "Количество энергетических окон";
                                fourBox.Visibility = Visibility.Visible;

                                fiveLabel.Visibility = Visibility.Visible;
                                fiveLabel.Content = "Номер полета";
                                fiveBox.Visibility = Visibility.Visible;
                            }
                            break;
                        case "Метаданые":
                            if (FlightRepository.GetDataOfFlight(ProjectId).Count == 0)
                            {
                                MessageBox.Show("Сначала добавьте данные о полет");
                                AddCombo.SelectedItem = null;
                            }
                            else
                            {
                                if (SpectrometerRepository.GetDataOfSpectrometer(FlightRepository.GetFlightIDByProjectID(ProjectId)).Count == 0)
                                {
                                    MessageBox.Show("Сначала добавьте данные о спектрометре");
                                    AddCombo.SelectedItem = null;
                                }
                                else
                                {
                                    oneLabel.Visibility = Visibility.Visible;
                                    oneLabel.Content = "Описание оборудования";
                                    oneBox.Visibility = Visibility.Visible;

                                    twoLabel.Visibility = Visibility.Visible;
                                    twoLabel.Content = "Записи";
                                    twoBox.Visibility = Visibility.Visible;

                                    threeLabel.Visibility = Visibility.Visible;
                                    threeLabel.Content = "Номер спектрометра";
                                    threeBox.Visibility = Visibility.Visible;

                                    fourLabel.Visibility = Visibility.Hidden;
                                    fiveLabel.Visibility = Visibility.Hidden;

                                    fourBox.Visibility = Visibility.Hidden;
                                    fiveBox.Visibility = Visibility.Hidden;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    oneLabel.Visibility = Visibility.Hidden;
                    twoLabel.Visibility = Visibility.Hidden;
                    threeLabel.Visibility = Visibility.Hidden;
                    fourLabel.Visibility = Visibility.Hidden;
                    fiveLabel.Visibility = Visibility.Hidden;
                    oneBox.Visibility = Visibility.Hidden;
                    twoBox.Visibility = Visibility.Hidden;
                    threeBox.Visibility = Visibility.Hidden;
                    fourBox.Visibility = Visibility.Hidden;
                    fiveBox.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                MessageBox.Show("Выберите проект!");
                AddCombo.SelectedItem = null;
            }
        }

        private void AnalyticsBTN_Click(object sender, RoutedEventArgs e)
        {
            if(ProjectCombo.SelectedItem != null)
            {
                AnalyticsWindow analyticsWindow = new AnalyticsWindow(ProjectId);
                analyticsWindow.Show();
            }
            else
            {
                MessageBox.Show("Выберите проект");
            }
        }

        private void ExitBTN_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
            this.Close();
        }
    }
}
