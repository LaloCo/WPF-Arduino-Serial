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
        Clima climaActual = new Clima();
        SerialPort puertoSerial = new SerialPort();
        DispatcherTimer temporizador = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            RegistrarComando();
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            ActualizarUI();
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
