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
using System.IO;

namespace Stopwatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // List со всеми резудьтатами секундомера.
        List<TimeSpan> results = new List<TimeSpan>();

        // Переменная для хранени номера круга.
        int lapNumber = 1;

        // Переменная для хранения номера секундомера.
        int timeNumber = 1;

        // Переменная для храения предыдущего конца круга.
        TimeSpan prevLap;

        // Флаг для запуска секундомера.
        bool isRunning = false;

        // Переменная для вычисления пройденного времения с начала отсчета.
        TimeSpan deltaTime;

        // Переменная для хранения начального времени.
        DateTime tStart;

        // Переменная для хранения времени, когда секундомер был остановлен.
        DateTime tPause;

        // Переменная для вычисления промежутка паузы.
        TimeSpan delta = TimeSpan.Zero;

        /// <summary>
        /// Метод для получения информации о результатах.
        /// </summary>
        public void showInfo()
        {
            TimeSpan max = TimeSpan.Zero;
            TimeSpan min = results[0];
            TimeSpan sum = max;
            foreach (TimeSpan i in results)
            {
                if (i >= max)
                    max = i;
                if (i <= min)
                    min = i;
                sum += i;
            }
            MessageBox.Show($"Max = {max}\nMin = {min}\nAverage = {sum / results.Count}");
        }

        /// <summary>
        /// Метод для красивово вывода TimeSpan.
        /// </summary>
        /// <param name="t"> Переменная, котороую надо красиво вывести.</param>
        /// <returns> Красивая строка.</returns>
        public string stringing(TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
                return timeSpan.ToString();
            string[] startStr = timeSpan.ToString().Split('.');
            startStr[1] = startStr[1].Substring(0, 2);
            return string.Join('.', startStr);
        }

        /// <summary>
        /// Метод для добавления в журнал данных о текущем секундомере.
        /// </summary>
        /// <param name="withTime"> TextBox с времене.</param>
        /// <param name="withLaps"> TextBox  с кругами.</param>
        public void addInLog(TextBox withTime, TextBox withLaps)
        {
            try
            {
                File.AppendAllText("log.txt", $"{timeNumber++}. Time:\n{withTime.Text}\n");
                if (withLaps.Text != "")
                    File.AppendAllText("log.txt", $"Laps:\n{withLaps.Text}");
            }
            catch
            {
                MessageBox.Show("Error:\n Could not save new data");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            stop.Visibility = Visibility.Hidden;
            timeBox.Text = TimeSpan.Zero.ToString();
            CompositionTarget.Rendering += (ss, ee) =>
            {
                // Отображение текущего значения секундомера.
                if (!isRunning)
                    return;
                deltaTime = DateTime.Now - tStart - delta;
                timeBox.Text = stringing(deltaTime);
                rotateSecond.Angle = 6 * (deltaTime.TotalSeconds + deltaTime.TotalSeconds / 1000);
            };

            // Перемотка TextBox'a  вниз, если новые значения уже не видны.
            laps.TextChanged += (sender, e) =>
            {
                laps.ScrollToEnd();
            };

            // Удаление файла-журнала.
            this.Closed += (ss, e) =>
            {
                try
                {
                    if (File.Exists("log.txt"))
                        File.Delete("log.txt");
                }
                catch
                {
                    MessageBox.Show("Some troubles happend, but it's ok.\n Dont worry!");
                }
            };
        }

        /// <summary>
        /// Метод для выполнения функий кнопки Старт и Пауза.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void startPause_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                if (startPause.Content.ToString() == "Start")
                {
                    laps.Text = "";
                    tStart = DateTime.Now;
                    stop.Visibility = Visibility.Visible;
                    logs.Visibility = Visibility.Hidden;
                    info.Visibility = Visibility.Hidden;
                }
                else
                {
                    delta += DateTime.Now - tPause;
                    isRunning = false;
                }

                isRunning = true;
                startPause.Content = "Pause";
                stop.Content = "Lap";
            }
            else
            {
                tPause = DateTime.Now;
                isRunning = false;
                tPause = DateTime.Now;
                startPause.Content = "Continue";
                stop.Content = "Stop";
            }
        }

        /// <summary>
        /// Метод для обработки кнопок Стоп и Круг.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void stop_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                results.Add(deltaTime);
                lapNumber = 1;
                logs.Visibility = Visibility.Visible;
                info.Visibility = Visibility.Visible;
                addInLog(timeBox, laps);
                delta = TimeSpan.Zero;
                deltaTime = TimeSpan.Zero;
                timeBox.Text = stringing(deltaTime);
                rotateSecond.Angle = 0;
                isRunning = false;
                startPause.Content = "Start";
                stop.Visibility = Visibility.Hidden;
                laps.Text = "";
                prevLap = TimeSpan.Zero;
            }
            else
            {
                laps.Text += $"{lapNumber++} lap: {stringing(deltaTime)} (+{stringing(deltaTime - prevLap)}) \n";
                prevLap = deltaTime;
            }
        }

        /// <summary>
        /// Метод для вывода журнала.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logs_Click(object sender, RoutedEventArgs e)
        {
            laps.Text = File.ReadAllText("log.txt");
        }

        /// <summary>
        ///  Метод для обработки кнопки Информация.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void info_Click(object sender, RoutedEventArgs e)
        {
            showInfo();
        }
    }
}
