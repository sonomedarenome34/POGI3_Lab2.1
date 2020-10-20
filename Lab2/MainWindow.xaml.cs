using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;

namespace Lab2
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		List<PointLatLng> _lastMapObjectPoints = new List<PointLatLng>();
		GMapMarker _lastMarker = null;

		List<MapObject> _mapObjects = new List<MapObject>();

		enum MapObjectType
		{
			None = -1,
			Human,
			Car,
			Location,
			Route,
			Area
		}

		public MainWindow()
		{
			InitializeComponent();
			MapObjectName.Text = $"Object {_mapObjects.Count + 1}";
		}

		private void MapLoaded(object sender, RoutedEventArgs e)
		{
			GMaps.Instance.Mode = AccessMode.ServerAndCache;
			Map.MapProvider = YandexMapProvider.Instance;
			Map.MinZoom = 2;
			Map.MaxZoom = 17;
			Map.Zoom = 15;
			Map.Position = new PointLatLng(55.012823, 82.950359);
			Map.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
			Map.CanDragMap = true;
			Map.DragButton = MouseButton.Left;
		}

		private void AddMarker(string title, PointLatLng point)
		{
			MapObject mapObject;
			switch ((MapObjectType)MapObjectSelector.SelectedIndex)
			{
				case MapObjectType.None:
					MessageBox.Show("Choose a map object type.");
					return;
				case MapObjectType.Human:
					mapObject = new Human(title, point);
					break;
				case MapObjectType.Car:
					mapObject = new Car(title, point);
					break;
				case MapObjectType.Location:
					mapObject = new Location(title, point);
					break;
				case MapObjectType.Route:
					_lastMapObjectPoints.Add(point);
					if (_lastMapObjectPoints.Count < 2) return;
					mapObject = new Route(title, _lastMapObjectPoints);
					break;
				case MapObjectType.Area:
					_lastMapObjectPoints.Add(point);
					if (_lastMapObjectPoints.Count < 3) return;
					mapObject = new Area(title, _lastMapObjectPoints);
					break;
				default:
					MessageBox.Show("Unknown map object.");
					return;
			}
			if (_lastMarker != null && _mapObjects.Count != 0 && (mapObject is Route || mapObject is Area))
			{
				Map.Markers.Remove(_lastMarker);
				_mapObjects.RemoveAt(_mapObjects.Count - 1);
			}
			_lastMarker = mapObject.GetMarker();
			Map.Markers.Add(_lastMarker);
			_mapObjects.Add(mapObject);
			MapObjectName.Text = $"Object {_mapObjects.Count + 1}";
		}

		private void Map_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var point = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y);
			if ((bool) CreateMode.IsChecked)
				AddMarker(MapObjectName.Text, point);
			if ((bool) SearchMode.IsChecked)
			{
				NearbyObjects.Items.Clear();
				List<MapObject> sortedMapObjects = _mapObjects.OrderBy(o => o.GetDistance(point)).ToList();
				for (int i = 0; i < sortedMapObjects.Count; i++)
				{
					KeyValuePair<MapObject, string> keyValuePair = new KeyValuePair<MapObject, string>(sortedMapObjects[i], sortedMapObjects[i].GetTitle());
					NearbyObjects.Items.Add(keyValuePair);
				}
			};
		}

		private void MapObjectSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_lastMapObjectPoints.Clear();
			_lastMarker = null;
		}

		private void ClearMap_Click(object sender, RoutedEventArgs e)
		{
			Map.Markers.Clear();
			_lastMapObjectPoints.Clear();
			_mapObjects.Clear();
			NearbyObjects.Items.Clear();
			MapObjectName.Text = $"Object {_mapObjects.Count + 1}";
		}

		private void NearbyObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (((ListBox) sender).SelectedIndex > -1)
			{
				var point = ((KeyValuePair<MapObject, string>) ((ListBox) sender).SelectedItem).Key.GetFocus();
				Map.Position = point;
			}
		}

		private void SearchNearbyByName_Click(object sender, RoutedEventArgs e)
		{
			NearbyObjects.Items.Clear();
			PointLatLng point = PointLatLng.Empty;
			MapObject @object = null;
			foreach (var mapObject in _mapObjects)
			{
				if (mapObject.GetTitle() == MapObjectName.Text)
				{
					point = mapObject.GetFocus();
					@object = mapObject;
					break;
				}
			}
			if (point == PointLatLng.Empty)
			{
				MessageBox.Show($"There is no object with the name \"{MapObjectName.Text}\".");
				return;
			}
			List<MapObject> sortedMapObjects = _mapObjects.OrderBy(o => o.GetDistance(point)).ToList();
			sortedMapObjects.Remove(@object);
			for (int i = 0; i < sortedMapObjects.Count; i++)
			{
				KeyValuePair<MapObject, string> keyValuePair = new KeyValuePair<MapObject, string>(sortedMapObjects[i], sortedMapObjects[i].GetTitle());
				NearbyObjects.Items.Add(keyValuePair);
			}
		}
	}
}