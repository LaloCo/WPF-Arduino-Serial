using Microsoft.WindowsAzure.MobileServices;
using SerialDemo.Model;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SerialDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MobileServiceClient MobileService = new MobileServiceClient(
            "https://waterio.azure-mobile.net/",
            "bSrdodNMQQJgOUmhXZAbxiaLHsMrma35"
        );
        Clima climaActual = new Clima();
        SerialPort puertoSerial = new SerialPort();
        DispatcherTimer temporizador = new DispatcherTimer();

        static string nombreEventHub = "waterio";
        static string cadenaConexion = "Endpoint=sb://waterioeh.servicebus.windows.net/;SharedAccessKeyName=READ;SharedAccessKey=6AIUtZUjNw4fnr1Ksif82ca6+Hl+ajUVjFRdMpjl98c=";

        static string nombreEventHubDetalles = "wateriosecondary";
        static string cadenaConexionDetalles = "Endpoint=sb://wateriosecondatyeh.servicebus.windows.net/;SharedAccessKeyName=Read;SharedAccessKey=43yd3uGBZjsAkeCqXYmTae8VFonh7g642SJcFNQVshY=";

        public MainWindow()
        {
            InitializeComponent();

            //x Dummies();

            RegistrarComando();
        }

        private async void Dummies()
        {
            var house = (await MobileService.GetTable<House>().ToListAsync()).First();
            /*for(int i = 0; i <= 9; i++)
            {
                Device newHouse = new Device() {Name = $"Device", Count = i, HouseId = house.Id };

                await MobileService.GetTable<Device>().InsertAsync(newHouse);
            }*/

            for (int i = 0; i <= 1; i++)
            {
                Device newHouse = new Device() { HouseId = house.Id, Name = $"Device1", Count = i * 123, Tiempo = DateTime.UtcNow.ToString(), EventEnqueuedUtcTime = DateTime.UtcNow, EventProcessedUtcTime = DateTime.UtcNow, PartitionId = i };

                await MobileService.GetTable<Device>().InsertAsync(newHouse);
            }
        }

        private void RegistrarComando()
        {
            try
            {
                puertoSerial.BaudRate = 9600;
                puertoSerial.PortName = "COM3";
                puertoSerial.Open();

                temporizador.Interval = TimeSpan.FromSeconds(1);
                temporizador.Tick += Temporizador_Tick;
                temporizador.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Temporizador_Tick(object sender, EventArgs e)
        {
            string datosLeidos = puertoSerial.ReadLine().Trim();
            int indexMililitrosHouse = datosLeidos.IndexOf('U');
            int indexMililitrosDevice = datosLeidos.IndexOf('D');

            int lenght = indexMililitrosDevice - indexMililitrosHouse;
            int lenght2 = datosLeidos.Length - indexMililitrosDevice;

            double mililitrosHouse = 0;
            double mililitrosDevice = 0;

            try
            {
                if (indexMililitrosHouse >= 0)
                {
                    var substring = datosLeidos.Substring(indexMililitrosHouse + 1, lenght - 1).Trim();
                    mililitrosHouse = double.Parse(substring);
                }
                if (indexMililitrosDevice >= 0)
                {
                    var substring = datosLeidos.Substring(indexMililitrosDevice + 1, lenght2 - 1).Trim();
                    mililitrosDevice = double.Parse(substring);
                }

                // TODO: Insertar al Event Hub
                InsertarDatos(mililitrosHouse, mililitrosDevice);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            /*string datosLeidos = puertoSerial.ReadLine().Trim();

            int indexHumidity = datosLeidos.IndexOf('h');
            int indexCelsius = datosLeidos.IndexOf('c');
            int indexFahrenheit = datosLeidos.IndexOf('f');
            int indexKelvin = datosLeidos.IndexOf('k');
            int indexDewPoint = datosLeidos.IndexOf('d');
            int indexDewPointFast = datosLeidos.IndexOf('w');

            try
            {
                if (indexHumidity >= 0)
                {
                    var substring = datosLeidos.Substring(indexHumidity + 1, 15).Trim();
                    float humedad = float.Parse(substring);
                    climaActual.Humedad = humedad;
                }
                if (indexCelsius >= 0)
                {
                    var substring = datosLeidos.Substring(indexCelsius + 1, 15).Trim();
                    float celsius = float.Parse(substring);
                    climaActual.TemperaturaCelsius = celsius;
                }
                if (indexFahrenheit >= 0)
                {
                    var substring = datosLeidos.Substring(indexFahrenheit + 1, 15).Trim();
                    float fahrenheit = float.Parse(substring);
                    climaActual.TemperaturaFahrenheit = fahrenheit;
                }
                if (indexKelvin >= 0)
                {
                    var substring = datosLeidos.Substring(indexKelvin + 1, 15).Trim();
                    float kelvin = float.Parse(substring);
                    climaActual.TemperaturaKelvin = kelvin;
                }
                if (indexDewPoint >= 0 && indexDewPointFast >= 0)
                {
                    var substring = datosLeidos.Substring(indexDewPoint + 1, 14).Trim();
                    float puntoRocio = float.Parse(substring);
                    climaActual.PuntoDeRocio = puntoRocio;

                    substring = datosLeidos.Substring(indexDewPointFast + 1).Trim();
                    float puntoRocioRapido = float.Parse(substring);
                    climaActual.PuntoDeRocioRapido = puntoRocioRapido;
                }

                // TODO: Insertar al Event Hub
                InsertarDatos(climaActual);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            ActualizarUI();*/
        }

        private void InsertarDatos(Clima climaActual)
        {
            var clienteEventHub = Microsoft.ServiceBus.Messaging.EventHubClient.CreateFromConnectionString(
                cadenaConexion, nombreEventHub);

            try
            {
                var mensaje = string.Format("{{ 'Humedad': {0}, 'TemperaturaC': {1}, 'PuntoRocio': {2}, 'Tiempo': {3:yyyy-MM-ddTHH:mm:ss.sssZ} }}",
                    climaActual.Humedad, climaActual.TemperaturaCelsius, climaActual.PuntoDeRocio, DateTimeOffset.UtcNow);

                Console.WriteLine("Mandando mensaje:\n{0}", mensaje);

                clienteEventHub.SendAsync(new Microsoft.ServiceBus.Messaging.EventData(Encoding.UTF8.GetBytes(mensaje)));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void InsertarDatos(double house, double device)
        {
            var clienteEventHub = Microsoft.ServiceBus.Messaging.EventHubClient.CreateFromConnectionString(
                cadenaConexion, nombreEventHub);

            var clienteEventHubSec = Microsoft.ServiceBus.Messaging.EventHubClient.CreateFromConnectionString(
                cadenaConexionDetalles, nombreEventHubDetalles);

            try
            {
                var mensaje = string.Format("{{ 'Name':'House1', 'Count': {0}}",
                    house);
                
                var mensajeDetalles = string.Format("{{ 'Name':'Device1', 'Count':{0}, 'HouseId':'House1' }}", device);

                Console.WriteLine("Mandando mensaje:\n{0}", mensaje);

                await clienteEventHub.SendAsync(new Microsoft.ServiceBus.Messaging.EventData(Encoding.UTF8.GetBytes(mensaje)));

                await clienteEventHubSec.SendAsync(new Microsoft.ServiceBus.Messaging.EventData(Encoding.UTF8.GetBytes(mensajeDetalles)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ActualizarUI()
        {
            textBlockHumedad.Text = string.Format("{0}%", climaActual.Humedad);
            textBlockPuntoRocio.Text = string.Format("Punto de rocío: {0} °C", climaActual.PuntoDeRocio);
            textBlockPuntoRocioRapido.Text = string.Format("Punto de rocío rápido: {0} °C", climaActual.PuntoDeRocioRapido);
            textBlockTemperatura.Text = string.Format("{0} °C", climaActual.TemperaturaCelsius);
            textBlockTemperaturaF.Text = string.Format("{0} °F", climaActual.TemperaturaFahrenheit);
        }
    }
}
