using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
namespace U_I

{
    public partial class MainForm : Form
    {

        private Label voltageLabel;
        private Label currentLabel;
        private Label frequencyLabel;
        private Label powerLabel;
        private Button generatorButton;


        private double voltage = 0.0;
        private double current = 0.0;
        private double frequency = 0.0;
        private double power = 0.0;
        private bool generatorActive = false;

        private HttpClient client;

        public MainForm()
        {

            InitializeComponents();
            client = new HttpClient();
        }


        private void InitializeComponents()
        {
            voltageLabel = new Label { Top = 50, Left = 50, Text = $"Voltage: {voltage} V", Width = 200 };
            this.Controls.Add(voltageLabel);

            currentLabel = new Label { Top = 100, Left = 50, Text = $"Current: {current} A", Width = 200 };
            this.Controls.Add(currentLabel);

            frequencyLabel = new Label { Top = 150, Left = 50, Text = $"Frequency: {frequency} Hz", Width = 200 };
            this.Controls.Add(frequencyLabel);

            powerLabel = new Label { Top = 200, Left = 50, Text = $"Power: {power} W", Width = 200 };
            this.Controls.Add(powerLabel);

            generatorButton = new Button { Top = 250, Left = 50, Text = "Generator: Off", Width = 150 };
            generatorButton.Click += GeneratorButton_Click;
            this.Controls.Add(generatorButton);
        }


        private async void StartPolling()
        {
            while (true)
            {
                await GetSensorData();
                await Task.Delay(1000);
            }
        }


        private async Task GetSensorData()
        {
            try
            {

                HttpResponseMessage response = await client.GetAsync("http://localhost:8080/api/sensors");

                if (response.IsSuccessStatusCode)
                {

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var sensorData = JsonConvert.DeserializeObject<SensorData>(jsonResponse);


                    voltage = sensorData.Voltage;
                    current = sensorData.Current;
                    frequency = sensorData.Frequency;
                    power = sensorData.Power;
                    generatorActive = sensorData.GeneratorActive;


                    UpdateUI();
                }
                else
                {
                    MessageBox.Show("Error: Unable to fetch data from the server.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }


        private void UpdateUI()
        {

            if (InvokeRequired)
            {
                Invoke(new Action(UpdateUI));
                return;
            }


            voltageLabel.Text = $"Voltage: {voltage} V";
            currentLabel.Text = $"Current: {current} A";
            frequencyLabel.Text = $"Frequency: {frequency} Hz";
            powerLabel.Text = $"Power: {power} W";
            generatorButton.Text = generatorActive ? "Generator: On" : "Generator: Off";
        }


        private async void GeneratorButton_Click(object sender, EventArgs e)
        {
            try
            {

                var command = new { Action = generatorActive ? "StopGenerator" : "StartGenerator" };
                string jsonCommand = JsonConvert.SerializeObject(command);


                var content = new StringContent(jsonCommand, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("http://localhost:8080/api/manual-control", content);

                if (response.IsSuccessStatusCode)
                {
                    generatorActive = !generatorActive;
                    UpdateUI();
                }
                else
                {
                    MessageBox.Show("Error: Unable to control generator.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }
    }


    public class SensorData
    {
        public double Voltage { get; set; }
        public double Current { get; set; }
        public double Frequency { get; set; }
        public double Power { get; set; }
        public bool GeneratorActive { get; set; }
    }
}
