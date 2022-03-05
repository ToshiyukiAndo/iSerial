using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;

namespace iSerial
{

    public partial class iSerial : Form
    {

        Random rdm = new Random(); /* 乱数 */

        Series seriesLine = new Series();
        Series seriesLine2 = new Series();
        private int index = 0;
        private int yIndex = 0;

        bool chartOptimized = false;

        List<int> startList = new List<int>();
        List<int> endList = new List<int>();

        List<ChartArea> chartAreas = new List<ChartArea>();
        List<Series> chartSeries = new List<Series>();

        public iSerial()
        {
            InitializeComponent();
            ScanCOMPorts();

            InitCharts();
        }

        private void InitCharts()
        {
            /* 初期値の削除 */
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.Titles.Clear();

            //seriesLine.ChartType = SeriesChartType.Line;
            //seriesLine.LegendText = "Legend:Line";

            //seriesLine2.ChartType = SeriesChartType.Line;
            //seriesLine2.LegendText = "Legend:Line2";
            //seriesLine.BorderWidth = 2;
            //seriesLine.MarkerStyle = MarkerStyle.Circle;
            //seriesLine.MarkerSize = 12;

            //for (int i = 0; i < 10; i++)
            //{
                //seriesLine2.Points.Add(new DataPoint(i, rdm.Next(0, 210)));
            //}

            //ChartArea area1 = new ChartArea("area1");
            //ChartArea area2 = new ChartArea("area2");

            //seriesLine2.ChartArea = "area2";

            //chart1.ChartAreas.Add(area1);
            //chart1.ChartAreas.Add(area2);
            //chart1.Series.Add(seriesLine);
            //chart1.Series.Add(seriesLine2);
        }



        private void ScanCOMPorts()
        {
            cmbCOMPort.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (string p in ports)
            {
                cmbCOMPort.Items.Add(p);
            }
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            ScanCOMPorts();
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = cmbCOMPort.Text; // COM名設定
                serialPort1.Open();                     // ポート接続
                btnOpen.Enabled = false;                // 接続　Off
                btnClose.Enabled = true;                // 切断　On
                btnScan.Enabled = false;                // 更新　Off
                cmbCOMPort.Enabled = false;             // COMリスト　Off
                btnSend.Enabled = true;                 // 送信　On
                tbxRxData.Clear();                      // 画面クリア
                tbxRxData.AppendText("Connected\r\n");  // 接続と表示
            }
            catch
            {
                BtnClose_Click(this, null);     // 切断ボタンを押す
            }
        }

        // TODO: Chartとの組み合わせでうまく動いていない部分があるっぽい
        private void BtnClose_Click(object sender, EventArgs e)
        {
            btnOpen.Enabled = true;             // 接続　On
            btnClose.Enabled = false;           // 切断　Off
            btnScan.Enabled = true;             // 更新　On
            cmbCOMPort.Enabled = true;          // COMリスト　On
            btnSend.Enabled = false;            // 送信　Off
            try
            {
                serialPort1.DiscardInBuffer();  // 入力バッファを破棄
                serialPort1.DiscardOutBuffer(); // 出力バッファを破棄
                serialPort1.Close();             // COMポートを閉じる
            }
            catch { };
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Write(tbxTxData.Text + "\r\n");
            }
            catch
            {
                BtnClose_Click(this, null);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            tbxRxData.Clear();
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort1.BytesToRead < 2)
            {
                return;
            }

            string recievedData;
            if(!serialPort1.IsOpen) return;
            try {
                recievedData = serialPort1.ReadLine();
            }
            catch
            {
                return ;
            }
            index++;/* 読み込みまで出来たら番号を増やす */
            try
            {
                SetText(recievedData);
            }catch{
                BtnClose_Click(this, null);
            }

            try
            {
                string[] splitedData = recievedData.Replace("\r", "").Replace("\n", "").Split(',');

                /* 初期化がまだの場合、ここで文解析 */
                if (index == 3 && !chartOptimized)
                {
                    System.Diagnostics.Debug.WriteLine("data for optimize");
                    System.Diagnostics.Debug.WriteLine(recievedData);
                    OptimizeCharts(splitedData);
                }else if (chartOptimized) {
                    UpdateCharts2(splitedData);
                }
                //if (splitedText[0] != "") UpdateCharts(index, Convert.ToDouble(splitedText[0]));
                //if (splitedData[1] != "") UpdateCharts(index, Convert.ToDouble(splitedData[1]));
                //if (splitedText[2] != "") UpdateCharts(index, Convert.ToDouble(splitedText[2]));

            }
            catch
            {
                return ;
            }

        }

        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            if (tbxRxData.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                BeginInvoke(d, new object[] { text });
            }
            else
            {
                tbxRxData.AppendText(text+"\n");
            }
        }

        delegate void UpdateChartsCallback(double x, double y);
        private void UpdateCharts(double x, double y)
        {
            if (chart1.InvokeRequired){
                UpdateChartsCallback d = new UpdateChartsCallback(UpdateCharts);
                BeginInvoke(d, new object[] { x, y });
            }else{
                seriesLine.Points.Add(new DataPoint(x, y));
                chart1.ChartAreas["area1"].AxisX.Minimum = x - 100;
            }
        }

        delegate void UpdateCharts2Callback(string[] splitedData);
        private void UpdateCharts2(string[] splitedData)
        {
            if (chart1.InvokeRequired)
            {
                UpdateCharts2Callback d = new UpdateCharts2Callback(UpdateCharts2);
                BeginInvoke(d, new object[] { splitedData });
            }
            else
            {
                int currentChartArea = -1;
                int currentSeries = -1;
                foreach(var val in splitedData)
                {
                    if (val.Equals("<"))
                    {
                        currentChartArea++;
                    }
                    else if (val.Equals(">"))
                    {
                        chart1.ChartAreas["cArea" + currentChartArea.ToString()].AxisX.Minimum = yIndex - 100;
                    }
                    else if (currentChartArea != -1) /* -1だとareaがないのでスルー */
                    {
                        currentSeries++;
                        chartSeries[currentSeries].Points.Add(new DataPoint(yIndex, Convert.ToDouble(val)));
                        yIndex++;
                    }
                    else
                    {
                    }
                }
                //seriesLine.Points.Add(new DataPoint(splitedData);
            }
        }

        delegate void OptimizeChartsCallback(string[] splitedData);

        private void OptimizeCharts(string[] splitedData)
        {
            if (chart1.InvokeRequired)
            {
                System.Diagnostics.Debug.WriteLine("OPT2");
                OptimizeChartsCallback d = new OptimizeChartsCallback(OptimizeCharts);
                Invoke(d, new object[] { splitedData });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OPT");
                /* <と>がどれだけあるかの判別 */
                int num = -1;
                foreach (string s in splitedData)
                {
                    num++;
                    if(s.Equals("<")) startList.Add(num);
                    if(s.Equals(">")) endList.Add(num);
                }
                if (startList.Count != endList.Count) chartOptimized =  false; /* <と>の数が違うとき */

                int currentChartArea = -1;
                /* <>の数に併せてchartを作成 */

                foreach (var val in splitedData) {
                    System.Diagnostics.Debug.WriteLine("|");
                    System.Diagnostics.Debug.WriteLine(val);
                    if (val.Equals("<")) {
                        currentChartArea++;
                        ChartArea cA = new ChartArea("cArea" + currentChartArea.ToString());
                        chart1.ChartAreas.Add(cA);
                    }
                    else if (val.Equals(">"))
                    {
                        /* 閉じかっこの時（何もしない） */
                    }
                    else if(currentChartArea != -1 && val != "") /* -1だとareaがないのでスルー */
                    {
                        var s = new Series();
                        s.ChartType = SeriesChartType.Line;
                        s.LegendText = "Legend:Line";
                        s.ChartArea = "cArea" + currentChartArea.ToString();
                        chartSeries.Add(s);
                        chart1.Series.Add(s);
                        System.Diagnostics.Debug.WriteLine(chartSeries.Count);
                    }
                    else
                    {
                    }
                }
                System.Diagnostics.Debug.WriteLine("checking");
                System.Diagnostics.Debug.WriteLine((currentChartArea + 1) * 2 + chartSeries.Count);
                System.Diagnostics.Debug.WriteLine(chartSeries.Count);
                System.Diagnostics.Debug.WriteLine("/");
                System.Diagnostics.Debug.WriteLine(splitedData.Length);

                if ((currentChartArea+1)*2 + chartSeries.Count == splitedData.Length) {
                    chartOptimized = true;
                }
            }
        }

        private void Chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
