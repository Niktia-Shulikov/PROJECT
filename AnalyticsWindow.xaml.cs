using System;
using System.Collections.Generic;
using System.Data;
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
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;

namespace AGSS
{
    /// <summary>
    /// Логика взаимодействия для AnalystWindow.xaml
    /// </summary>
    public partial class AnalyticsWindow : Window
    {
        int ProjectId;
        public AnalyticsWindow(int Id)
        {
            InitializeComponent();
            ProjectId = Id;
            LoadData();
        }  

        private void LoadData()
        {
            double AverAreaChannel1;
            double AverAreaChannel2;
            double AverAreaChannel3;

            List<DateTime?> StartTime;
            List<DateTime?> EndTime;
            double AverSpeed;
            double AverHeight;

            using (var context = new GravitySurveyOnDeleteNoAction())
            {
                var AreaId = context.Areas.FirstOrDefault(a => a.ProjectId == ProjectId).AreaId;
                var ProfileIds = context.Profiles.Where(p => p.AreaId == AreaId).Select(p => p.ProfileId).ToList();
                var ProfileCoordinatesId = context.ProfileCoordinates.Where(p => ProfileIds.Contains(p.ProfileId ?? -1)).Select(p => p.ProfileCoordinatesId).ToList();

                AverSpeed = (double)context.Flights.Where(f => f.ProjectId == ProjectId).Select(f => f.Speed).ToList().Average();
                AverHeight = (double)context.Flights.Where(f => f.ProjectId == ProjectId).Select(f => f.AltitudeAboveSea).ToList().Average();
                StartTime = context.Flights.Where(f => f.ProjectId == ProjectId).Select(f => f.StartDateTime).ToList();
                EndTime = context.Flights.Where(f => f.ProjectId == ProjectId).Select(f => f.EndDateTime).ToList();

                AverAreaChannel1 = (double)context.MagnetChannels.Where(c => ProfileCoordinatesId.Contains(c.ProfileCoordinatesId ?? -1)).Select(c => c.MeasurementResult).ToList().Average();

                ObservableCollection <string> Data = new ObservableCollection<string>();
                foreach (int profile in ProfileIds)
                {
                    ProfileCoordinatesId = context.ProfileCoordinates.Where(p => p.ProfileId == profile).Select(p => p.ProfileCoordinatesId).ToList();
                    var AverChannel1 = context.MagnetChannels.Where(c => ProfileCoordinatesId.Contains(c.ProfileCoordinatesId ?? -1)).Select(c => c.MeasurementResult).ToList().Average();

                    Data.Add($"Среднее значение канала на профиле №{profile}: {AverChannel1}");
                }
                DataGrid1.ItemsSource = Data;
            }

            AreaChannel1.Content += AverAreaChannel1.ToString("F2");

            Speed.Content += AverSpeed.ToString("F2");
            Height.Content += AverHeight.ToString("F2");
            Time.Content += GetAverageFlightDuration(StartTime, EndTime).ToString();
        }

        public static TimeSpan GetAverageFlightDuration(List<DateTime?> starts, List<DateTime?> ends)
        {
            var durations = new List<TimeSpan>();

            for (int i = 0; i < starts.Count; i++)
            {
                if (starts[i].HasValue && ends[i].HasValue)
                {
                    durations.Add(ends[i].Value - starts[i].Value);
                }
            }

            if (durations.Count == 0)
                return TimeSpan.Zero;

            long averageTicks = (long)durations.Average(ts => ts.Ticks);
            return TimeSpan.FromTicks(averageTicks);
        }

    }
}
