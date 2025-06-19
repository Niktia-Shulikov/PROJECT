using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using AGSS.Entities;
using AGSS.Repositories;
using Microsoft.Data.SqlClient;
using OxyPlot.Series;
using OxyPlot;

namespace AGSS
{
    /// <summary>
    /// Логика взаимодействия для CustomerWindow.xaml
    /// </summary>
    public partial class CustomerWindow : Window
    {
        int ID;
        int ProjectId;

        private ObservableCollection<Area> areaData = new ObservableCollection<Area>();
        private ObservableCollection<AreaCoordinate> areaCoordinates = new ObservableCollection<AreaCoordinate>();

        private ObservableCollection<object> profileData = new ObservableCollection<object>();
        private ObservableCollection<Profile> profilDataForGraph = new ObservableCollection<Profile>();
        private ObservableCollection<ProfileCoordinate> profileCoordinates = new ObservableCollection<ProfileCoordinate>();

        private ObservableCollection<object> channelData = new ObservableCollection<object>();

        private ObservableCollection<MagnetChannel> channelDataForGraph = new ObservableCollection<MagnetChannel>();

        private ObservableCollection<Flight> flightData = new ObservableCollection<Flight>();
        private ObservableCollection<Spectrometer> spectrometerData = new ObservableCollection<Spectrometer>();
        private ObservableCollection<Metadata> metadataData = new ObservableCollection<Metadata>();

        public CustomerWindow(int id)
        {
            InitializeComponent();
            ID = id;
            LoadProjects();
        }
        
        private void LoadProjects()
        {
            var names = ProjectRepository.GetProjectNames(ID);
            if (names != null)
            {
                ProjectCombo.ItemsSource = names;
            }
            else
                MessageBox.Show("Нет данных о проектах");
        }
       
        private void ExitBTN_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
            this.Close();
        }

        private void LoadDataArea()
        {
            areaData = new ObservableCollection<Area>(AreaRepository.GetDataOfArea(ProjectId));
        }

        private void LoadDataProfile()
        {
            profileData = new ObservableCollection<object>(ProfileRepository.GetDataOfProfile(
                        AreaRepository.GetAreaIDByProjectID(ProjectId)));
        }

        private void LoadDataChannel()
        {
            channelData = new ObservableCollection<object>(ChannelsRepository.GetDataOfChannel(
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

        private void LoadDataAreaCoordinates()
        {
            areaCoordinates = new ObservableCollection<AreaCoordinate>(AreaRepository.GetAreaCoordinates(AreaRepository.GetAreaIDByProjectID(ProjectId)));
        }

        private void LoadDataProfileCoordinates()
        {
            profileCoordinates = new ObservableCollection<ProfileCoordinate>(ProfileRepository.GetProfileCoordinates(AreaRepository.GetAreaIDByProjectID(ProjectId)));
        }

        private void LoadDataProfileForGraph()
        {
            profilDataForGraph = new ObservableCollection<Profile>(ProfileRepository.GetProfiles(AreaRepository.GetAreaIDByProjectID(ProjectId)));
        }

        private void LoadDataChannelForGraph()
        {
            channelDataForGraph = new ObservableCollection<MagnetChannel>(ChannelsRepository.GetChannels(
                                AreaRepository.GetAreaIDByProjectID(ProjectId)));
        }
        private void LoadAreaGraph()
        {
            LoadDataAreaCoordinates();
            var plotModel = new PlotModel { Title = "График координат площади" };

            plotModel.Series.Clear();

            var series = new LineSeries { Title = "Линия", MarkerType = MarkerType.Circle };

            foreach (var c in areaCoordinates)
            {
                series.Points.Add(new DataPoint((double)c.X, (double)c.Y));
            }
            if (areaCoordinates.Count > 0)
            {
                series.Points.Add(new DataPoint((double)areaCoordinates[0].X, (double)areaCoordinates[0].Y));
            }

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
            LoadDataProfileForGraph();
            LoadDataProfileCoordinates();

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
                    foreach (var p in profilDataForGraph)
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

            LoadDataProfileForGraph();
            LoadDataProfileCoordinates();
            LoadDataChannelForGraph();

            if (channelDataForGraph.Count > 0 && profileCoordinates.Count > 0 && profilDataForGraph.Count > 0)
            {
                foreach (var p in profilDataForGraph)
                {
                    LineSeries line = new LineSeries { MarkerType = MarkerType.Circle, Title = $"Профиль {p.ProfileId}" };
                    int i = 1;
                    foreach (var coord in profileCoordinates)
                    {
                        if (p.ProfileId == coord.ProfileId)
                        {
                            foreach (var c in channelDataForGraph)
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
                    LoadDataArea();
                    Data.ItemsSource = areaData;
                    LoadAreaGraph();
                    break;
                case 2:
                    ProfileColumns();
                    LoadDataProfile();
                    Data.ItemsSource = profileData;
                    LoadProfileGraph();
                    break;
                case 3:
                    ChannelsColumns();
                    LoadDataChannel();
                    Data.ItemsSource = channelData;
                    LoadChannelGraph();
                    break;
                case 4:
                    FlightColumns();
                    LoadDataFlight();
                    Data.ItemsSource = flightData;
                    break;
                case 5:
                    SpectrometerColumns();
                    LoadDataSpectrometer();
                    Data.ItemsSource = spectrometerData;
                    break;
                case 6:
                    MetadataColumns();
                    LoadDataMetadata();
                    Data.ItemsSource = metadataData;
                    break;
            }
        }

        private void AreaColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "Геологическая информация", DisplayMemberBinding = new Binding("GeologicalInfo") },
                    new GridViewColumn { Header = "Область", DisplayMemberBinding = new Binding("Area1") },
                    new GridViewColumn { Header = "Количество профилей", DisplayMemberBinding = new Binding("ProfileCount") }
                }
            };
        }

        private void ProfileColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "Номер Профиля", DisplayMemberBinding = new Binding("ProfileId") },
                    new GridViewColumn { Header = "X", DisplayMemberBinding = new Binding("X") },
                    new GridViewColumn { Header = "Y", DisplayMemberBinding = new Binding("Y") }
                }
            };
        }

        private void ChannelsColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "Номер Профиля", DisplayMemberBinding = new Binding("ProfileId") },
                    new GridViewColumn { Header = "X", DisplayMemberBinding = new Binding("X") },
                    new GridViewColumn { Header = "Y", DisplayMemberBinding = new Binding("Y") },
                    new GridViewColumn { Header = "Значение", DisplayMemberBinding = new Binding("MeasurementResult")}
                }
            };
        }

        private void FlightColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "Дата начала полета", DisplayMemberBinding = new Binding("StartDateTime") },
                    new GridViewColumn { Header = "Дата конца полета", DisplayMemberBinding = new Binding("EndDateTime") },
                    new GridViewColumn { Header = "Высота над уровнем моря", DisplayMemberBinding = new Binding("AltitudeAboveSea") },
                    new GridViewColumn { Header = "Высота над уровнем земли", DisplayMemberBinding = new Binding("AltitudeAboveGround")},
                    new GridViewColumn { Header = "Средняя скорость", DisplayMemberBinding = new Binding("Speed")}
                }
            };
        }

        private void SpectrometerColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "Дата начала полета", DisplayMemberBinding = new Binding("MeasurementTime") },
                    new GridViewColumn { Header = "Дата конца полета", DisplayMemberBinding = new Binding("PulseCount") },
                    new GridViewColumn { Header = "Высота над уровнем моря", DisplayMemberBinding = new Binding("TotalCount") },
                    new GridViewColumn { Header = "Высота над уровнем земли", DisplayMemberBinding = new Binding("EnergyWindowsCount")}
                }
            };
        }

        private void MetadataColumns()
        {
            Data.View = new GridView
            {
                Columns =
                {
                    new GridViewColumn { Header = "Дата начала полета", DisplayMemberBinding = new Binding("EquipmentDescription") },
                    new GridViewColumn { Header = "Дата конца полета", DisplayMemberBinding = new Binding("Notes") }
                }
            };
        }

        private void AnalysticBTN_Click(object sender, RoutedEventArgs e)
        {
            if(ProjectCombo.SelectedItem != null)
            {
                AnalyticsWindow analyticsWindow = new AnalyticsWindow(ProjectId);
                analyticsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите проект!");
            }
        }

        private void ProjectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ProjectCombo.SelectedItem != null)
            {
                ProjectId = ProjectRepository.GetIDByProjectName(ProjectCombo.SelectedItem.ToString());
            }
            else
            {
                MessageBox.Show("Выберите проект!");
            }
        }
    }
}
