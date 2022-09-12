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
using System.Xml.Linq;
using static InterfazPrueba1.Form1;


namespace InterfazPrueba1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }
        public struct Data
        {
            public double maximo;
            public double minimo;
            public double start;
            public List<string> dias;
            public List<string> fechas;
            public List<double> gastos;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMaximizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            btnMaximizar.Visible = false;
            btnAchicar.Visible = true;
        }

        private void btnAchicar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            btnAchicar.Visible = false;
            btnMaximizar.Visible = true;
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void pnTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                int indice = cmbMes.SelectedIndex;
                int[] Nmeses = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                string[] meses = new string[] { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" };
                string[] dias = new string[] { "domingo", "sabado", "viernes", "jueves", "miercoles", "martes", "lunes"};
                Matrix datos = new Matrix();
                datos = Datos(Nmeses[indice]);
                string mes = indice.ToString();
                double dia = datos.GetValue(0, 3);
                int j = Convert.ToInt32(((dia) / 4))-1;
                for (int i = 0; i < Nmeses[indice]; i++)
                {
                    if (j < 0)
                    {
                        j = 6;
                    }
                    int n = dGV1.Rows.Add();
                    dGV1.Rows[n].Cells[0].Value = dias[j];
                    string fecha = (i + 1) + " de " + meses[indice];
                    dGV1.Rows[n].Cells[1].Value = fecha;
                    dGV1.Rows[n].Cells[2].Value = datos.GetValue(i, 4);
                    j--;
                }
            }          
        }
        public double[,] Datos(int mes)
        {
            System.IO.StreamReader archivo = new System.IO.StreamReader(openFileDialog1.FileName);
            char separador = ';';
            string linea;
            Matrix matrix = new double[mes, 5];
            int i = 0;
            while ((linea = archivo.ReadLine()) != null)
            {
                string[] fila = linea.Split(separador);
                double monto = Convert.ToDouble(fila[0]);
                double limite = Convert.ToDouble(fila[1]);
                double diasM = Convert.ToDouble(fila[2]);
                double diasS = Convert.ToDouble(fila[3]);
                double gasto = Convert.ToDouble(fila[4]);
                matrix.SetValue(i, 0, monto);
                matrix.SetValue(i, 1, limite);
                matrix.SetValue(i, 2, diasM);
                matrix.SetValue(i, 3, diasS);
                matrix.SetValue(i, 4, gasto);
                i++;
            }
            return matrix;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int indice = cmbMes.SelectedIndex;
            int[] Nmeses = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            string[] meses = new string[] { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" };
            string[] dias = new string[] { "domingo", "sabado", "viernes", "jueves", "miercoles", "martes", "lunes" };
            Matrix datos = new Matrix();
            datos = Datos(Nmeses[indice]);
            string mes = indice.ToString();
            double dia = datos.GetValue(0, 3);
            int j = Convert.ToInt32(((dia) / 4)) - 1;
            Data data;
            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();
            List<double> list3 = new List<double>();
            data.maximo = datos.GetValue(0, 0);
            data.minimo = datos.GetValue(0, 1);
            data.start = datos.GetValue(0, 3);
            data.dias = list1;
            data.fechas = list2;
            data.gastos = list3;
            for (int i = 0; i < Nmeses[indice]; i++)
            {
                if (j < 0)
                {
                    j = 6;
                }
                string fecha = (i + 1) + " de " + meses[indice];
                data.dias.Add(dias[j]);
                data.fechas.Add(fecha);
                data.gastos.Add(datos.GetValue(i, 4));
                j--;
            }
            Form2_respuestas_ form2_Respuestas1 = new Form2_respuestas_(data);
            form2_Respuestas1.ShowDialog();
        }
    }
}
