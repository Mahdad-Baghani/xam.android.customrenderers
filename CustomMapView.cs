using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Map = Xamarin.Forms.GoogleMaps.Map;
namespace Tag.Core.Controls
{
    public class CustomMapView : Map
    {
        public CustomMapView()
        {
            IsShowingUser = true;
        }
    }
}
