using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Nawigacja
{
    public sealed partial class Koordynaty : Page
    {
        private MapControl mojaMapa;


        public Koordynaty()
        {
            this.InitializeComponent();


            mojaMapa = new MapControl();

            GdzieJaNaMapie();

        }


        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }


        private void btnWroc_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
        private async void GdzieJaNaMapie()
        {

            try
            {

                Geolocator mójGPS = new Geolocator { DesiredAccuracyInMeters = 10 };
                Geoposition mojeZGPS = await mójGPS.GetGeopositionAsync();

                double szerokosc = mojeZGPS.Coordinate.Point.Position.Latitude;
                //double szerokosc = 53.1152756479731;
                double dlugosc = mojeZGPS.Coordinate.Point.Position.Longitude;
                //double dlugosc = 17.9611684300166;

                tbGPS.Text = $"Szerokość: {szerokosc}, Długość: {dlugosc}";

                DaneGeograficzne.pktStartowy = new BasicGeoposition { Latitude = szerokosc, Longitude = dlugosc };
            }
            catch (Exception ex)
            {
                tbGPS.Text = "Nie można poprawnie odczytać GPS: " + ex.Message;
            }

        }
        private async Task<Geopoint> GeocodeAddressAsync(string address)
        {

            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(address, null, 1);
            if (result.Status == MapLocationFinderStatus.Success && result.Locations.Count > 0)
            {
                return result.Locations[0].Point;
            }
            else
            {
                return null;
            }
        }
        private async void txAdres_TextChanged(object sender, TextChangedEventArgs e)
        {
            string adres = txAdres.Text;
            if (txAdres.Text != " ")
            {
                Geopoint punktDocelowy = await GeocodeAddressAsync(adres);
                if (punktDocelowy != null)
                {
                    mojaMapa.MapElements.Clear();
                    MapIcon znacznikDocelowy = new MapIcon();

                    znacznikDocelowy.Location = punktDocelowy;
                    znacznikDocelowy.Title = "Cel podróży";

                    mojaMapa.MapElements.Add(znacznikDocelowy);
                    await mojaMapa.TrySetViewAsync(punktDocelowy, 10);

                    tbDlg.Text = $"Długość geograficzna: {punktDocelowy.Position.Longitude}";


                    tbSzer.Text = $"Szerokość geograficzna: {punktDocelowy.Position.Latitude}";
                }
            }
        }

        private async void btnSzukaj_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private async void btnPotwierdzenie_Click(object sender, RoutedEventArgs e)
        {

            string adres = txAdres.Text;

            if (txAdres.Text != " ")
            {
                Geopoint punktDocelowy = await GeocodeAddressAsync(adres); 

                if (punktDocelowy != null)
                {
                    mojaMapa.MapElements.Clear();
                    MapIcon znacznikDocelowy = new MapIcon();

                    znacznikDocelowy.Location = punktDocelowy;
                    znacznikDocelowy.Title = "Cel podróży";

                    mojaMapa.MapElements.Add(znacznikDocelowy);
                    await mojaMapa.TrySetViewAsync(punktDocelowy, 10); 

                    DaneGeograficzne.pktDocelowy = punktDocelowy.Position;

                    var dialog = new MessageDialog("Adres został znaleziony i zaznaczony na mapie.");
                    await dialog.ShowAsync();
                }
                else
                {
                    var dialog = new MessageDialog("Nie udało się znaleźć współrzędnych dla podanego adresu.");
                    await dialog.ShowAsync();
                }
            }

        }

        private async void btnInformacje_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Iwona Figas, Informatyka rok 2, studia niestacjonarne. \n\n Program ma za zadanie wskazać aktualne położenie i wybrany cel podróży. \n Ma także pokazać przejazd drogami i obliczyć jak daleko jesteśmy od celu");
            await dialog.ShowAsync();
        }

        private void btnPotwierdzenie_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                btnPotwierdzenie_Click(sender, e);
            }
        }

        private void txAdres_KeyDown(object sender, KeyRoutedEventArgs e)
        {

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                btnPotwierdzenie_Click(sender, e);
            }
        }
    }
}
