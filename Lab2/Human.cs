using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Device.Location;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lab2
{
	public class Human : MapObject
	{
		private PointLatLng _point;

		public Human(string title, PointLatLng point) : base(title) => _point = point;

		public override double GetDistance(PointLatLng point)
		{
			GeoCoordinate c1 = new GeoCoordinate(_point.Lat, point.Lng);
			GeoCoordinate c2 = new GeoCoordinate(point.Lat, _point.Lng);
			return c1.GetDistanceTo(c2);
		}

		public override PointLatLng GetFocus() => _point;

		public override GMapMarker GetMarker()
		{
			double width = 40;
			double height = 40;
			var tt = new TranslateTransform(height / -2, -(width));
			GMapMarker marker = new GMapMarker(_point)
			{
				Shape = new Image
				{
					Width = width,
					Height = height,
					ToolTip = this.GetTitle(),
					Source = new BitmapImage(new Uri("pack://application:,,,/images/person.png")),
					RenderTransform = tt
				}
			};
			return marker;
		}
	}
}
