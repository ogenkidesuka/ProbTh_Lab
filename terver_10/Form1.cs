using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;

namespace terver_10
{
    public partial class Form1 : Form
    {
        protected Random rnd = new Random((int)DateTime.Now.Ticks);                        // - псевдослуч чисел генератор
        protected SortedDictionary<int, int> vals;                                          // - отсорт словарь, в котором хранятся значения сл в (key) и их кол-во (value)
        protected int N = 1000;                                                                  // - кол-во экспериментов
        protected double p1 = 0.3;                                                                // - вероятность попадания первого
        protected double p2 = 0.2;                                                                // - вероятность попадания второго

        protected double alpha;                                                             // - уровень значимости
        protected int interv_amt;                                                          // - количество разбиений




        public Form1()
        {
            InitializeComponent();
            vals = new SortedDictionary<int, int>();

            // - настройки графика
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 1;
            chart1.BackColor = Color.DarkGray;

            chart2.ChartAreas[0].AxisX.Minimum = 0;
            chart2.ChartAreas[0].AxisY.Minimum = 0;
            chart2.ChartAreas[0].AxisY.Maximum = 1.2;
            chart2.BackColor = Color.DarkGray;

            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine;
            chart1.Series[0].BorderWidth = 3;
            chart1.Series[0].Name = "p";
            chart1.Series[0].Color = Color.ForestGreen;
            chart1.Series.Add("v");
            chart1.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine;
            chart1.Series[1].BorderWidth = 2;
            chart1.Series[1].Color = Color.Purple;
            //chart1.Series.Add("Теор расп");
            chart2.Series[0].Name = "F_etta";
            chart2.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine;
            chart2.Series[0].BorderWidth = 2;
            chart2.Series[0].Color = Color.Red;
            chart2.Series.Add("^F_etta");
            chart2.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StepLine;
            chart2.Series[1].BorderWidth = 2;
            chart2.Series[1].Color = Color.Blue;

            label12.Text = " ";
        }
        private double probabilityLaw(int _etta)                                   // - теоретический закон распределения
        {
            if (_etta == 0)
                return p1;
            else
                return Math.Pow(1 - p1, _etta) * Math.Pow(1 - p2, _etta - 1) * (p1 + p2 - p1 * p2);
        }
        private double gam(double x)
        {
            return chart1.DataManipulator.Statistics.GammaFunction(x);
        }
        private double f_hi(double x)
        {
            if (x <= 0)
                return 0;
            else
            {
                double r = Convert.ToDouble(interv_amt - 1);
                return Math.Pow(2,-r/2) * Math.Pow(gam(r/2),-1) * Math.Pow(x, r/2 - 1) * Math.Exp(-x / 2);
            }
        }

        private double _F(double R)
        {
            double integr = 0;

            for (int k = 1; k <= N; k++)
            {
                integr += f_hi( Convert.ToDouble(R * (k - 1) / N)) + f_hi( Convert.ToDouble(R*k/N) );
            }
            integr *= R / Convert.ToDouble(2 * N);
            return (1 - integr);

        }
        private double probabilityLawP(int _etta)
        {
            double res;
            if (!vals.TryGetValue(_etta, out int tmp))
                res = 0;
            res = Convert.ToDouble(tmp) / Convert.ToDouble(N);
            return res;
        }
        private double distributionLaw(int x)
        {
            double res = 0;
            /*foreach(var sd in vals)
            {
                res += probabilityLaw(sd.Key);
            }*/
            for (int i = 0; i <= x; i++)
                res += probabilityLaw(i);
            return res;
        }
        private double distributionLawP(int x)
        {
            double res = 0;
            /*foreach (var sd in vals)
            {
                res += probabilityLawP(sd.Key);
            }*/
            for (int i = 0; i <= x; i++)
                res += probabilityLawP(i);
            return res;
        }
        private int RandomValueGen()                                                        // - возвращает этту
        {
            int etta = 0;                                                                 // - число бросков 2 баскетболиста
            double right = p1;                                                              // - правая граница полуинтервала
            double left = 0;                                                                // - левая граница полуинтервала
            double roll = Convert.ToDouble(rnd.Next(100000) / 100000.0);                                // - сл.в
            //double roll = rnd.NextDouble();
            while (roll > right)                                                            // - пока roll не входит в полуинт
            {
                etta++;
                left = right;
                //right = left + Math.Pow(1 - p1, etta) * Math.Pow(1 - p2, etta - 1) * (p1 + p2 - p1 * p2);   // - перебираем разные полуинт согласно з.расп.
                right = left + probabilityLaw(etta);
            }
            return etta;
        }
        private double mathExpectation()                                                    // - подсчет теоретического мат. ожидания
        {
            double res = 0;
            for(int i = 0; i < 10000; i++)
            {
                res += i * probabilityLaw(i);
            }
            return res;
            //return 1.0 / ((p1 - 1)*Math.Pow(p2 - 1,2)*(p1*p2-p1-p2));
        }
        private double mathExpectationP()                                                    // - подсчет Практическогго мат. ожидания
        {
            double res = 0;
            foreach (var sd in vals)
            {
                res += sd.Key * (Convert.ToDouble(sd.Value) / Convert.ToDouble(N));
            }
            return res;
        }
        private double dispersion()                                                         // - дисперсия
        {
            // - подсчет м о квадрата сл в
            double res = 0;
            for (int i = 0; i < 10000; i++)
            {
                res += i*i * probabilityLaw(i*i);
            }
            res -= Math.Pow(mathExpectation(), 2);
            return Math.Abs(res);
        }
        private double dispersionP()                                                        // - выб.дисперсия
        {
            double res = 0, mid = mathExpectationP();
            //int n = 0;
            foreach (var sd in vals)
            {
                res += Math.Pow(sd.Key - mid, 2);
            }
            res = (Convert.ToDouble(res)) / (Convert.ToDouble(N));
            return res;
        }
        private double mediane()
        {
            double res = 0;
            List<int> tmp = new List<int>();

            foreach (var sd in vals)
                for (int i = 0; i < sd.Value; i++)
                    tmp.Add(sd.Key);

            //tmp.Sort();

            int ind = N / 2;

            if (N % 2 == 0)
                res = Convert.ToDouble(tmp.ElementAt(ind) + tmp.ElementAt(ind++)) / 2.0;
            else
                res = tmp.ElementAt(ind);
            return res;
        }
        private int range()
        {
            return vals.Last().Key-vals.First().Key;
        }
        private void button1_Modelling_Click(object sender, EventArgs e)
        {
            /*while (dataGridView1.RowCount > 1)                                              // - чистим таблу
                dataGridView1.Rows.RemoveAt(dataGridView1.RowCount - 2);*/
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart2.Series[1].Points.Clear();
            //SortedDictionary<int, int> vals = new SortedDictionary<int, int>();
            vals.Clear();
            // - pt 1 begin ////////////////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < N; i++)
            {
                int ranVal = RandomValueGen();
                int tmpout;
                if (!vals.TryGetValue(ranVal,out tmpout))                                   // - если в словаре еще не встречался ключ ranVal
                {
                    vals.Add(ranVal, 0);                                                    // - то добавим его в словарь со значением 0
                }
                vals[ranVal]++;
            }
            int iRow = 0;
            foreach(var sd in vals) // - ключ - значение этты, вэлью - кол-во появлений этты
            {
                //MessageBox.Show(((double)(sd.Value / N)).ToString());
                dataGridView1.Rows.Add();
                dataGridView1.Rows[iRow].Cells[0].Value = sd.Key.ToString();
                dataGridView1.Rows[iRow].Cells[1].Value = probabilityLaw(sd.Key).ToString();
                //dataGridView1.Rows[iRow].Cells[2].Value = (Convert.ToDouble(sd.Value)/Convert.ToDouble(N)).ToString();
                dataGridView1.Rows[iRow].Cells[2].Value = probabilityLawP(sd.Key).ToString();

                iRow++;
            }
            dataGridView1.Rows.Add();

            // подсчет макс. разности между вероятностью и частотой
            double maxdiffpr = 0;
            /*foreach(var sd in vals)
            {
                double tmp = Math.Abs(probabilityLaw(sd.Key) - probabilityLawP(sd.Key));
                if (tmp > maxdiffpr)
                    maxdiffpr = tmp;
            }*/

            for(int i = 0; i <= vals.Last().Key + 10; i++)
            {
                double tmp = Math.Abs(probabilityLaw(i) - probabilityLawP(i));
                if (tmp > maxdiffpr)
                    maxdiffpr = tmp;
            }

            dataGridView1.Rows[iRow].Cells[1].Value = "max |pj - vj|:";
            dataGridView1.Rows[iRow].Cells[2].Value = maxdiffpr.ToString();

            iRow++;
            dataGridView1.Rows.Add();

            // - разница между теор и выбор функ распр
            double maxdiffdi = 0;
            /*foreach (var sd in vals)
            {
                double tmp = Math.Abs(distributionLaw(sd.Key) - distributionLawP(sd.Key));
                if (tmp > maxdiffdi)
                    maxdiffdi = tmp;
            }*/

            for (int i = 0; i <= vals.Last().Key; i++)
            {
                double tmp = Math.Abs(distributionLaw(i) - distributionLawP(i));
                if (tmp > maxdiffdi)
                    maxdiffdi = tmp;
            }

            dataGridView1.Rows[iRow].Cells[1].Value = "max |Fj - F^j|:";
            dataGridView1.Rows[iRow].Cells[2].Value = maxdiffdi.ToString();
            // - pt 1 end ////////////////////////////////////////////////////////////////////////////////////////
            // - pt 2 begin //////////////////////////////////////////////////////////////////////////////////////
            //dataGridView2.Rows.Add();
            dataGridView2.Rows[0].Cells[0].Value = mathExpectation().ToString(); // - мат.ож
            dataGridView2.Rows[0].Cells[1].Value = mathExpectationP().ToString(); // - выб.ср.
            dataGridView2.Rows[0].Cells[2].Value = Math.Abs(mathExpectation() - mathExpectationP()).ToString(); // - разница мат.ож. и выб.ср
            dataGridView2.Rows[0].Cells[3].Value = dispersion().ToString(); // -дисп
            dataGridView2.Rows[0].Cells[4].Value = dispersionP().ToString(); // - выб дисп
            dataGridView2.Rows[0].Cells[5].Value = Math.Abs(dispersion() - dispersionP()).ToString(); // - разница дисп и выб.дисп
            dataGridView2.Rows[0].Cells[6].Value = mediane(); // - выб дисп
            dataGridView2.Rows[0].Cells[7].Value = range(); // - разница дисп и выб.дисп

            for(int i = 0; i <= vals.Last().Key; i++)
            {
                chart1.Series[0].Points.AddXY(i, probabilityLaw(i));
                chart1.Series[1].Points.AddXY(i, probabilityLawP(i));
                chart2.Series[0].Points.AddXY(i, distributionLaw(i));
                chart2.Series[1].Points.AddXY(i, distributionLawP(i));
            }
            /*for (int i = 0; i < vals.Count; i++)
            {
                chart1.Series[0].Points.AddXY(i, probabilityLaw(i));
                chart1.Series[1].Points.AddXY(i, probabilityLawP(i));
            }*/
        }

        private void textBox_p1_Leave(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox_p1.Text, out p1))
            {
                button1_Modelling.Enabled = false;
                MessageBox.Show("p1 должна быть дробным числом от 0 до 1");
            }
            else
                button1_Modelling.Enabled = true;
        }

        private void textBox_p2_Leave(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox_p2.Text, out p2))
            {
                button1_Modelling.Enabled = false;
                MessageBox.Show("p2 должна быть дробным числом от 0 до 1");
            }
            else
                button1_Modelling.Enabled = true;
        }

        private void textBox_Namt_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox_Namt.Text, out N))
            {
                if (N <= 0)
                {
                    button1_Modelling.Enabled = false;
                    MessageBox.Show("N должно быть положительным целым");
                }
            }
            else
            {
                if (N <= 0)
                {
                    button1_Modelling.Enabled = false;
                    MessageBox.Show("N должно быть положительным целым");
                }
                button1_Modelling.Enabled = true;
            }
        }

        private void rollTesterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(Convert.ToDouble(rnd.Next(100000) / 100000.0).ToString(),"Roll Test",MessageBoxButtons.RetryCancel);
            while (result == DialogResult.Retry)
            {
                result = MessageBox.Show(Convert.ToDouble(rnd.Next(100000) / 100000.0).ToString(), "Roll Test", MessageBoxButtons.RetryCancel);
            }

        }

        private void button1_Click(object sender, EventArgs e)  // - проверка гипотезы
        {
            double left_border;                                 // - левая граница
            double right_border;                                // - правая граница


            double tmpprob = 0;
            for (int i = 0; i < interv_amt; i++)    // - проходимся по каждому интервалу из третьего дгв
            {
                // - ставим левую границу интервала
                if (dataGridView3.Rows[i].Cells[0].Value.ToString() == "-inf")     // - если написано -inf
                    left_border = double.MinValue;                      // - устанавливаем самое мин значение (-inf)
                else
                    left_border = Convert.ToDouble(dataGridView3.Rows[i].Cells[0].Value);       // - иначе берем значение из 0 столба

                // - ставим правую границу интервала
                if (dataGridView3.Rows[i].Cells[1].Value.ToString() == "+inf")                   // - если написано +inf
                    right_border = double.MaxValue;                     // - устанавливаем самое макс значение (+inf)
                else
                    right_border = Convert.ToDouble(dataGridView3.Rows[i].Cells[1].Value);      // - иначе берем значение из 1 столба

                // - ПОДСЧЕТ КОЛИЧЕСТВА ВХОЖДЕНИЙ
                dataGridView3.Rows[i].Cells[2].Value = 0;               // - кол-во вхождений изначально ставим 0
                foreach (var sd in vals)                                 // - обходим только загенерированной сл в
                {
                    if ((sd.Key >= left_border) && (sd.Key < right_border))   // - если значение сл в попадает в текущий интервал
                        dataGridView3.Rows[i].Cells[2].Value = Convert.ToInt32(dataGridView3.Rows[i].Cells[2].Value) + sd.Value;    // - то к текущему кол-ву вхождений прибавляем кол-во выпадений текущей этты
                }
                // - ПОДСЧЕТ ВЕРОЯТНОСТЕЙ
                dataGridView3.Rows[i].Cells[3].Value = 0;               // - вероятность изначально ставим 0
                for (int j = 0; j <= vals.Last().Key; j++)              // - обходим все сл в до последней загенерированной
                {
                    if ((j >= left_border) && (j < right_border))         // - если значение сл в попадает в текущий интервал
                    {
                        if (i != interv_amt - 1)
                        {
                            dataGridView3.Rows[i].Cells[3].Value = Convert.ToDouble(dataGridView3.Rows[i].Cells[3].Value) + probabilityLaw(j); // - прибавляем к текущей вероятности вероятность j
                            tmpprob += probabilityLaw(j);       // - это нужно для подсчета вероятности на последнем интервале
                        }
                        else
                            dataGridView3.Rows[i].Cells[3].Value = 1 - tmpprob;
                    }
                }
            }

            // - ПОДСЧЕТ R_0
            double R = 0;

            for (int i = 0; i < interv_amt; i++)
            {
                int entryAmount = Convert.ToInt32(dataGridView3.Rows[i].Cells[2].Value);    // - кол-во вхождений
                double probInter = Convert.ToDouble(dataGridView3.Rows[i].Cells[3].Value); // - вероятность попадания в интервал
                
                R += Math.Pow(entryAmount - N * probInter, 2)/(N * probInter);              // - считаем р
            }
            label10.Text = "R_0 = ";
            label10.Text += R;                  // - вывод

            // - ПОДСЧЕТ F(R_0)

            //double F = Math.Abs( 1 - Math.Pow( 2, -((interv_amt-1)/2)) /** gamma*/ * (Math.Pow(R, (interv_amt-1)/2 - 1 ) * Math.Exp(-R/2) * R ));


            /*double F = 1;
            double intgr = 0;

            for (int i = 0; i < N; i++)
            {
                intgr += f_hi( (R * (i - 1)) / Convert.ToDouble(N)) + f_hi( (R * i) / Convert.ToDouble(N));
            }
            F = 1 - ((R / (2 * N)) * intgr);*/

            double F = _F(R);

            label11.Text = "F(R_0) = ";
            label11.Text += F;

            if (F > alpha)
                label12.Text = "Гипотеза принята";
            else
                label12.Text = " ";
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox1.Text, out alpha))
            {
                button1.Enabled = false;
                MessageBox.Show("Ошибка ввода");
            }
            else
                button1.Enabled = true;
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox2.Text, out interv_amt))
            {
                button1.Enabled = false;
                MessageBox.Show("Ошибка ввода");
            }
            else
            {
                button1.Enabled = true;
                dataGridView3.RowCount = interv_amt;
                dataGridView3.Rows[0].Cells[0].Value = "-inf";
                dataGridView3.Rows[interv_amt-1].Cells[1].Value = "+inf";
            }
        }

        private void gammaTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Math.Pow(chart1.DataManipulator.Statistics.GammaFunction(Convert.ToDouble((interv_amt - 1) / 2.0)), -1).ToString());
        }

        private void fhiTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(f_hi(0.55).ToString());
        }
    }
}
