using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace Nawigacja
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        private void powMapa(object sender, RoutedEventArgs e)
        {
            mojaMapa.ZoomLevel += 1;
        }

        private void pomMapa(object sender, RoutedEventArgs e)
        {
            if (mojaMapa.ZoomLevel > 1)
            {
                mojaMapa.ZoomLevel -= 1;
            }
        }

        private void trybMapy(object sender, RoutedEventArgs e)
        {
            AppBarButton ab = sender as AppBarButton;
            if (mojaMapa.Style == MapStyle.AerialWithRoads)
            {
                mojaMapa.Style = MapStyle.Road;
                ab.Label = "satelita";
                FontIcon fIcon = new FontIcon();
                fIcon.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Auto");
                fIcon.Glyph = "\uE701"; // Ikona "S" (skala)
                ab.Icon = fIcon;
            }
            else
            {
                mojaMapa.Style = MapStyle.AerialWithRoads;
                ab.Label = "mapa";

                FontIcon fIcon = new FontIcon();

                fIcon.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Auto");
                fIcon.Glyph = "\uE8C4";
                ab.Icon = fIcon;
            }
        }

        private void przejdzNaKoordynay(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Koordynaty), mojaMapa);
        }




        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!object.ReferenceEquals(DaneGeograficzne.pktStartowy, null) && !object.ReferenceEquals(DaneGeograficzne.pktDocelowy, null))
            {

                Geopoint pktStartowy = new Geopoint(DaneGeograficzne.pktStartowy);
                Geopoint pktDocelowy = new Geopoint(DaneGeograficzne.pktDocelowy);

                MapIcon znacznikStart = new MapIcon();
                znacznikStart.Location = pktStartowy;
                znacznikStart.Title = "Tu jestem!";

                MapIcon znacznikDocelowy = new MapIcon();
                znacznikDocelowy.Location = pktDocelowy;
                znacznikDocelowy.Title = "Cel podróży";

                MapPolyline trasaLotem = new MapPolyline();
                trasaLotem.StrokeColor = Windows.UI.Colors.Black;
                trasaLotem.StrokeThickness = 3;
                trasaLotem.StrokeDashed = true;
                trasaLotem.Path = new Geopath(new List<BasicGeoposition> {
            DaneGeograficzne.pktStartowy,
            DaneGeograficzne.pktDocelowy
        });

                mojaMapa.MapElements.Add(znacznikStart);
                mojaMapa.MapElements.Add(znacznikDocelowy);
                mojaMapa.MapElements.Add(trasaLotem);

                await mojaMapa.TrySetViewAsync(pktStartowy, 8);
            }
            else
            {
                return;
            }

        }

        private async Task Trasa()
        {
            if (!object.ReferenceEquals(DaneGeograficzne.pktStartowy, null) && !object.ReferenceEquals(DaneGeograficzne.pktDocelowy, null))
            {
                Geopoint pktStartowy = new Geopoint(DaneGeograficzne.pktStartowy);
                Geopoint pktDocelowy = new Geopoint(DaneGeograficzne.pktDocelowy);

                MapRouteFinderResult trasaResult = await MapRouteFinder.GetDrivingRouteAsync(pktStartowy, pktDocelowy);


                if (trasaResult.Status == MapRouteFinderStatus.Success)
                {
                    MapRoute trasa = trasaResult.Route;
                    double dlugoscTrasy = trasa.LengthInMeters;
                    TimeSpan czasPodrozy = trasa.EstimatedDuration;

                    string jednostkiDl;
                    double distance;
                    if (dlugoscTrasy < 1000)
                    {
                        jednostkiDl = "cm";
                        distance = dlugoscTrasy * 100;
                    }
                    else if (dlugoscTrasy < 10000)
                    {
                        jednostkiDl = "m";
                        distance = dlugoscTrasy;
                    }
                    else
                    {
                        jednostkiDl = "km";
                        distance = dlugoscTrasy / 1000;
                    }

                    string info = $"Odległość między punktami: {distance:F2} {jednostkiDl}\nCzas podróży: {czasPodrozy}";
                    MessageDialog dialog = new MessageDialog(info);
                    await dialog.ShowAsync();
                }
                else
                {
                    string infoBlad = "Nie udało się znaleźć trasy.";
                    MessageDialog bladDialog = new MessageDialog(infoBlad);
                    await bladDialog.ShowAsync();
                }
            }
            else
            {
                return;
            }

        }

        private async Task PokazTraseNaMapie(MapRouteFinderResult trasaResult)
        {

            if (trasaResult.Status == MapRouteFinderStatus.Success)
            {
                MapRoute trasa = trasaResult.Route;

                MapRouteView viewOfRoute = new MapRouteView(trasa);

                viewOfRoute.RouteColor = Windows.UI.Colors.LightPink;
                viewOfRoute.OutlineColor = Windows.UI.Colors.DeepPink;

                mojaMapa.Routes.Add(viewOfRoute);
            }
            else
            {
                string blad = "Nie udało się znaleźć trasy.";
                MessageDialog bladDialog = new MessageDialog(blad);
                await bladDialog.ShowAsync();
            }

        }




        private async void btnPokazTrase_Click(object sender, RoutedEventArgs e)
        {
            if (!object.ReferenceEquals(DaneGeograficzne.pktStartowy, null) && !object.ReferenceEquals(DaneGeograficzne.pktDocelowy, null))
            {

                Geopoint pktStartowy = new Geopoint(DaneGeograficzne.pktStartowy);
                Geopoint pktDocelowy = new Geopoint(DaneGeograficzne.pktDocelowy);
                MapRouteFinderResult trasaResult = await MapRouteFinder.GetDrivingRouteAsync(pktStartowy, pktDocelowy);

                await PokazTraseNaMapie(trasaResult);
            }
            else
            {
                return;
            }
            await Trasa();
        }
    }
}
