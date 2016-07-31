using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokemonGo.RocketAPI.Console
{
    public partial class Tracker : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        private Settings _settings;
        private Logic.Logic _logic;

        public Tracker()
        {
            InitializeComponent();
            _settings = new Settings();
            SetUpMap(_settings);
            Logic.Utils.Statistics.HasUI = true;
            Logic.Utils.Statistics.window = this;
            AttachConsole(-1);
            StartProgram(_settings);
        }

        private void SetUpMap(Settings p_Settings)
        {
            // Initialize map:
            //use google provider
            gMapControl1.MapProvider = GoogleMapProvider.Instance;
            //get tiles from server only
            gMapControl1.Manager.Mode = AccessMode.ServerOnly;
            //not use proxy
            GMapProvider.WebProxy = null;
            //center map 


            string lat = p_Settings.DefaultLatitude.ToString();
            string longit = p_Settings.DefaultLongitude.ToString();
            lat.Replace(',', '.');
            longit.Replace(',', '.');
            var start = new PointLatLng(Convert.ToDouble(lat), Convert.ToDouble(longit));
            gMapControl1.Position = start;


            //zoom min/max; default both = 2
            gMapControl1.DragButton = MouseButtons.Left;
            gMapControl1.MarkersEnabled = true;

            gMapControl1.CenterPen = new Pen(Color.Transparent, 2);
            gMapControl1.MinZoom = trackBar1.Maximum = 1;
            gMapControl1.MaxZoom = trackBar1.Maximum = 20;
            trackBar1.Value = 15;

            //set zoom
            gMapControl1.Zoom = trackBar1.Value;

            GMapOverlay userOverlay = new GMapOverlay("user");
            Bitmap userBitmap = new Bitmap(12, 12);
            
                using (Graphics g = Graphics.FromImage(userBitmap))
                {
                    using (Brush b = new SolidBrush(Color.Goldenrod))
                    {
                        g.FillEllipse(b, 0, 0, 12, 12);
                    }
                }


            GMarkerGoogle user = new GMarkerGoogle(start, userBitmap);
            userOverlay.Markers.Add(user);

            GMapOverlay startOverlay = new GMapOverlay("start");
            GMarkerGoogle startMarker = new GMarkerGoogle(start,
            GMarkerGoogleType.gray_small);
            startOverlay.Markers.Add(startMarker);

            GMapOverlay mapPointOverlay = new GMapOverlay("objects");
            GMapOverlay pokemonOverlay = new GMapOverlay("pokemon");

            GMapOverlay areaOverlay = new GMapOverlay("area");
            //p_Settings.MaxTravelDistanceInMeters
            var areaMarker = Marker.CreateCircle(start, p_Settings.MaxTravelDistanceInMeters, 32);
            areaOverlay.Polygons.Add(areaMarker);

            gMapControl1.Overlays.Add(areaOverlay);
            gMapControl1.Overlays.Add(startOverlay);
            gMapControl1.Overlays.Add(mapPointOverlay);
            gMapControl1.Overlays.Add(pokemonOverlay);
            gMapControl1.Overlays.Add(userOverlay);

        }

        private void StartProgram(Settings p_Settings)
        {
            Logger.SetLogger(new FormLogger(richTextBox1, LogLevel.Info));

            Task.Run(() =>
            {
                try
                {
                    _logic = new Logic.Logic(p_Settings, gMapControl1, summaryPanel);
                    _logic.Execute().Wait();
                }
                catch (PtcOfflineException)
                {
                    Logger.Write("PTC Servers are probably down OR your credentials are wrong. Try google",
                        LogLevel.Error);
                    Logger.Write("Trying again in 20 seconds...");
                    Thread.Sleep(20000);
                    new Logic.Logic(new Settings(), gMapControl1, summaryPanel).Execute().Wait();
                }
                catch (AccountNotVerifiedException)
                {
                    Logger.Write("Account not verified. - Exiting");
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Logger.Write($"Unhandled exception: {ex}", LogLevel.Error);
                    new Logic.Logic(new Settings(), gMapControl1, summaryPanel).Execute().Wait();
                }
            });
            System.Console.ReadLine();

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            gMapControl1.Zoom = trackBar1.Value;
        }

        private async void gMapControl1_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            var result = MessageBox.Show("Do you want to move directly to this marker?", "Change Movement", MessageBoxButtons.YesNo);

            if(result != DialogResult.Yes)
            {
                return;
            }

            var coord = new System.Device.Location.GeoCoordinate(item.Position.Lat, item.Position.Lng);
            Logger.Write($"Walking to marker");

            try
            {
                await _logic._navigation.HumanLikeWalking(coord, _settings.WalkingSpeedInKilometerPerHour, null, true);
            }
            catch (Exception ex)
            {

               
            }
            
        }
    }

}
