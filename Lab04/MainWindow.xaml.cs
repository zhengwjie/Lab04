using System;
using System.Collections.Generic;
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
using System.IO.Ports;
using Microsoft.Win32;
using System.IO;
using ZedGraph;
using System.Drawing;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Color = System.Drawing.Color;
namespace Lab04
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort serialPort = new SerialPort();
        bool Record = false;
        BindText temper = new BindText();
        BindText intens = new BindText();
        private StreamWriter sr = null;
        PointPairList list1 = new PointPairList();
        PointPairList list2 = new PointPairList();
        LineItem myCurve02;
        LineItem myCurve01;
        public MainWindow()
        {
            InitializeComponent();
            searchPortName();
            serialPort.ReceivedBytesThreshold = 3;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(this.OnDateReceived);
            Temp.DataContext = temper;
            Inte.DataContext = intens;
            init();
        }
        private void init()
        {
            zedGraphControl1.GraphPane.Title.Text = "温度实时动态图";
            zedGraphControl1.GraphPane.Title.FontSpec.Size = 30;
            zedGraphControl1.GraphPane.XAxis.Title.Text = "时间";
            zedGraphControl1.GraphPane.XAxis.Title.FontSpec.Size = 20;
            zedGraphControl1.GraphPane.YAxis.Title.FontSpec.Size = 20;
            zedGraphControl1.GraphPane.YAxis.Title.Text = "温度";
            zedGraphControl2.GraphPane.Title.Text = "光强实时动态图";
            zedGraphControl2.GraphPane.Title.FontSpec.Size = 30;
            zedGraphControl2.GraphPane.XAxis.Title.Text = "时间";
            zedGraphControl2.GraphPane.XAxis.Title.FontSpec.Size = 20;
            zedGraphControl2.GraphPane.YAxis.Title.FontSpec.Size = 20;
            zedGraphControl2.GraphPane.YAxis.Title.Text = "光强";
            zedGraphControl2.GraphPane.XAxis.Type = ZedGraph.AxisType.DateAsOrdinal;
            myCurve02=zedGraphControl2.GraphPane.AddCurve("光强",
        list2, System.Drawing.Color.Red, SymbolType.None);
            myCurve01= zedGraphControl1.GraphPane.AddCurve("温度",
        list1, System.Drawing.Color.YellowGreen, SymbolType.None);
            zedGraphControl1.AxisChange();
            zedGraphControl1.Refresh();
        }
        private void OnDateReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int k = serialPort.BytesToRead;
            for (int i = 0; i < k; i++)
            {
                HandleMidFormat handleMid = new HandleMidFormat();
                byte midk = (byte)serialPort.ReadByte();
                if ((midk >> 4) == 0xE)
                {
                    handleMid.mid[0] = midk;
                    handleMid.mid[1] = (byte)serialPort.ReadByte(); i++;
                    handleMid.mid[2] = (byte)serialPort.ReadByte(); i++;
                    //AD通道：0是温度，1是光强
                    String te = null;
                    String li = null;
                    int f = midk & 0xf;
                    if (f == 0)
                    {
                        temper.Show = ((int)(handleMid.getTempertaure(handleMid.getData()))).ToString();
                        te = handleMid.MidFormat.ToString();
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            listView2.Items.Add(te);
                            double x = (double)new XDate(DateTime.Now);
                            list1.Add(x, handleMid.getTempertaure(handleMid.getData()));
                            zedGraphControl1.AxisChange();
                            zedGraphControl1.Refresh();
                            if (Record)
                            {
                                string str = "{\"ReceiveData\":\"" + te + "\"}";
                                sr.WriteLine(str);
                                sr.Flush();
                            }
                        }
                            ));
                    }
                    else if (f == 1)
                    {
                        intens.Show = handleMid.getData().ToString();
                        li = handleMid.MidFormat.ToString();
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            listView2.Items.Add(li);
                            double x = (double)new XDate(DateTime.Now);
                            list2.Add(x, handleMid.getData());
                            zedGraphControl2.AxisChange();
                            zedGraphControl2.Refresh();
                            if (Record)
                            {
                                string str = "{\"ReceiveData\":\"" + li + "\"}";
                                sr.WriteLine(str);
                                sr.Flush();
                            }
                        }
                           ));
                    }
                }
            }
        }
        //串口端口号搜索并添加到combox中
        public void searchPortName()
        {
            string[] portNames = SerialPort.GetPortNames();
            for(int i=0;i<portNames.Length;i++)
            {
                string name = portNames[i];
                bool exists = false;
                for(int j=0;j<combox1.Items.Count;j++)
                {
                    if (name.Equals(combox1.Items[j]))
                        exists = true;
                }
                if (exists)
                    continue;
                combox1.Items.Add(name);
            }
        }

        private void select_PortName(object sender, SelectionChangedEventArgs e)
        {
            serialPort.PortName = (String)combox1.SelectedItem;
            //MessageBox.Show(SerialPort.PortName);
        }

        private void select_PortBps(object sender, SelectionChangedEventArgs e)
        {
            String str = combox2.SelectedItem.ToString();
            string result = System.Text.RegularExpressions.Regex.Replace(str, @"[^0-9]+", "");
            int bps = int.Parse(result);
            serialPort.BaudRate = bps;
        }

        private void comBox1_Focus(object sender, RoutedEventArgs e)
        {
            searchPortName();
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if(!serialPort.IsOpen)
                serialPort.Open();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(serialPort.IsOpen)
            serialPort.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {   //3（红）,5（绿）,6（黄）,9（蓝）,10（白）pwm输出控制
                HandleMidFormat handleMid = new HandleMidFormat();
                handleMid.CreateMidiPWM((int)slider01.Value, 3);
                serialPort.Write(handleMid.mid, 0, 3);
                listView1.Items.Add(handleMid.MidFormat);
                if (Record)
                {
                    String str = "{\"Send\":\"" + handleMid.MidFormat + "\"}";
                    sr.WriteLine(str);
                    sr.Flush();
                }
                handleMid.CreateMidiPWM((int)slider02.Value, 5);
                serialPort.Write(handleMid.mid, 0, 3);
                listView1.Items.Add(handleMid.MidFormat);
                if (Record)
                {
                    String str = "{\"Send\":\"" + handleMid.MidFormat + "\"}";
                    sr.WriteLine(str);
                    sr.Flush();
                }
                handleMid.CreateMidiPWM((int)slider03.Value, 6);
                serialPort.Write(handleMid.mid, 0, 3);
                listView1.Items.Add(handleMid.MidFormat);
                if (Record)
                {
                    String str = "{\"Send\":\"" + handleMid.MidFormat + "\"}";
                    sr.WriteLine(str);
                    sr.Flush();
                }
                handleMid.CreateMidiPWM((int)slider04.Value, 9);
                serialPort.Write(handleMid.mid, 0, 3);
                listView1.Items.Add(handleMid.MidFormat);
                if (Record)
                {
                    String str = "{\"Send\":\"" + handleMid.MidFormat + "\"}";
                    sr.WriteLine(str);
                    sr.Flush();
                }
                handleMid.CreateMidiPWM((int)slider05.Value, 10);
                serialPort.Write(handleMid.mid, 0, 3);
                listView1.Items.Add(handleMid.MidFormat);
                if (Record)
                {
                    String str = "{\"Send\":\"" + handleMid.MidFormat + "\"}";
                    sr.WriteLine(str);
                    sr.Flush();
                }    
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        private void Color_Change(object sender, System.Windows.DragEventArgs e)
        {
            
        }

        private void Color_Change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color Red = Color.Red;
            Color Green = Color.Green;
            Color Yellow = Color.Yellow;
            Color Blue = Color.Blue;
            Color White = Color.White;
            double r = Red.R * slider01.Value / 5 + Green.R * slider02.Value / 5 + Yellow.R * slider03.Value / 5
                + Blue.R * slider04.Value / 5 + White.R * slider05.Value / 5;
            double g = Red.G * slider01.Value / 5 + Green.G * slider02.Value / 5 + Yellow.G * slider03.Value / 5 + 
                Blue.G * slider04.Value / 5 + White.G * slider05.Value / 5;
            double b = Red.B * slider01.Value / 5 + Green.B * slider02.Value / 5 + Yellow.B * slider03.Value / 5 + 
                Blue.B * slider04.Value / 5 + White.B * slider05.Value / 5;
            ShowColor.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)r,(byte)g,(byte)b));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog fl = new Microsoft.Win32.SaveFileDialog();
            fl.Title = "请选择文件路径";
            fl.Filter = "Json格式|*.json|文本格式|*.txt";
            fl.DefaultExt = "文本格式|*.txt";
            DateTime dateTime = DateTime.Now;
            String timeNow = dateTime.ToString("yyyy-MM-dd-HH-mm-ss");
            fl.FileName = "log-" + timeNow + ".txt";
            bool result = (bool)fl.ShowDialog();
            if (result==true)
            {
                Record = true;
                string file = fl.FileName;
                sr = new StreamWriter(file);
                String str01 = "{\"PortName\":\""+combox1.SelectedItem+"\"}\n";
                sr.Write(str01);
                String str02= "{\"PortBPS\":\"" + (combox2.SelectedValue.ToString()).Split(':')[1] + "\"}\n";
                sr.Write(str02);
                String str03= "{\"Temperature\":\"" + Temp.Text + "\"}\n";
                sr.Write(str03);
                String str04= "{\"Light Intensity\":\"" + Inte.Text + "\"}\n";
                sr.Write(str04);
                String str05= "{\"LED PWM Values\":["+(int)slider01.Value+","+(int)slider02.Value+","+(int)slider03.Value+","+(int)slider04.Value
                    +","+(int)slider05.Value+ "]}\n";
                sr.Write(str05);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if(Record)
            {
                sr.Flush();
                sr.Close();
                Record = false;
            }
        }

        private void WindowsFormsHost_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            serialPort.DataReceived -= new SerialDataReceivedEventHandler(this.OnDateReceived);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(this.OnDateReceived);
        }

        private void comBox1_Focus(object sender, System.Windows.Input.KeyEventArgs e)
        {
            searchPortName();
        }
    }
}
