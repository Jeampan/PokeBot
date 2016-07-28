using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace PokemonGo.RocketAPI.Helpers
{
    public class Marker
    {

        public static GMapPolygon CreateCircle(PointLatLng point, double radius, int segments)
        {

            List<PointLatLng> gpollist = new List<PointLatLng>();

            double seg = Math.PI * 2 / segments;

            for (int i = 0; i < segments; i++)
            {
                double theta = seg * i;
                double a = point.Lat + Math.Cos(theta) * radius * 0.0000089889;
                double b = point.Lng + Math.Sin(theta) * radius * 0.00001424549;

                PointLatLng gpoi = new PointLatLng(a, b);

                gpollist.Add(gpoi);
            }
            GMapPolygon gpol = new GMapPolygon(gpollist, "pol");

            gpol.Stroke = new Pen(Color.Purple, 1);
            gpol.Fill = new SolidBrush(Color.FromArgb(20, Color.Purple));

            return gpol;
        }
    }
}