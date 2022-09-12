using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;


namespace InterfazPrueba1
{
    public partial class Form2_respuestas_ : Form
    {
        public Form2_respuestas_(Form1.Data datos)
        {
            InitializeComponent();
            chart1.Series["Series1"].ChartType = SeriesChartType.Line;
            chart1.Series["Series1"].BorderWidth = 5;
            chart1.Series["Series1"].Color = Color.Red;
            chart1.Series["Series1"].LegendText = "Grafico de gastos";

            chart2.Series["Series1"].ChartType = SeriesChartType.Line;
            chart2.Series["Series1"].BorderWidth = 5;
            chart2.Series["Series1"].Color = Color.Blue;
            chart2.Series["Series1"].LegendText = "Grafico descendiente";
            
            Dictionary<int, double> data = new Dictionary<int, double>();

            List<double> data2 = new List<double>();
            double inicio = datos.start/10;
            for (int i = 0; i < datos.gastos.Count; i++)
            {
                if (inicio < 0.4)
                {
                    inicio = 2.8;
                }
                data.Add(i + 1, datos.gastos[i]);
                data2.Add(numeroN(datos.maximo, datos.minimo, datos.gastos.Count,inicio));
                inicio -= 0.4;
            }
            double max = datos.maximo;
            
            foreach (KeyValuePair<int,double> d in data)
            {
                chart1.Series["Series1"].Points.AddXY(d.Key, d.Value);
                chart2.Series["Series1"].Points.AddXY(d.Key, max);
                max = max - d.Value;                
            }
            Perceptron p = new Perceptron(new int[] { 5, 20, 10, 1 });
            outputRequest(p, datos);
            for (int j = 0; j < datos.gastos.Count; j++)
            {
                if (datos.gastos[j] > data2[j])
                {
                    int n = dGV2.Rows.Add();
                    dGV2.Rows[n].Cells[0].Value = datos.dias[j];
                    dGV2.Rows[n].Cells[1].Value = datos.fechas[j];
                    dGV2.Rows[n].Cells[2].Value = datos.gastos[j];
                    dGV2.Rows[n].Cells[3].Value = datos.gastos[j] - data2[j];
                }
            }
            
        }
        public double numeroN(double max, double min, double diasM, double diasS)
        {
            return ((max - min) / diasM) / diasS;
        }
        private void btnCerrar1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMaximizar1_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            btnMaximizar1.Visible = false;
            btnAchicar1.Visible = true;
        }

        private void btnAchicar1_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            btnAchicar1.Visible = false;
            btnMaximizar1.Visible = true;
        }

        private void btnMinimizar1_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void pnTitulo1_MouseDown_1(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        static string neuronPath = @"D:\pruebas\ProyectoAvanzados\InterfazPrueba1\neuralNetwork.bin";        


        static double Normalize(double value, double min, double max)
        {
            return (value - min) / (max - min);
        }
        static double InverseNormalize(double value, double min, double max)
        {
            return value * (max - min) + min;
        }
        
        static void outputRequest(Perceptron p,Form1.Data data)
        {
            double inputMax = 3000;
            double inputMin = 0.4;
            int outputCount = 1;
            double[] output = new double[outputCount];
            FileStream fs = new FileStream(neuronPath, FileMode.Open);
            int j = 0;
            bool flag = true;
            while (flag)
            {
                double[] val = new double[5];
                for (int i = 0; i < 5; i++)
                {
                    val[i] = Normalize(data.gastos[i], inputMin, inputMax);
                }

                double[] sal = p.Activate(val);
                for (int i = 0; i < outputCount; i++)
                {
                    output[i] = sal[i];
                }
                if (j == data.maximo)
                {
                    flag = false;
                }
                j += 1;
            }
        }
    }
}
