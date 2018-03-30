using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RegressionModels
{
    public class DataSet
    {
        private RegressionModel _RegressionModel;
        private RegressionEstimation _RegressionEstimation;
        private Dictionary<string, double> _Forecast;
        private double[,] _array;
        private double[] _coefficients, Value_X;
        private string _FileName, _ErrorString;
        private int _Rows, _Cols;
        private bool Create_Chart_Model, Create_Chart_Forecast;
        private Exception _Error;
        private Canvas _Chart;

        #region Конструкторы
        
        /// <summary>
        /// Конструктор класса DataSet
        /// </summary>
        public DataSet()
        {
            _Rows = 1;
            _Cols = 1;
            _array = new double[_Rows, _Cols];
            _array[0, 0] = 0;
            _FileName = "";
            _ErrorString = "";
            _Chart = new Canvas();
            Create_Chart_Model = false;
            Create_Chart_Forecast = false;
            _coefficients = new double[1];
            _coefficients[0] = 0;
        }

        #endregion

        #region Свойства

        /// <summary>
        /// Свойство, которое получает и возвращает массив исходных данных
        /// </summary>
        public double[,] Array
        {
            get { return _array; }
            set 
            {
                try
                {
                    _array = value;
                    _Rows = _array.GetLength(0);
                    _Cols = _array.GetLength(1);
                    
                    _ErrorString = "";
                    _Error = null;
                }
                catch (FormatException e)
                {
                    _ErrorString = "Некорректные данные";
                    _Error = e;
                }
                catch (Exception e)
                {
                    _ErrorString = "" + e;
                    _Error = e;
                }
            }
        }

        /// <summary>
        /// Свойство, которое возвращает количество строк массива исходных данных
        /// </summary>
        public int GetRows
        {
            get { return _Rows; }
        }

        /// <summary>
        /// Свойство, которое возвращает количество столбцов массива исходных данных
        /// </summary>
        public int GetCols
        {
            get { return _Cols; }
        }

        /// <summary>
        /// Свойство, которое возвращает полный путь к последнему, использованному в классе, файлу
        /// </summary>
        public string GetFileName
        {
            get { return _FileName; }
        }

        /// <summary>
        /// Свойство, которое возвращает ошибку, возникшую при работе класса. Если ошибки нет, вернет значение NULL
        /// </summary>
        public Exception GetError
        {
            get{ return _Error; }
        }

        /// <summary>
        /// Свойство, которое возвращает ошибку, записанную в строку. В отличии от свойства GetError, при возникновении исключений: FormatException и ArgumentNullException, вернет значения: "Некорректные данные" и "Файл не выбран"
        /// </summary>
        public string GetErrorString
        {
            get { return _ErrorString; }
        }

        #endregion

        #region Методы

        /// <summary>
        /// Метод, отвечающий за загрузку данных из файла с расширением ".txt"
        /// </summary>
        /// <param name="FileName">Полное имя файла, с которым осуществляется работа</param>
        public void OutFile(string FileName)
        {
            _FileName = FileName;
            string Line;
            int count = 0;
            string[] strArr;
            char[] charArr = new char[] { ' ' };
            try
            {
                _FileName = System.IO.Path.ChangeExtension(_FileName, "txt");
                StreamReader File = new StreamReader(_FileName);

                Line = File.ReadLine();
                strArr = Line.Split(charArr);
                _Cols = strArr.Length;
                count += 1;

                while (File.EndOfStream != true)
                {
                    Line = File.ReadLine();
                    if (Line != "")
                        count += 1;
                }
                File.BaseStream.Position = 0;
                _Rows = count;
                _array = new double[_Rows, _Cols];

                count = 0;
                while (File.EndOfStream != true)
                {
                    Line = File.ReadLine();
                    strArr = Line.Split(charArr);
                    for (int i = 0; i < _Cols; i++)
                    {
                        _array[count, i] = Convert.ToDouble(strArr[i].Trim());
                    }
                    count += 1;
                }
                File.Close();
                _ErrorString = "";
                _Error = null;
            }
            catch (ArgumentNullException e)
            {
                _ErrorString = "Файл не выбран";
                _Error = e;
            }
            catch (Exception e)
            {
                _ErrorString = "" + e;
                _Error = e;
            }
        }      

        /// <summary>
        /// Метод, отвечающий за запись данных в файл с расширением ".txt"
        /// </summary>
        /// <param name="FileName">Полное имя файла, с которым осуществляется работа</param>
        public void InFile(string FileName)
        {
            _FileName = FileName;
            if (_ErrorString != "Некорректные данные")
            {
                string Line;
                try
                {
                    _FileName = System.IO.Path.ChangeExtension(_FileName, "txt");
                    StreamWriter File = new StreamWriter(_FileName);

                    for (int i = 0; i < _Rows; i++)
                    {
                        Line = _array[i, 0] + "";
                        for (int j = 1; j < _Cols; j++)
                        {
                            Line += " " + _array[i, j];
                        }
                        File.WriteLine(Line);
                    }
                    File.Close();
                    _ErrorString = "";
                    _Error = null;
                }
                catch (ArgumentNullException e)
                {
                    _ErrorString = "Файл не выбран";
                    _Error = e;
                }
                catch (Exception e)
                {
                    _ErrorString = "" + e;
                    _Error = e;
                }
            }
        }

        /// <summary>
        /// Метод, отвечающий за загрузку данных из файла с расширением ".xml"
        /// </summary>
        /// <param name="FileName">Полное имя файла, с которым осуществляется работа</param>
        public void OutXML(string FileName)
        {
            _FileName = FileName;
            try
            {
                _FileName = System.IO.Path.ChangeExtension(_FileName, "xml");
                int i = 0, j = 0;
                using (XmlTextReader reader = new XmlTextReader(_FileName))
                {
                    if (reader.IsStartElement("Набор_данных") && !reader.IsEmptyElement)
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Количество_строк"))
                                _Rows = Convert.ToInt32(reader.ReadString());
                            else if (reader.IsStartElement("Количество_столбцов"))
                            {
                                _Cols = Convert.ToInt32(reader.ReadString());
                                break;
                            }
                        }

                        _array = new double[_Rows, _Cols];

                        while (reader.Read())
                        {
                            if (reader.IsStartElement("Строка") && !reader.IsEmptyElement)
                            {
                                i = Convert.ToInt32(reader.GetAttribute(0));
                                while (reader.Read())
                                {
                                    if (reader.IsStartElement("Столбец"))
                                    {
                                        j = Convert.ToInt32(reader.GetAttribute("id"));
                                        _array[i, j] = Convert.ToDouble(reader.GetAttribute("Value"));
                                    }
                                    else if (!reader.IsStartElement() && reader.Name == "Строка") break;

                                }
                            }
                            else if (!reader.IsStartElement() && reader.Name == "configuration") break;
                        }
                    }
                }
                _ErrorString = "";
                _Error = null;
            }
            catch (ArgumentNullException e)
            {
                _ErrorString = "Файл не выбран";
                _Error = e;
            }
            catch (Exception e)
            {
                _ErrorString = "" + e;
                _Error = e;
            }
        }

        /// <summary>
        /// Метод, отвечающий за запись данных в файл с расширением ".xml"
        /// </summary>
        /// <param name="FileName">Полное имя файла, с которым осуществляется работа</param>
        public void InXML(string FileName)
        {
            _FileName = FileName;
            if (_ErrorString != "Некорректные данные")
            {
                try
                {
                    _FileName = System.IO.Path.ChangeExtension(_FileName, "xml");
                    using (XmlTextWriter writer = new XmlTextWriter(_FileName, Encoding.GetEncoding(1251)))
                    {
                        writer.Formatting = Formatting.Indented;
                        writer.Indentation = 1;
                        writer.IndentChar = '\t';
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Набор_данных");
                        {
                            writer.WriteElementString("Количество_строк", _Rows + "");
                            writer.WriteElementString("Количество_столбцов", _Cols + "");
                            for (int i = 0; i < _Rows; i++)
                            {
                                writer.WriteStartElement("Строка");
                                writer.WriteAttributeString("id", i + "");
                                for (int j = 0; j < _Cols; j++)
                                {
                                    writer.WriteStartElement("Столбец");
                                    writer.WriteAttributeString("id", j + "");
                                    writer.WriteAttributeString("Value", _array[i, j] + "");
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();
                            }
                        }
                        writer.WriteEndElement();
                    }
                    _ErrorString = "";
                    _Error = null;
                }
                catch (ArgumentNullException e)
                {
                    _ErrorString = "Файл не выбран";
                    _Error = e;
                }
                catch (Exception e)
                {
                    _ErrorString = "" + e;
                    _Error = e;
                }
            }
        }

        /// <summary>
        /// Создание графика от двух переменных
        /// </summary>
        /// <param name="Chart_Width">Ширина элемента Canvas, на котором будет отображаться график</param>
        /// <param name="Chart_Height">Высота элемента Canvas, на котором будет отображаться график</param>
        private void Create_Chart(double Chart_Width, double Chart_Height)
        {
            if (_ErrorString != "Некорректные данные" && _Rows > 0 && _Cols == 2)
            {
                try
                {
                    double _MaxY = _array[0, 0], _MinY = _array[0, 0], _MaxX = _array[0, 1], _MinX = _array[0, 1], TempX, TempY, CoefX, CoefY, _Min = 0;
                    int StepX, StepY, SizeX, SizeY, positionX, positionY, StartX, StartY, EndX, EndY, _Size = 10;

                    if (Chart_Width < 500) Chart_Width = 500;
                    if (Chart_Width > 1300) Chart_Width = 1300;

                    if (Chart_Height < 350) Chart_Height = 350;
                    if (Chart_Height > 750) Chart_Height = 750;

                    _Chart = new Canvas();
                    _Chart.Background = new SolidColorBrush(Colors.Transparent);
                    _Chart.Width = Chart_Width;
                    _Chart.Height = Chart_Height;

                    for (int i = 1; i < _Rows; i++)
                    {
                        if (_MaxY < _array[i, 0]) _MaxY = _array[i, 0];
                        if (_MinY > _array[i, 0]) _MinY = _array[i, 0];
                        if (_MaxX < _array[i, 1]) _MaxX = _array[i, 1];
                        if (_MinX > _array[i, 1]) _MinX = _array[i, 1];
                    }

                    _Min = _MinX;

                    if (_MaxX < 0) { TempX = _MinX * -1; }
                    else if (_MinX > 0) { TempX = _MaxX; }
                    else
                    {
                        if (_MaxX > _MinX * -1) TempX = _MaxX;
                        else TempX = _MinX * -1;
                    }

                    if (_MaxY < 0) { TempY = _MinY * -1; }
                    else if (_MinY > 0) { TempY = _MaxY; }
                    else
                    {
                        if (_MaxY > _MinY * -1) TempY = _MaxY;
                        else TempY = _MinY * -1;
                    }

                    StepX = 0;
                    while (TempX > 10) { TempX = TempX / 10; StepX += 1; }

                    StepY = 0;
                    while (TempY > 10) { TempY = TempY / 10; StepY += 1; }

                    #region Определение четверти

                    if (_MaxX < 0)
                    {
                        if (_MaxY < 0)                          // 3 четверть
                        {
                            CoefX = Convert.ToInt32(Chart_Width * 0.0860);
                            CoefY = Convert.ToInt32(Chart_Height * 0.0846);
                            positionX = Convert.ToInt32(Chart_Width * 0.913);
                            positionY = Convert.ToInt32(Chart_Height * 0.087);
                            _MaxX = Convert.ToInt32(Chart_Width * 0.035);
                            _MinX = Convert.ToInt32(Chart_Width * -0.884);
                            _MaxY = Convert.ToInt32(Chart_Height * 0.058);
                            _MinY = Convert.ToInt32(Chart_Height * -0.885);
                            StartX = -10; EndX = 1;
                            StartY = -10; EndY = 1;
                        }
                        else
                        {
                            if (_MinY < 0)                      // 2 и 3 четверти
                            {
                                CoefX = Convert.ToInt32(Chart_Width * 0.0860);
                                CoefY = (Convert.ToInt32(Chart_Height * 0.0846)) / 2;
                                positionX = Convert.ToInt32(Chart_Width * 0.913);
                                positionY = Convert.ToInt32(Chart_Height * 0.510);
                                _MaxX = Convert.ToInt32(Chart_Width * 0.035);
                                _MinX = Convert.ToInt32(Chart_Width * -0.884);
                                _MaxY = Convert.ToInt32(Chart_Height * 0.471);
                                _MinY = Convert.ToInt32(Chart_Height * -0.471);
                                StartX = -10; EndX = 1;
                                StartY = -10; EndY = 11;
                            }
                            else                                // 2 четверть
                            {
                                CoefX = Convert.ToInt32(Chart_Width * 0.0860);
                                CoefY = Convert.ToInt32(Chart_Height * 0.0846);
                                positionX = Convert.ToInt32(Chart_Width * 0.913);
                                positionY = Convert.ToInt32(Chart_Height * 0.923);
                                _MaxX = Convert.ToInt32(Chart_Width * 0.035);
                                _MinX = Convert.ToInt32(Chart_Width * -0.884);
                                _MaxY = Convert.ToInt32(Chart_Height * 0.885);
                                _MinY = Convert.ToInt32(Chart_Height * -0.058);
                                StartX = -10; EndX = 1;
                                StartY = 0; EndY = 11;
                            }
                        }
                    }
                    else
                    {
                        if (_MinX < 0)
                        {
                            if (_MaxY < 0)                      // 3 и 4 четверти
                            {
                                CoefX = (Convert.ToInt32(Chart_Width * 0.0860)) / 2;
                                CoefY = Convert.ToInt32(Chart_Height * 0.0846);
                                positionX = Convert.ToInt32(Chart_Width * 0.477);
                                positionY = Convert.ToInt32(Chart_Height * 0.087);
                                _MaxX = Convert.ToInt32(Chart_Width * 0.459);
                                _MinX = Convert.ToInt32(Chart_Width * -0.459);
                                _MaxY = Convert.ToInt32(Chart_Height * 0.058);
                                _MinY = Convert.ToInt32(Chart_Height * -0.885);
                                StartX = -10; EndX = 11;
                                StartY = -10; EndY = 1;
                            }
                            else
                            {
                                if (_MinY < 0)                  // 1,2,3,4 четверти
                                {
                                    CoefX = (Convert.ToInt32(Chart_Width * 0.0860)) / 2;
                                    CoefY = (Convert.ToInt32(Chart_Height * 0.0846)) / 2;
                                    positionX = Convert.ToInt32(Chart_Width * 0.477);
                                    positionY = Convert.ToInt32(Chart_Height * 0.510);
                                    _MaxX = Convert.ToInt32(Chart_Width * 0.459);
                                    _MinX = Convert.ToInt32(Chart_Width * -0.459);
                                    _MaxY = Convert.ToInt32(Chart_Height * 0.471);
                                    _MinY = Convert.ToInt32(Chart_Height * -0.471);
                                    StartX = -10; EndX = 11;
                                    StartY = -10; EndY = 11;
                                }
                                else                            // 1 и 2 четверти
                                {
                                    CoefX = (Convert.ToInt32(Chart_Width * 0.0860)) / 2;
                                    CoefY = Convert.ToInt32(Chart_Height * 0.0846);
                                    positionX = Convert.ToInt32(Chart_Width * 0.477);
                                    positionY = Convert.ToInt32(Chart_Height * 0.923);
                                    _MaxX = Convert.ToInt32(Chart_Width * 0.459);
                                    _MinX = Convert.ToInt32(Chart_Width * -0.459);
                                    _MaxY = Convert.ToInt32(Chart_Height * 0.885);
                                    _MinY = Convert.ToInt32(Chart_Height * -0.058);
                                    StartX = -10; EndX = 11;
                                    StartY = 0; EndY = 11;
                                }
                            }
                        }
                        else
                        {
                            if (_MaxY < 0)                      // 4 четверть
                            {
                                CoefX = Convert.ToInt32(Chart_Width * 0.0860);
                                CoefY = Convert.ToInt32(Chart_Height * 0.0846);
                                positionX = Convert.ToInt32(Chart_Width * 0.035);
                                positionY = Convert.ToInt32(Chart_Height * 0.087);
                                _MaxX = Convert.ToInt32(Chart_Width * 0.884);
                                _MinX = Convert.ToInt32(Chart_Width * -0.035);
                                _MaxY = Convert.ToInt32(Chart_Height * 0.058);
                                _MinY = Convert.ToInt32(Chart_Height * -0.885);
                                StartX = 0; EndX = 11;
                                StartY = -10; EndY = 1;
                            }
                            else
                            {
                                if (_MinY < 0)                  // 1 и 4 четверти
                                {
                                    CoefX = Convert.ToInt32(Chart_Width * 0.0860);
                                    CoefY = (Convert.ToInt32(Chart_Height * 0.0846)) / 2;
                                    positionX = Convert.ToInt32(Chart_Width * 0.035);
                                    positionY = Convert.ToInt32(Chart_Height * 0.510);
                                    _MaxX = Convert.ToInt32(Chart_Width * 0.884);
                                    _MinX = Convert.ToInt32(Chart_Width * -0.035);
                                    _MaxY = Convert.ToInt32(Chart_Height * 0.471);
                                    _MinY = Convert.ToInt32(Chart_Height * -0.471);
                                    StartX = 0; EndX = 11;
                                    StartY = -10; EndY = 11;
                                }
                                else                            // 1 четверть
                                {
                                    CoefX = Convert.ToInt32(Chart_Width * 0.0860);
                                    CoefY = Convert.ToInt32(Chart_Height * 0.0846);
                                    positionX = Convert.ToInt32(Chart_Width * 0.035);
                                    positionY = Convert.ToInt32(Chart_Height * 0.923);
                                    _MaxX = Convert.ToInt32(Chart_Width * 0.884);
                                    _MinX = Convert.ToInt32(Chart_Width * -0.035);
                                    _MaxY = Convert.ToInt32(Chart_Height * 0.885);
                                    _MinY = Convert.ToInt32(Chart_Height * -0.058);
                                    StartX = 0; EndX = 11;
                                    StartY = 0; EndY = 11;
                                }
                            }
                        }
                    }

                    #endregion

                    SizeX = Convert.ToInt32(Math.Pow(10.0, Convert.ToDouble(StepX)));
                    SizeY = Convert.ToInt32(Math.Pow(10.0, Convert.ToDouble(StepY)));

                    #region Координатная плоскость

                    Line OX = new Line();
                    OX.X1 = _MinX; OX.Y1 = 0;
                    OX.X2 = _MaxX; OX.Y2 = 0;
                    OX.StrokeThickness = 1;
                    OX.Stroke = new SolidColorBrush(Colors.Black);
                    OX.Fill = new SolidColorBrush(Colors.White);
                    OX.Margin = new Thickness(positionX, positionY, 0, 0);
                    _Chart.Children.Add(OX);

                    Line OXa1 = new Line();
                    OXa1.X1 = _MaxX - 10; OXa1.Y1 = -3;
                    OXa1.X2 = _MaxX; OXa1.Y2 = 0;
                    OXa1.StrokeThickness = 1;
                    OXa1.Stroke = new SolidColorBrush(Colors.Black);
                    OXa1.Fill = new SolidColorBrush(Colors.White);
                    OXa1.Margin = new Thickness(positionX, positionY, 0, 0);
                    _Chart.Children.Add(OXa1);

                    Line OXa2 = new Line();
                    OXa2.X1 = _MaxX - 10; OXa2.Y1 = 3;
                    OXa2.X2 = _MaxX; OXa2.Y2 = 0;
                    OXa2.StrokeThickness = 1;
                    OXa2.Stroke = new SolidColorBrush(Colors.Black);
                    OXa2.Fill = new SolidColorBrush(Colors.White);
                    OXa2.Margin = new Thickness(positionX, positionY, 0, 0);
                    _Chart.Children.Add(OXa2);

                    Line OY = new Line();
                    OY.Y1 = _MinY * -1; OY.X1 = 0;
                    OY.Y2 = _MaxY * -1; OY.X2 = 0;
                    OY.StrokeThickness = 1;
                    OY.Stroke = new SolidColorBrush(Colors.Black);
                    OY.Fill = new SolidColorBrush(Colors.White);
                    OY.Margin = new Thickness(positionX, positionY, 0, 0);
                    _Chart.Children.Add(OY);

                    Line OYa1 = new Line();
                    OYa1.Y1 = (_MaxY - 10) * -1; OYa1.X1 = 3;
                    OYa1.Y2 = (_MaxY) * -1; OYa1.X2 = 0;
                    OYa1.StrokeThickness = 1;
                    OYa1.Stroke = new SolidColorBrush(Colors.Black);
                    OYa1.Fill = new SolidColorBrush(Colors.White);
                    OYa1.Margin = new Thickness(positionX, positionY, 0, 0);
                    _Chart.Children.Add(OYa1);

                    Line OYa2 = new Line();
                    OYa2.Y1 = (_MaxY - 10) * -1; OYa2.X1 = -3;
                    OYa2.Y2 = (_MaxY) * -1; OYa2.X2 = 0;
                    OYa2.StrokeThickness = 1;
                    OYa2.Stroke = new SolidColorBrush(Colors.Black);
                    OYa2.Fill = new SolidColorBrush(Colors.White);
                    OYa2.Margin = new Thickness(positionX, positionY, 0, 0);
                    _Chart.Children.Add(OYa2);

                    Ellipse El1 = new Ellipse();
                    El1.Fill = new SolidColorBrush(Colors.Black);
                    El1.Width = 4;
                    El1.Height = 4;
                    El1.Margin = new Thickness(positionX - 2, positionY - 2, 0, 0);
                    _Chart.Children.Add(El1);

                    Label LX = new Label();
                    LX.Content = "x";
                    LX.Margin = new Thickness(Chart_Width - 30, positionY - 22, 0, 0);
                    _Chart.Children.Add(LX);

                    Label LY = new Label();
                    LY.Content = "y";
                    LY.Margin = new Thickness(positionX - 16, 0, 0, 0);
                    _Chart.Children.Add(LY);

                    for (int i = StartX; i < EndX; i++)
                    {
                        Line XO = new Line();
                        XO.Y1 = 5; XO.X1 = i * CoefX;
                        XO.Y2 = -5; XO.X2 = i * CoefX;
                        XO.StrokeThickness = 1;
                        XO.Stroke = new SolidColorBrush(Colors.Black);
                        XO.Fill = new SolidColorBrush(Colors.White);
                        XO.Margin = new Thickness(positionX, positionY, 0, 0);
                        _Chart.Children.Add(XO);

                        Label LXO = new Label();
                        LXO.Content = "" + Math.Pow(10.0, Convert.ToDouble(StepX)) * i;
                        LXO.Margin = new Thickness(positionX - 8 + CoefX * i, positionY - 2, 0, 0);
                        _Chart.Children.Add(LXO);
                    }

                    for (int i = StartY; i < EndY; i++)
                    {
                        Line YO = new Line();
                        YO.Y1 = -1 * i * CoefY; YO.X1 = -5;
                        YO.Y2 = -1 * i * CoefY; YO.X2 = 5;
                        YO.StrokeThickness = 1;
                        YO.Stroke = new SolidColorBrush(Colors.Black);
                        YO.Fill = new SolidColorBrush(Colors.White);
                        YO.Margin = new Thickness(positionX, positionY, 0, 0);
                        _Chart.Children.Add(YO);

                        Label LYO = new Label();
                        LYO.Content = "" + Math.Pow(10.0, Convert.ToDouble(StepY)) * i;
                        LYO.Margin = new Thickness(positionX + 5, positionY - 15 - CoefY * i, 0, 0);
                        _Chart.Children.Add(LYO);
                    }

                    #endregion

                    for (int i = 0; i < _Rows; i++)
                    {
                        Ellipse El = new Ellipse();
                        El.Fill = new SolidColorBrush(Colors.Black);
                        El.Width = 4;
                        El.Height = 4;
                        El.Margin = new Thickness(positionX - 2 + (_array[i, 1] / SizeX) * CoefX, positionY - 2 - (_array[i, 0] / SizeY) * CoefY, 0, 0);
                        _Chart.Children.Add(El);
                    }

                    if (Create_Chart_Model)
                    {
                        if (_RegressionModel.GetInfoModel["Тип модели"] == "Полулогарифмическая модель")
                        {
                            if (SizeX < 10) _Size = 10;
                            else _Size = SizeX;

                            for (int i = 0; i < 11 * SizeX; i += (_Size / 10))
                            {
                                Line _line = new Line();
                                _line.Y1 = -1 * (Math.Log(1 + i) * _coefficients[1] + _coefficients[0]) * CoefY / SizeY; _line.X1 = (1 + i) * CoefX / SizeX;
                                _line.Y2 = -1 * (Math.Log(2 + i) * _coefficients[1] + _coefficients[0]) * CoefY / SizeY; _line.X2 = (2 + i) * CoefX / SizeX;
                                _line.StrokeThickness = 1;
                                _line.Fill = new SolidColorBrush(Colors.White);
                                _line.Stroke = new SolidColorBrush(Colors.Blue);
                                _line.Margin = new Thickness(positionX, positionY, 0, 0);
                                _Chart.Children.Add(_line);
                            }
                        }
                        else if (_RegressionModel.GetInfoModel["Тип модели"] == "Степенная модель")
                        {
                            for (double i = 0.1; i < 11 * SizeX - _Min; i += (SizeX / 10.0))
                            {
                                Line _line = new Line();
                                
                                _line.Y1 = -1 * (Math.Pow(i, _coefficients[1]) * Math.Pow(2.718281, _coefficients[0])) * CoefY / SizeY;
                                _line.X1 = (i) * CoefX / SizeX;

                                _line.Y2 = -1 * (Math.Pow((i + (SizeX / 10.0)), _coefficients[1]) * Math.Pow(2.718281, _coefficients[0])) * CoefY / SizeY; 
                                _line.X2 = (i + (SizeX / 10.0)) * CoefX / SizeX;
                                
                                _line.StrokeThickness = 1;
                                _line.Fill = new SolidColorBrush(Colors.White);
                                _line.Stroke = new SolidColorBrush(Colors.Blue);
                                _line.Margin = new Thickness(positionX, positionY, 0, 0);
                                _Chart.Children.Add(_line);
                            }
                        }
                        else
                        {
                            int Degrees = 0;
                            double Temp = 0;

                            if (_RegressionModel.GetInfoModel["Тип модели"] == "Адаптивная модель, полином " + _RegressionModel.GetDegreesAdaptiveModel + " степени") Degrees = _RegressionModel.GetDegreesAdaptiveModel;
                            else if (_RegressionModel.GetInfoModel["Тип модели"] == "Параболическая модель") Degrees = 2;
                            else if (_RegressionModel.GetInfoModel["Тип модели"] == "Линейная модель" || _RegressionModel.GetInfoModel["Тип модели"] == "Модель Брауна") Degrees = 1;
                            else return;

                            for (double i = _Min - SizeX; i < (12 * SizeX); i += (SizeX / 10.0))
                            {
                                Line _line = new Line();

                                Temp = 0;
                                for (int j = Degrees; j >= 1; j--)
                                {
                                    Temp = Temp + Math.Pow((_Min - (SizeX / 10.0) + i), Convert.ToDouble(j)) * _coefficients[j];
                                }
                                Temp = Temp + _coefficients[0];
                                _line.Y1 = -1 * Temp * CoefY / SizeY;
                                _line.X1 = (_Min - (SizeX / 10.0) + i) * CoefX / SizeX;

                                Temp = 0;
                                for (int j = Degrees; j >= 1; j--)
                                {
                                    Temp = Temp + Math.Pow((_Min + i), Convert.ToDouble(j)) * _coefficients[j];
                                }
                                Temp = Temp + _coefficients[0];
                                _line.Y2 = -1 * Temp * CoefY / SizeY; 
                                _line.X2 = (_Min + i) * CoefX / SizeX;

                                _line.StrokeThickness = 1;
                                _line.Fill = new SolidColorBrush(Colors.White);
                                _line.Stroke = new SolidColorBrush(Colors.Blue);
                                _line.Margin = new Thickness(positionX, positionY, 0, 0);
                                _Chart.Children.Add(_line);
                            }
                        }
                    }
                    if (Create_Chart_Forecast)
                    {
                        double Temp = _Forecast["Нижняя граница интервального прогноза"];

                        for (int i = 0; i < 10; i++)
                        {
                            Line _line = new Line();
                            _line.Y1 = -1 * _Forecast["Верхняя граница интервального прогноза"] * CoefY / SizeY;
                            _line.X1 = Value_X[0] * CoefX / SizeX;

                            _line.Y2 = -1 * _Forecast["Нижняя граница интервального прогноза"] * CoefY / SizeY;
                            _line.X2 = Value_X[0] * CoefX / SizeX;

                            _line.StrokeThickness = 1;
                            _line.Fill = new SolidColorBrush(Colors.White);
                            _line.Stroke = new SolidColorBrush(Colors.LimeGreen);
                            _line.Margin = new Thickness(positionX, positionY, 0, 0);
                            _Chart.Children.Add(_line);

                            Temp = _Forecast["Верхняя граница интервального прогноза"]/10;
                        }

                        Line _line1 = new Line();
                        _line1.Y1 = -1 * _Forecast["Нижняя граница интервального прогноза"] * CoefY / SizeY;
                        _line1.X1 = (Value_X[0] - 1) * CoefX / SizeX;
                        _line1.Y2 = -1 * _Forecast["Нижняя граница интервального прогноза"] * CoefY / SizeY;
                        _line1.X2 = (Value_X[0] + 1) * CoefX / SizeX;

                        _line1.StrokeThickness = 1;
                        _line1.Fill = new SolidColorBrush(Colors.White);
                        _line1.Stroke = new SolidColorBrush(Colors.LimeGreen);
                        _line1.Margin = new Thickness(positionX, positionY, 0, 0);
                        _Chart.Children.Add(_line1);

                        Line _line2 = new Line();
                        _line2.Y1 = -1 * _Forecast["Верхняя граница интервального прогноза"] * CoefY / SizeY;
                        _line2.X1 = (Value_X[0] - 1) * CoefX / SizeX;
                        _line2.Y2 = -1 * _Forecast["Верхняя граница интервального прогноза"] * CoefY / SizeY;
                        _line2.X2 = (Value_X[0] + 1) * CoefX / SizeX;

                        _line2.StrokeThickness = 1;
                        _line2.Fill = new SolidColorBrush(Colors.White);
                        _line2.Stroke = new SolidColorBrush(Colors.LimeGreen);
                        _line2.Margin = new Thickness(positionX, positionY, 0, 0);
                        _Chart.Children.Add(_line2);

                        Ellipse PointForecast = new Ellipse();
                        PointForecast.Fill = new SolidColorBrush(Colors.Green);
                        PointForecast.Width = 4;
                        PointForecast.Height = 4;
                        PointForecast.Margin = new Thickness(positionX - 2 + (Value_X[0] / SizeX) * CoefX, positionY - 2 - (_Forecast["Точечный прогноз"] / SizeY) * CoefY, 0, 0);
                        _Chart.Children.Add(PointForecast);
                    }
                    _ErrorString = "";
                    _Error = null;
                }
                catch (Exception e)
                {
                    _ErrorString = "" + e;
                    _Error = e;
                }
            }
            else
            {
                _Chart = new Canvas();
                _Chart.Background = new SolidColorBrush(Colors.Transparent);
                _Chart.Width = Chart_Width;
                _Chart.Height = Chart_Height;
            }
        }

        /// <summary>
        /// Метод, возвращающий построенный график со стандартными размерами (860x520)
        /// </summary>
        /// <returns>Canvas, на котором изображен график</returns>
        public Canvas Get_Chart_DataSet()
        {
            double Chart_Width = 860, Chart_Height = 520;
            Create_Chart(Chart_Width, Chart_Height);
            return _Chart;
        }

        /// <summary>
        /// Метод, возвращающий построенный график с заданными размерами
        /// </summary>
        /// <param name="Chart_Width">Ширина элемента Canvas, на котором будет отображаться график</param>
        /// <param name="Chart_Height">Высота элемента Canvas, на котором будет отображаться график</param>
        /// <returns>Canvas, на котором изображен график</returns>
        public Canvas Get_Chart_DataSet(double Chart_Width, double Chart_Height)
        {
            Create_Chart(Chart_Width, Chart_Height);
            return _Chart;
        }

        /// <summary>
        /// Метод, возвращающий построенный график на основе исходных данных и модели регрессии. Размеры стандартные (860x520)
        /// </summary>
        /// <param name="RegM">Объект класса RegressionModel, с набором значений</param>
        /// <returns>Canvas, на котором изображен график</returns>
        public Canvas Get_Chart_Model(RegressionModel RegM)
        {
            _RegressionModel = RegM;
            _coefficients = RegM.GetCoefficients;
            double Chart_Width = 860, Chart_Height = 520;
            Create_Chart_Model = true;
            Create_Chart(Chart_Width, Chart_Height);
            Create_Chart_Model = false;
            return _Chart;
        }

        /// <summary>
        /// Метод, возвращающий построенный график на основе исходных данных и модели регрессии.
        /// </summary>
        /// <param name="RegM">Объект класса RegressionModel, с набором значений</param>
        /// <param name="Chart_Width">Ширина элемента Canvas, на котором будет отображаться график</param>
        /// <param name="Chart_Height">Высота элемента Canvas, на котором будет отображаться график</param>
        /// <returns>Canvas, на котором изображен график</returns>
        public Canvas Get_Chart_Model(RegressionModel RegM, double Chart_Width, double Chart_Height)
        {
            _RegressionModel = RegM;
            _coefficients = RegM.GetCoefficients;
            Create_Chart_Model = true;
            Create_Chart(Chart_Width, Chart_Height);
            Create_Chart_Model = false;
            return _Chart;
        }

        /// <summary>
        /// Метод, возвращающий построенный график на основе исходных данных, модели регрессии и построенного прогноза. Размеры стандартные (860x520)
        /// </summary>
        /// <param name="RegM">Объект класса RegressionModel, с набором значений</param>
        /// <param name="RegE">Объект класса RegressionEstimation, с набором значений</param>
        /// <returns>Canvas, на котором изображен график</returns>
        public Canvas Get_Chart_Model_And_Forecast(RegressionModel RegM, RegressionEstimation RegE, double[] Factors)
        {
            _RegressionModel = RegM;
            _RegressionEstimation = RegE;
            Value_X = Factors;
            _Forecast = RegE.Spot_And_Interval_Forecast(Value_X);
            _coefficients = RegM.GetCoefficients;
            double Chart_Width = 860, Chart_Height = 520;
            Create_Chart_Model = true;
            Create_Chart_Forecast = true;
            Create_Chart(Chart_Width, Chart_Height);
            Create_Chart_Forecast = false;
            Create_Chart_Model = false;
            return _Chart;
        }

        /// <summary>
        /// Метод, возвращающий построенный график на основе исходных данных, модели регрессии и построенного прогноза.
        /// </summary>
        /// <param name="RegM">Объект класса RegressionModel, с набором значений</param>
        /// <param name="RegE">Объект класса RegressionEstimation, с набором значений</param>
        /// <param name="Chart_Width">Ширина элемента Canvas, на котором будет отображаться график</param>
        /// <param name="Chart_Height">Высота элемента Canvas, на котором будет отображаться график</param>
        /// <returns>Canvas, на котором изображен график</returns>
        public Canvas Get_Chart_Model_And_Forecast(RegressionModel RegM, RegressionEstimation RegE, double[] Factors, double Chart_Width, double Chart_Height)
        {
            _RegressionModel = RegM;
            _RegressionEstimation = RegE;
            Value_X = Factors;
            _Forecast = RegE.Spot_And_Interval_Forecast(Value_X);
            _coefficients = RegM.GetCoefficients;
            Create_Chart_Model = true;
            Create_Chart_Forecast = true;
            Create_Chart(Chart_Width, Chart_Height);
            Create_Chart_Forecast = false;
            Create_Chart_Model = false;
            return _Chart;
        }

        /// <summary>
        /// Метод, отвечающий за сохранение графика
        /// </summary>
        /// <param name="CanvasChart">График, который необходимо сохранить</param>
        /// <param name="FileName">Полное имя файла, с которым осуществляется работа</param>
        public void Save_Chart(Canvas CanvasChart, string FileName)
        {
            _FileName = FileName;
            _FileName = System.IO.Path.ChangeExtension(_FileName, "png");
            _Chart = CanvasChart;

            if (_ErrorString != "Некорректные данные")
            {
                try
                {
                    RenderTargetBitmap PNG = new RenderTargetBitmap((int)_Chart.Width, (int)_Chart.Height, 96d, 96d, PixelFormats.Pbgra32);
                    _Chart.Measure(new Size((int)_Chart.Width, (int)_Chart.Height));
                    _Chart.Arrange(new Rect(new Size((int)_Chart.Width, (int)_Chart.Height)));
                    PNG.Render(_Chart);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(PNG));
                    using (FileStream file = File.Create(_FileName))
                    {
                        encoder.Save(file);
                    }
                    _ErrorString = "";
                    _Error = null;
                }
                catch (ArgumentNullException e)
                {
                    _ErrorString = "Файл не выбран";
                    _Error = e;
                }
                catch (Exception e)
                {
                    _ErrorString = "" + e;
                    _Error = e;
                }
            }
        }

        #endregion
    }

    public class RegressionModel
    {
        private DataSet _DataSet;
        private string _Model;
        private double[,] ArrayDataSet, _matrixY, _matrixF, _matrixFT, _MultMatrixFT_F, _MultMatrixFT_Y, _ReverseMatrixFT_F;
        private double[] _coefficients, Deviations, Error_Approximation;
        private Exception _Error;
        private double _Det, _SmoothingParameter, _avg_error_approximation;
        private int AmountRows, AmountCols, Degrees;
        private Dictionary<string, string> _InfoModel;
        private bool KeyBrownModel;

        #region Конструкторы

        /// <summary>
        /// Конструктор класса RegressionModel
        /// </summary>
        public RegressionModel()
        {
            _Model = "";
            _Det = 1;
            _InfoModel = new Dictionary<string,string>();
            _coefficients = new double[1];
            _coefficients[0] = 0;
            Deviations = new double[1];
            Deviations[0] = 0;
            _Error = null;
            ArrayDataSet = new double[1, 1];
            ArrayDataSet[0, 0] = 0;
            Degrees = 0;
            KeyBrownModel = false;
            _avg_error_approximation = 0;
        }

        #endregion

        #region Свойства

        /// <summary>
        /// Свойство, которое возвращает скорректированный массив исходных данных
        /// </summary>
        public double[,] GetArray
        {
            get { return ArrayDataSet; }
        }

        /// <summary>
        /// Свойство, которое возвращает степень полинома адаптивной модели
        /// </summary>
        public int GetDegreesAdaptiveModel
        {
            get { return Degrees; }
        }

        /// <summary>
        /// Свойство, которое возвращает ошибку, возникшую при работе класса. Если ошибки нет, вернет значение NULL
        /// </summary>
        public Exception GetError
        {
            get { return _Error; }
        }

        /// <summary>
        /// Свойство, которое возвращает значени коэффициентов в виде одномерного массива, где 0 элемент равен b0 и т.д.
        /// </summary>
        public double[] GetCoefficients
        {
            get { return _coefficients; }
        }

        /// <summary>
        /// Свойство, которое возвращает обратную матрицу XT * X.
        /// </summary>
        public double[,] GetReverseMatrix
        {
            get { return _ReverseMatrixFT_F; }
        }

        /// <summary>
        /// Свойство, которое возвращает значение детерминанта информационной матрицы
        /// </summary>
        public double GetDeterminant
        {
            get { return _Det; }
        }

        /// <summary>
        /// Информация о построенной модели регрессии
        /// </summary>
        public Dictionary<string, string> GetInfoModel
        {
            get { return _InfoModel; }
        }

        /// <summary>
        /// Свойство, которое возвращает значение средней относительной ошибки аппроксимации
        /// </summary>
        public double GetAvgErrorApproximation
        {
            get { return _avg_error_approximation; }
        }

        #endregion

        #region Методы

        /// <summary>
        /// Сортировка пузырьком по первому фактору
        /// </summary>
        /// <param name="TempArray">Массив с данными</param>
        /// <returns>Отсортированный массив</returns>
        private double[,] Bubble_Sort(double[,] TempArray)
        {
            double Temp = 0;

            for (int i = TempArray.GetLength(0) - 1; i > 0; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (TempArray[j, 1] > TempArray[j + 1, 1])
                    {
                        for (int k = 0; k < TempArray.GetLength(1); k++)
                        {
                            Temp = TempArray[j, k];
                            TempArray[j, k] = TempArray[j + 1, k];
                            TempArray[j + 1, k] = Temp;
                        }
                    }
                }
            }

            return TempArray;
        }

        /// <summary>
        /// Линейная модель
        /// </summary>
        /// <param name="data_set">Данные из класса DataSet</param>
        public void BuildLinearModel(DataSet data_set)
        {
            try
            {
                _Model = "";
                _DataSet = data_set;
                AmountRows = _DataSet.GetRows;
                AmountCols = _DataSet.GetCols;
                _Det = 1;
                _InfoModel = new Dictionary<string, string>();

                if (KeyBrownModel)
                {
                    AmountRows = 5;
                    KeyBrownModel = false;
                }
                
                #region Корректировка исходных данных

                ArrayDataSet = new double[AmountRows, AmountCols];

                for (int i = 0; i < AmountRows; i++)
                {
                    for (int j = 0; j < AmountCols; j++)
                    {
                        ArrayDataSet[i, j] = _DataSet.Array[i, j];
                    }
                }

                #endregion

                LeastSquareMethod();

                for (int i = AmountCols - 1; i >= 0; i--)
                {
                    if (_coefficients[i] > 0)
                    {
                        if (i == 0) _Model = _Model + " + " + Convert.ToString(_coefficients[i]);
                        else
                        {
                            if (i != AmountCols - 1) _Model = _Model + " + (" + Convert.ToString(_coefficients[i]) + " * X" + Convert.ToString(i) + ")";
                            else _Model = _Model + " (" + Convert.ToString(_coefficients[i]) + " * X" + Convert.ToString(i) + ")";
                        }
                    }
                    else
                    {
                        if (i == 0) _Model = _Model + " - " + Convert.ToString(_coefficients[i] * -1);
                        else _Model = _Model + " - (" + Convert.ToString(_coefficients[i] * -1) + " * X" + Convert.ToString(i) + ")";
                    }
                }

                _InfoModel["Тип модели"] = "Линейная модель";
                _InfoModel["Модель"] = _Model;
                if (_DataSet.GetCols > 2)
                {
                    _InfoModel["Тип модели по количеству факторов"] = "Множественная линейная регрессия";
                }
                else
                {
                    _InfoModel["Тип модели по количеству факторов"] = "Парная линейная регрессия";

                    if (_coefficients[1] < 0) _InfoModel["Вид зависимости"] = "Отрицательная линейная зависимость";
                    else _InfoModel["Вид зависимости"] = "Положительная линейная зависимость";
                }
                _InfoModel["Успешность построения модели"] = "Модель была построена корректно";

                _Error = null;
            }
            catch (Exception e)
            {
                _Error = e;
            }
        }

        /// <summary>
        /// Полулогарифмической модель
        /// </summary>
        /// <param name="data_set">Данные из класса DataSet</param>
        public void BuildSemilogModel(DataSet data_set)
        {
            int Counter = 0;
            try
            {
                _DataSet = data_set;
                AmountRows = _DataSet.GetRows;
                AmountCols = _DataSet.GetCols;
                _Model = "";
                _InfoModel = new Dictionary<string, string>();
                _Det = 1;

                #region Корректировка исходных данных

                //Подсчет количества строк, с нулевыми и отрицательными элементами
                for (int i = 0; i < AmountRows; i++)
                {
                    for (int j = 1; j < AmountCols; j++)
                    {
                        if (_DataSet.Array[i, j] <= 0)
                        {
                            Counter += 1;
                            break;
                        }
                    }
                }

                //Удаление строк с нулевыми и отрицательными элементами
                if ((AmountRows - Counter) > 0)
                {
                    AmountRows = AmountRows - Counter;
                    ArrayDataSet = new double[AmountRows, AmountCols];
                }

                Counter = 0;

                for (int i = 0; i < AmountRows; i++)
                {
                    for (int j = 0; j < AmountCols; j++)
                    {
                        if (j > 0 && _DataSet.Array[Counter, j] <= 0)
                        {
                            i -= 1;
                            break;
                        }

                        if (j == 0) ArrayDataSet[i, j] = _DataSet.Array[Counter, j];
                        else ArrayDataSet[i, j] = Math.Log(_DataSet.Array[Counter, j]);
                    }
                    Counter += 1;
                }

                #endregion
                
                LeastSquareMethod();

                for (int i = AmountCols - 1; i >= 0; i--)
                {
                    if (_coefficients[i] > 0)
                    {
                        if (i == 0) _Model = _Model + " + " + Convert.ToString(_coefficients[i]);
                        else
                        {
                            if (i != AmountCols - 1) _Model = _Model + " + (" + Convert.ToString(_coefficients[i]) + " * ln(X" + Convert.ToString(i) + "))";
                            else _Model = _Model + " (" + Convert.ToString(_coefficients[i]) + " * ln(X" + Convert.ToString(i) + "))";
                        }
                    }
                    else
                    {
                        if (i == 0) _Model = _Model + " - " + Convert.ToString(_coefficients[i] * -1);
                        else _Model = _Model + " - (" + Convert.ToString(_coefficients[i] * -1) + " * ln(X" + Convert.ToString(i) + "))";
                    }
                }

                _InfoModel["Тип модели"] = "Полулогарифмическая модель";
                _InfoModel["Модель"] = _Model;
                if (_DataSet.GetCols > 2)
                {
                    _InfoModel["Тип модели по количеству факторов"] = "Множественная нелинейная регрессия";
                }
                else
                {
                    _InfoModel["Тип модели по количеству факторов"] = "Парная нелинейная регрессия";

                    if (_coefficients[1] < 0) _InfoModel["Вид зависимости"] = "Отрицательная нелинейная зависимость";
                    else _InfoModel["Вид зависимости"] = "Положительная нелинейная зависимость";
                }
                _InfoModel["Успешность построения модели"] = "Модель была построена корректно";

                _Error = null;
            }
            catch (Exception e)
            {
                _Error = e;
            }
        }

        /// <summary>
        /// Параболическая модель
        /// </summary>
        /// <param name="data_set">Данные из класса DataSet</param>
        public void BuildParabolicModel(DataSet data_set)
        {
            int Degrees = 2;
            try
            {
                _DataSet = data_set;
                AmountRows = _DataSet.GetRows;
                AmountCols = ((_DataSet.GetCols - 1) * 2) + 1;
                _Model = "";
                _InfoModel = new Dictionary<string, string>();
                _Det = 1;

                #region Корректировка исходных данных

                ArrayDataSet = new double[AmountRows, AmountCols];

                for (int i = 0; i < AmountRows; i++)
                {
                    ArrayDataSet[i, 0] = _DataSet.Array[i, 0];

                    for (int j = 1; j < _DataSet.GetCols; j++)
                    {
                        for (int k = 0; k < Degrees; k++)
                        {
                            ArrayDataSet[i, j + k] = Math.Pow(_DataSet.Array[i, j], Convert.ToDouble(k + 1));
                        }
                    }
                }

                #endregion

                LeastSquareMethod();

                for (int i = _DataSet.GetCols - 1; i >= 1; i--)
                {
                    for (int j = Degrees; j >= 1; j--)
                    {
                        if (j > 1)
                        {
                            if (_coefficients[i * j] > 0)
                            {
                                if ((i * j) != AmountCols - 1) _Model = _Model + " + (" + Convert.ToString(_coefficients[i * j]) + " * X" + Convert.ToString(i) + "^" + (j) + ")";
                                else _Model = _Model + " (" + Convert.ToString(_coefficients[i * j]) + " * X" + Convert.ToString(i) + "^" + (j) + ")";
                            }
                            else
                            {
                                _Model = _Model + " - (" + Convert.ToString(_coefficients[i * j] * -1) + " * X" + Convert.ToString(i) + "^" + (j) + ")";
                            }
                        }
                        else
                        {
                            if (_coefficients[i] > 0)
                            {
                                if ((i * j) != AmountCols - 1) _Model = _Model + " + (" + Convert.ToString(_coefficients[i * j]) + " * X" + Convert.ToString(i) + ")";
                                else _Model = _Model + " (" + Convert.ToString(_coefficients[i * j]) + " * X" + Convert.ToString(i) + ")";
                            }
                            else
                            {
                                _Model = _Model + " - (" + Convert.ToString(_coefficients[i * j] * -1) + " * X" + Convert.ToString(i) + ")";
                            }
                        }
                    }
                }
                if (_coefficients[0] > 0) _Model = _Model + " + " + Convert.ToString(_coefficients[0]);
                else _Model = _Model + " - " + Convert.ToString(_coefficients[0] * -1);

                _InfoModel["Тип модели"] = "Параболическая модель";
                _InfoModel["Модель"] = _Model;
                if (_DataSet.GetCols > 2)
                {
                    _InfoModel["Тип модели по количеству факторов"] = "Множественная нелинейная регрессия";
                }
                else
                {
                    _InfoModel["Тип модели по количеству факторов"] = "Парная нелинейная регрессия";

                    if (_coefficients[2] < 0) _InfoModel["Вид зависимости"] = "Отрицательная нелинейная зависимость";
                    else _InfoModel["Вид зависимости"] = "Положительная нелинейная зависимость";
                }
                _InfoModel["Успешность построения модели"] = "Модель была построена корректно";

                _Error = null;
            }
            catch (Exception e)
            {
                _Error = e;
            }
        }

        /// <summary>
        /// Степенная модель
        /// </summary>
        /// <param name="data_set">Данные из класса DataSet</param>
        public void BuildDegreesModel(DataSet data_set)
        {
            int Counter = 0;
            try
            {
                _DataSet = data_set;
                AmountRows = _DataSet.GetRows;
                AmountCols = _DataSet.GetCols;
                _Model = "";
                _InfoModel = new Dictionary<string, string>();
                _Det = 1;

                #region Корректировка исходных данных

                for (int i = 0; i < AmountRows; i++)
                {
                    for (int j = 0; j < AmountCols; j++)
                    {
                        if (_DataSet.Array[i, j] <= 0)
                        {
                            Counter += 1;
                            break;
                        }
                    }
                }

                if ((AmountRows - Counter) > 0)
                {
                    AmountRows = AmountRows - Counter;
                    ArrayDataSet = new double[AmountRows, AmountCols];
                }

                Counter = 0;

                for (int i = 0; i < AmountRows; i++)
                {
                    for (int j = 0; j < AmountCols; j++)
                    {
                        if (_DataSet.Array[Counter, j] <= 0)
                        {
                            i -= 1;
                            break;
                        }

                        ArrayDataSet[i, j] = Math.Log(_DataSet.Array[Counter, j]);
                    }
                    Counter += 1;
                }

                #endregion

                LeastSquareMethod();

                for (int i = 0; i < AmountCols; i++)
                {
                    if (i == 0) _Model = _Model + " " + Convert.ToString(Math.Round((Math.Pow(2.718281, _coefficients[0])), 2));
                    else _Model = _Model + " * (X" + Convert.ToString(i) + "^" + Convert.ToString(_coefficients[i]) + ")";
                }

                _InfoModel["Тип модели"] = "Степенная модель";
                _InfoModel["Модель"] = _Model;
                if (_DataSet.GetCols > 2)
                {
                    _InfoModel["Тип модели по количеству факторов"] = "Множественная нелинейная регрессия";
                }
                else
                {
                    _InfoModel["Тип модели по количеству факторов"] = "Парная нелинейная регрессия";

                    if (_coefficients[1] < 0) _InfoModel["Вид зависимости"] = "Отрицательная нелинейная зависимость";
                    else _InfoModel["Вид зависимости"] = "Положительная нелинейная зависимость";
                }
                _InfoModel["Успешность построения модели"] = "Модель была построена корректно";

                _Error = null;
            }
            catch (Exception e)
            {
                _Error = e;
            }
        }

        /// <summary>
        /// Подсчёт степени для полиномиального уравнения
        /// </summary>
        /// <returns>Степень полиномиального уравнения</returns>
        private int FiniteDifferenceMethod(double[,] TempArrayDataSet)
        {
            bool _Stop = false, _control = false;
            double[,] TempArray;
            int Result = 1;

            TempArray = new double[_DataSet.GetRows, 2];

            for (int i = 1; i < _DataSet.GetRows; i++)
            {
                TempArray[i, 0] = TempArrayDataSet[i, 0] - TempArrayDataSet[i - 1, 0];
            }

            while (_Stop == false)
            {
                for (int i = Result; i < _DataSet.GetRows - 1; i++)
                {
                    if (TempArray[i + 1, 0] != TempArray[i, 0])
                    {
                        _control = true;
                        break;
                    }
                    else _control = false;
                }

                if (_control == false || Result >= _DataSet.GetRows-1)
                {
                    _Stop = true;
                }
                else
                {
                    Result += 1;
                    for (int i = Result; i < _DataSet.GetRows; i++)
                    {
                        TempArray[i, 1] = TempArray[i, 0] - TempArray[i - 1, 0];
                        TempArray[i-1, 0] = TempArray[i-1, 1];
                    }
                    TempArray[_DataSet.GetRows - 1, 0] = TempArray[_DataSet.GetRows - 1, 1];
                    TempArray[Result - 1, 0] = 0;
                    TempArray[Result - 1, 1] = 0;
                }
            }

            return Result;
        }

        /// <summary>
        /// Адаптивная модель
        /// </summary>
        /// <param name="data_set">Данные из класса DataSet</param>
        public void BuildAdaptiveModel(DataSet data_set)
        {
            try
            {
                _DataSet = data_set;

                double[,] TempArrayDataSet = new double[_DataSet.GetRows, _DataSet.GetCols];

                for (int i = 0; i < _DataSet.GetRows; i++)
                {
                    for (int j = 0; j < _DataSet.GetCols; j++)
                    {
                        TempArrayDataSet[i, j] = _DataSet.Array[i, j];
                    }
                }

                TempArrayDataSet = Bubble_Sort(TempArrayDataSet);

                Degrees = FiniteDifferenceMethod(TempArrayDataSet);

                AmountRows = _DataSet.GetRows;
                AmountCols = ((_DataSet.GetCols - 1) * Degrees) + 1;
                _Model = "";
                _InfoModel = new Dictionary<string, string>();
                _Det = 1;

                #region Корректировка исходных данных

                ArrayDataSet = new double[AmountRows, AmountCols];

                for (int i = 0; i < AmountRows; i++)
                {
                    ArrayDataSet[i, 0] = TempArrayDataSet[i, 0];

                    for (int j = 1; j < _DataSet.GetCols; j++)
                    {
                        for (int k = 0; k < Degrees; k++)
                        {
                            ArrayDataSet[i, j + k] = Math.Pow(TempArrayDataSet[i, j], Convert.ToDouble(k + 1));
                        }
                    }
                }

                #endregion

                LeastSquareMethod();

                for (int i = _DataSet.GetCols - 1; i >= 1; i--)
                {
                    for (int j = Degrees; j >= 1; j--)
                    {
                        if (j > 1)
                        {
                            if (_coefficients[i * j] > 0)
                            {
                                if ((i * j) != AmountCols - 1) _Model = _Model + " + (" + Convert.ToString(_coefficients[i * j]) + " * X" + Convert.ToString(i) + "^" + (j) + ")";
                                else _Model = _Model + " (" + Convert.ToString(_coefficients[i * j]) + " * X" + Convert.ToString(i) + "^" + (j) + ")";
                            }
                            else
                            {
                                _Model = _Model + " - (" + Convert.ToString(_coefficients[i * j] * -1) + " * X" + Convert.ToString(i) + "^" + (j) + ")";
                            }
                        }
                        else
                        {
                            if (_coefficients[i] > 0)
                            {
                                if ((i * j) != AmountCols - 1) _Model = _Model + " + (" + Convert.ToString(_coefficients[i * j]) + " * X" + Convert.ToString(i) + ")";
                                else _Model = _Model + " (" + Convert.ToString(_coefficients[i * j]) + " * X" + Convert.ToString(i) + ")";
                            }
                            else
                            {
                                _Model = _Model + " - (" + Convert.ToString(_coefficients[i * j] * -1) + " * X" + Convert.ToString(i) + ")";
                            }
                        }
                    }
                }
                if (_coefficients[0] > 0) _Model = _Model + " + " + Convert.ToString(_coefficients[0]);
                else _Model = _Model + " - " + Convert.ToString(_coefficients[0] * -1);

                _InfoModel["Тип модели"] = "Адаптивная модель, полином " + Degrees + " степени";
                _InfoModel["Модель"] = _Model;
                if (_DataSet.GetCols > 2)
                {
                    if (Degrees < 2) _InfoModel["Тип модели по количеству факторов"] = "Множественная линейная регрессия";
                    else _InfoModel["Тип модели по количеству факторов"] = "Множественная нелинейная регрессия";
                }
                else
                {
                    if (Degrees < 2)
                    {
                        _InfoModel["Тип модели по количеству факторов"] = "Парная линейная регрессия";

                        if (_coefficients[1] < 0) _InfoModel["Вид зависимости"] = "Отрицательная линейная зависимость";
                        else _InfoModel["Вид зависимости"] = "Положительная линейная зависимость";
                    }
                    else
                    {
                        _InfoModel["Тип модели по количеству факторов"] = "Парная нелинейная регрессия";

                        if (_coefficients[Degrees] < 0) _InfoModel["Вид зависимости"] = "Отрицательная нелинейная зависимость";
                        else _InfoModel["Вид зависимости"] = "Положительная нелинейная зависимость";
                    }

                    
                }
                if (Degrees >= _DataSet.GetRows - 1) _InfoModel["Успешность построения модели"] = "Модель была построена некорректно, так как темпы роста неравномерны";
                else _InfoModel["Успешность построения модели"] = "Модель была построена корректно";

                _Error = null;
            }
            catch (Exception e)
            {
                _Error = e;
            }
        }

        /// <summary>
        /// Метод, умножения матриц
        /// </summary>
        /// <param name="MatrixA">Матрица, которая умножается</param>
        /// <param name="MatrixB">Матрица, на которую умножают</param>
        /// <returns>Матрица, полученная в процессе работы метода</returns>
        public double[,] MatrixMultiplication(double[,] MatrixA, double[,] MatrixB)
        {
            int RowA, ColA, RowB, ColB;
            double Multiplication;
            double[,] MatrixC;

            try
            {
                RowA = MatrixA.GetLength(0);
                ColA = MatrixA.GetLength(1);

                RowB = MatrixB.GetLength(0);
                ColB = MatrixB.GetLength(1);

                if (ColA == RowB)
                {
                    MatrixC = new double[RowA, ColB];

                    for (int i = 0; i < RowA; i++)
                    {
                        for (int j = 0; j < ColB; j++)
                        {
                            Multiplication = 0;
                            for (int k = 0; k < ColA; k++)
                            {
                                Multiplication = Multiplication + (MatrixA[i, k] * MatrixB[k, j]);
                            }
                            MatrixC[i, j] = Multiplication;
                        }
                    }
                }
                else
                {
                    MatrixC = new double[1, 1];
                    MatrixC[0, 0] = 0;
                }
                _Error = null;
            }
            catch (Exception e)
            {
                MatrixC = new double[1, 1];
                MatrixC[0, 0] = 0;
                _Error = e;
            }

            return MatrixC;
        }

        /// <summary>
        /// Метод наименьших квадратов
        /// </summary>
        private void LeastSquareMethod()
        {
            try
            {
                int Size;
                double[,] IdentityMatrix;
                double Temp;

                _coefficients = new double[AmountCols];
                _matrixY = new double[AmountRows, 1];
                for (int i = 0; i < AmountRows; i++)
                {
                    _matrixY[i, 0] = ArrayDataSet[i, 0];
                }
                _matrixF = new double[AmountRows, AmountCols];
                for (int i = 0; i < AmountRows; i++)
                {
                    for (int j = 0; j < AmountCols; j++)
                    {
                        if (j == 0) _matrixF[i, j] = 1;
                        else _matrixF[i, j] = ArrayDataSet[i, j];
                    }
                }
                _matrixFT = new double[AmountCols, AmountRows];
                for (int i = 0; i < AmountRows; i++)
                {
                    for (int j = 0; j < AmountCols; j++)
                    {
                        _matrixFT[j, i] = _matrixF[i, j];
                    }
                }

                _MultMatrixFT_F = new double[AmountCols, AmountCols];
                _MultMatrixFT_Y = new double[AmountCols, 1];

                _MultMatrixFT_F = MatrixMultiplication(_matrixFT, _matrixF);
                _MultMatrixFT_Y = MatrixMultiplication(_matrixFT, _matrixY);
                
                _Det = Determinant(_MultMatrixFT_F);
                
                if (_Det != 0)
                {
                    Size = _MultMatrixFT_F.GetLength(0);
                    IdentityMatrix = new double[Size, Size];
                    for (int i = 0; i < Size; i++)
                    {
                        for (int j = 0; j < Size; j++)
                        {
                            if (i == j) IdentityMatrix[i, j] = 1;
                            else IdentityMatrix[i, j] = 0;
                        }
                    }

                    for (int i = 0; i < Size; i++)
                    {
                        for (int j = 0; j < Size; j++)
                        {
                            if ((j == i) && (_MultMatrixFT_F[j, i] != 1))
                            {
                                Temp = _MultMatrixFT_F[j, i];
                                for (int k = 0; k < Size; k++)
                                {
                                    _MultMatrixFT_F[j, k] = _MultMatrixFT_F[j, k] / Temp;
                                    IdentityMatrix[j, k] = IdentityMatrix[j, k] / Temp;
                                }
                                j = -1;
                            }
                            else if ((i != j) && (_MultMatrixFT_F[j, i] != 0) && (_MultMatrixFT_F[i, i] == 1))
                            {
                                Temp = _MultMatrixFT_F[j, i];
                                for (int k = 0; k < Size; k++)
                                {
                                    _MultMatrixFT_F[j, k] = _MultMatrixFT_F[j, k] - (_MultMatrixFT_F[i, k] * Temp);
                                    IdentityMatrix[j, k] = IdentityMatrix[j, k] - (IdentityMatrix[i, k] * Temp);
                                }
                            }
                        }
                    }

                    double[,] _TempArray;
                    _ReverseMatrixFT_F = IdentityMatrix;
                    _TempArray = MatrixMultiplication(IdentityMatrix, _MultMatrixFT_Y);
                    
                    _Model = "Y =";
                    for (int i = AmountCols - 1; i >= 0; i--)
                    {
                        _coefficients[i] = Math.Round(_TempArray[i, 0], 3);
                    }
                }
                _Error = null;
            }
            catch (Exception e)
            {
                _Error = e;
            }
        }

        /// <summary>
        /// Модель Брауна
        /// </summary>
        /// <param name="data_set">Данные из класса DataSet</param>
        /// <param name="SmoothingParameter">Параметр сглаживания</param>
        public void BuildBrownModel_LinaerTrend(DataSet data_set, double SmoothingParameter)
        {
            try
            {
                _DataSet = data_set;
                AmountRows = _DataSet.GetRows;
                AmountCols = _DataSet.GetCols;
                _SmoothingParameter = SmoothingParameter;

                if (AmountRows >= 5 && AmountCols == 2)
                {
                    KeyBrownModel = true;
                    BuildLinearModel(_DataSet);
                    AmountRows = _DataSet.GetRows;

                    _InfoModel["Тип модели"] = "Модель Брауна";
                    
                    #region Корректировка исходных данных

                    ArrayDataSet = new double[AmountRows, AmountCols];

                    for (int i = 0; i < AmountRows; i++)
                    {
                        for (int j = 0; j < AmountCols; j++)
                        {
                            ArrayDataSet[i, j] = _DataSet.Array[i, j];
                        }
                    }

                    #endregion

                    ModelExponentialSmoothing_LinaerTrend();

                    for (int i = 0; i < AmountCols; i++) _coefficients[i] = Math.Round(_coefficients[i], 2);

                    _InfoModel["Модель"] = _coefficients[1] + " * X1 + " + _coefficients[0];
                }
                _Error = null;
            }
            catch (Exception e)
            {
                _Error = e;
            }
        }

        /// <summary>
        /// Построение модели Брауна
        /// </summary>
        private void ModelExponentialSmoothing_LinaerTrend()
        {
            double B, Y_Calculated, Temp;
            int LengthNC, i, j, k;
            double[] NewСoefficients;

            LengthNC = _coefficients.Length;
            B = 1 - _SmoothingParameter;

            NewСoefficients = new double[LengthNC];
            Deviations = new double[AmountRows];
            Error_Approximation = new double[AmountRows];
            
            Y_Calculated = 0;
            for (i = 0; i < LengthNC; i++)
            {
                NewСoefficients[i] = _coefficients[i];
                Y_Calculated += NewСoefficients[i];
            }

            _avg_error_approximation = 0;
            for (i = 0; i < AmountRows; i++)
            {
                Deviations[i] = ArrayDataSet[i, 0] - Y_Calculated;
                Deviations[i] = Math.Round(Deviations[i], 4);

                Error_Approximation[i] = Math.Abs(Deviations[i]) / ArrayDataSet[i, 0]*100;
                Error_Approximation[i] = Math.Round(Error_Approximation[i], 4);

                _avg_error_approximation += Error_Approximation[i];
                
                for (j = 0; j < LengthNC; j++)
                {
                    Temp = 0;
                    for (k = j; k < LengthNC; k++)
                    {
                        Temp += NewСoefficients[k];
                    }
                    if(j == 0) NewСoefficients[j] = Temp + ((1 - (Math.Pow(B, 2))) * Deviations[i]);
                    else NewСoefficients[j] = Temp + ((Math.Pow(1 - B, 2)) * Deviations[i]);
                }

                Y_Calculated = 0;
                for (j = 0; j < LengthNC; j++) Y_Calculated += NewСoefficients[j];
            }
            _avg_error_approximation = _avg_error_approximation / AmountRows;
            _avg_error_approximation = Math.Round(_avg_error_approximation, 4);

            for (i = 0; i < LengthNC; i++) _coefficients[i] = NewСoefficients[i];
            _coefficients[0] = _coefficients[0] - (_coefficients[1] * (AmountRows - 1));
        }

        /// <summary>
        /// Метод, отвечающий за подсчет определителя матрицы
        /// </summary>
        /// <param name="Array1">Матрица</param>
        /// <returns>Определитель</returns>
        private double Determinant(double[,] Array1)
        {
            try
            {
                int Size, i, j, k, Start = 1, Row = Array1.GetLength(0), Col = Array1.GetLength(1);
                double A = 1, Temp, B = 1;

                double[,] Array = new double[Row, Col];
                for (i = 0; i < Row; i++)
                    for (j = 0; j < Col; j++)
                        Array[i, j] = Array1[i, j];

                if (Array.GetLength(0) == Array.GetLength(1))
                {
                    Size = Array.GetLength(0);

                    for (i = 0; i < Size; i++)
                    {
                        for (j = Start; j < Size; j++)
                        {
                            if (Array[j, i] == 0 && Array[Start - 1, i] == 0) B = 0;
                            else B = Array[j, i] / Array[Start - 1, i];
                            for (k = 0; k < Size; k++)
                            {
                                Temp = Array[Start - 1, k] * B;
                                Array[j, k] = Array[j, k] - Temp;
                            }
                        }
                        Start += 1;
                    }

                    i = 0;
                    j = 0;
                    while (i < Size && j < Size)
                    {
                        A = A * Array[i, j];
                        i += 1;
                        j += 1;
                    }
                    _Error = null;
                    return A;
                }
                else
                {
                    _Error = null;
                    return 0;
                }
            }
            catch (Exception e)
            {
                _Error = e;
                return 0;
            }
        }

        #endregion
    }
    
    public class RegressionEstimation
    {
        private DataSet _DataSet;
        private RegressionModel _RegressionModel;
        private double _Coef_Determination, _error_approximation, _Odj_Coef_Determination, _F_Criterion, _Standard_ECC, _Empirical_Cor_Ratio, _RS, AvgErrorApproximation;
        private double _CoefAutoCor, DW, _EstDispersion, _UnbEstDispersion, _EstStandardDeviation, _CorrelationCoef, _Sy, _Sb, _indSy, _Ttab, _TabDW_d1, _TabDW_d2;
        private double[] _coef_elasticity, _average_value, _Disp_Model_Parameters, _Significance, _Comparative_Evaluation, Sum_Xi_avgXi_Y_avgY, Coef_Pair_Cor, B_Coef, _Private_F_Criterion, _EstErrY, _Variance_Analysis;
        private double[,] ArrayDataSet, _cov_matrix_est, T_test, _ConfidenceIntervals, _F_Distribution, RMS_deviation, _Fkp, Table_Durbin_Watson, Max_and_min_Value_DS;
        private Exception _Error;
        private string[] _Report, _TempReport;
        private bool _availability, _key_FBM;
        private int AmountRows, AmountCols, _NumberDigits;
        private Dictionary<string, double> _DataEstimation;
        private Dictionary<string, double> _Forecast;
        private string[,] Summary_Table;
        private bool _EstimatAutoCor, _Estimat_Fkp;

        #region Конструкторы

        /// <summary>
        /// Конструктор класса RegressionEstimation, со стандартным количеством символов после запятой
        /// </summary>
        public RegressionEstimation()
        {
            _Error = null;
            _availability = false;
            _key_FBM = false;
            _Report = new string[1];
            _Report[0] = "";
            _DataEstimation = new Dictionary<string, double>();
            _NumberDigits = 2;
        }

        /// <summary>
        /// Конструктор класса RegressionEstimation, с указанным количеством символов после запятой
        /// </summary>
        /// <param name="NumberDigits">Количество символов после запятой</param>
        public RegressionEstimation(int NumberDigits)
        {
            _Error = null;
            _availability = false;
            _key_FBM = false;
            _Report = new string[1];
            _Report[0] = "";
            _DataEstimation = new Dictionary<string, double>();
            if (NumberDigits >= 0) _NumberDigits = NumberDigits;
            else _NumberDigits = 0;
        }

        #endregion
        
        #region Свойства

        /// <summary>
        /// Свойство, которое возвращает ошибку, возникшую при работе класса. Если ошибки нет, вернет значение NULL
        /// </summary>
        public Exception GetError
        {
            get { return _Error; }
        }

        /// <summary>
        /// Свойство, которое возвращает оценки модели в виде ассоциативного массива.
        /// </summary>
        public Dictionary<string, double> GetDataEstimation
        {
            get { return _DataEstimation; }
        }

        /// <summary>
        /// Свойство, которое возвращает отчёт и оценку адекватности построенной модели
        /// </summary>
        public string[] GetReport
        {
            get { return _Report; }
        }

        #endregion

        #region Методы

        /// <summary>
        /// Метод, иницализирующий данные для оценки новой модели
        /// </summary>
        /// <param name="data_set">Объект класса DataSet, с набором значений</param>
        /// <param name="regression_model">Объект класса RegressionModel, с набором значений</param>
        public void Estimate(DataSet data_set, RegressionModel regression_model)
        {
            _availability = true;
            try
            {
                _Error = null;
                _Coef_Determination = 0;
                _Odj_Coef_Determination = 0;
                _error_approximation = 0;
                _CorrelationCoef = 0;
                _EstDispersion = 0;
                _UnbEstDispersion = 0;
                _EstStandardDeviation = 0;
                _Standard_ECC = 0;
                _Sy = 0;
                _Ttab = 0;
                _TabDW_d1 = 0;
                _TabDW_d2 = 0;
                _F_Criterion = 0;
                _CoefAutoCor = 0;
                _Empirical_Cor_Ratio = 0;
                DW = 0;
                _DataSet = data_set;
                _RegressionModel = regression_model;

                AmountRows = _RegressionModel.GetArray.GetLength(0);
                AmountCols = _RegressionModel.GetArray.GetLength(1);
                ArrayDataSet = new double[AmountRows, AmountCols];

                for (int i = 0; i < AmountRows; i++)
                {
                    for (int j = 0; j < AmountCols; j++)
                    {
                        ArrayDataSet[i, j] = _RegressionModel.GetArray[i, j];
                    }
                }

                _DataEstimation = new Dictionary<string, double>();
                _coef_elasticity = new double[AmountCols - 1];
                _average_value = new double[AmountCols];
                _cov_matrix_est = new double[AmountCols, AmountCols];
                _Disp_Model_Parameters = new double[AmountCols];
                _Significance = new double[AmountCols];
                _ConfidenceIntervals = new double[AmountCols + 2, 2];
                Coef_Pair_Cor = new double[AmountCols - 1];
                _Comparative_Evaluation = new double[AmountCols - 1];
                Sum_Xi_avgXi_Y_avgY = new double[AmountCols - 1];
                B_Coef = new double[AmountCols - 1];
                _Private_F_Criterion = new double[AmountCols - 1];
                RMS_deviation = new double[2, AmountCols];
                _EstErrY = new double[AmountRows];
                Max_and_min_Value_DS = new double[AmountCols + 1, 2];
                _Variance_Analysis = new double[AmountCols*3+1];
                _Fkp = new double[2, 3];
                T_test = new double[,] 
                {{1, 12.706},{2, 4.303},{3, 3.182},{4, 2.776},{5, 2.571},{6, 2.447},{7, 2.3646},{8, 2.3060},{9, 2.2622},{10, 2.2281},
                {11, 2.201},{12, 2.1788},{13, 2.1604},{14, 2.1448},{15, 2.1315},{16, 2.1199},{17, 2.1098},{18, 2.1009},{19, 2.093},{20, 2.086},
                {21, 2.08},{22, 2.074},{23, 2.069},{24, 2.064},{25, 2.06},{26, 2.056},{27, 2.052},{28, 2.048},{29, 2.045},{30, 2.042},
                {32, 2.0360},{34, 2.0322},{36, 2.0281},{38, 2.0244},{40, 2.0211},{42, 2.018},{44, 2.0154},{46, 2.0129},{48, 2.0106},{50, 2.0086},
                {55, 2.0040},{60, 2.0003},{65, 1.997},{70, 1.9944},{80, 1.9900},{90, 1.9867},{100, 1.9840},{120, 1.9719},{150, 1.9759},{200, 1.9719},
                {250, 1.9695},{300, 1.9679},{400, 1.9659},{500, 1.9640}};
                _F_Distribution = new double[,]
                {{161.4, 199.5, 215.7, 224.6, 230.2, 234.0, 238.9, 243.9, 249.0, 253.3},
                {18.51, 19.00, 19.16, 19.25, 19.30, 19.33, 19.37, 19.41, 19.45, 19.50},
                {10.13, 9.55, 9.28, 9.12, 9.01, 8.94, 8.84, 8.74, 8.64, 8.53},
                {7.71, 6.94, 6.59, 6.39, 6.26, 6.16, 6.04, 5.91, 5.77, 5.63},
                {6.61, 5.79, 5.41, 5.19, 5.05, 4.95, 4.82, 4.68, 4.53, 4.36},
                {5.99, 5.14, 4.76, 4.53, 4.39, 4.28, 4.15, 4.00, 3.84, 3.67},
                {5.59, 4.74, 4.35, 4.12, 3.97, 3.87, 3.73, 3.57, 3.41, 3.23},
                {5.32, 4.46, 4.07, 3.84, 3.69, 3.58, 3.44, 3.28, 3.12, 2.99},
                {5.12, 4.26, 3.86, 3.63, 3.48, 3.37, 3.23, 3.07, 2.90, 2.71},
                {4.96, 4.10, 3.71, 3.48, 3.33, 3.22, 3.07, 2.91, 2.74, 2.54},
                {4.84, 3.98, 3.59, 3.36, 3.20, 3.09, 2.95, 2.79, 2.61, 2.40},
                {4.75, 3.88, 3.49, 3.26, 3.11, 3.00, 2.85, 2.69, 2.50, 2.30},
                {4.67, 3.80, 3.41, 3.18, 3.02, 2.92, 2.77, 2.60, 2.42, 2.21},
                {4.60, 3.74, 3.34, 3.11, 2.96, 2.85, 2.70, 2.53, 2.35, 2.13},
                {4.45, 3.68, 3.29, 3.06, 2.90, 2.79, 2.64, 2.48, 2.29, 2.07},
                {4.41, 3.63, 3.24, 3.01, 2.85, 2.74, 2.59, 2.42, 2.24, 2.01},
                {4.45, 3.59, 3.20, 2.96, 2.81, 2.70, 2.55, 2.38, 2.19, 1.96},
                {4.41, 3.55, 3.16, 2.93, 2.77, 2.66, 2.51, 2.34, 2.15, 1.92},
                {4.38, 3.52, 3.13, 2.90, 2.74, 2.63, 2.48, 2.31, 2.11, 1.88},
                {4.35, 3.49, 3.10, 2.87, 2.71, 2.60, 2.45, 2.28, 2.08, 1.84},
                {4.32, 3.47, 3.07, 2.84, 2.68, 2.57, 2.42, 2.25, 2.05, 1.82},
                {4.30, 3.44, 3.05, 2.82, 2.66, 2.55, 2.40, 2.23, 2.03, 1.78},
                {4.28, 3.42, 3.03, 2.80, 2.64, 2.53, 2.38, 2.20, 2.00, 1.76},
                {4.26, 3.40, 3.01, 2.78, 2.62, 2.51, 2.36, 2.18, 1.98, 1.73},
                {4.24, 3.38, 2.99, 2.76, 2.60, 2.49, 2.34, 2.16, 1.96, 1.71},
                {4.22, 3.37, 2.98, 2.74, 2.59, 2.47, 2.32, 2.15, 1.95, 1.69},
                {4.21, 3.35, 2.96, 2.73, 2.57, 2.46, 2.30, 2.13, 1.93, 1.67},
                {4.19, 3.34, 2.95, 2.71, 2.56, 2.44, 2.29, 2.12, 1.91, 1.65},
                {4.18, 3.33, 2.93, 2.70, 2.54, 2.43, 2.28, 2.10, 1.90, 1.64},
                {4.17, 3.32, 2.92, 2.69, 2.53, 2.42, 2.27, 2.09, 1.89, 1.62},
                {4.00, 3.15, 2.76, 2.52, 2.37, 2.25, 2.10, 1.92, 1.70, 1.39},
                {3.84, 2.99, 2.60, 2.37, 2.21, 2.09, 1.94, 1.75, 1.52, 1.03}};
                Table_Durbin_Watson = new double[,]
                {{15, 1.08, 1.36, 0.95, 1.54, 0.82, 1.75, 0.69, 1.97, 0.56, 2.21},
                {16, 1.10, 1.37, 0.98, 1.54, 0.86, 1.73, 0.74, 1.93, 0.62, 2.15},
                {17, 1.13, 1.38, 1.02, 1.54, 0.90, 1.71, 0.78, 1.90, 0.67, 2.10},
                {18, 1.16, 1.39, 1.05, 1.53, 0.93, 1.69, 0.82, 1.87, 0.71, 2.06},
                {19, 1.18, 1.40, 1.08, 1.53, 0.97, 1.68, 0.86, 1.85, 0.75, 2.02},
                {20, 1.20, 1.41, 1.10, 1.54, 1.00, 1.68, 0.90, 1.83, 0.79, 1.99},
                {21, 1.22, 1.42, 1.13, 1.54, 1.03, 1.67, 0.93, 1.81, 0.83, 1.96},
                {22, 1.24, 1.43, 1.15, 1.54, 1.05, 1.66, 0.96, 1.80, 0.86, 1.94},
                {23, 1.26, 1.44, 1.17, 1.54, 1.08, 1.66, 0.99, 1.79, 0.90, 1.92},
                {24, 1.27, 1.45, 1.19, 1.55, 1.10, 1.66, 1.01, 1.78, 0.93, 1.90},
                {25, 1.29, 1.45, 1.21, 1.55, 1.12, 1.66, 1.04, 1.77, 0.95, 1.89},
                {26, 1.30, 1.46, 1.22, 1.55, 1.14, 1.65, 1.06, 1.76, 0.98, 1.88},
                {27, 1.32, 1.47, 1.24, 1.56, 1.16, 1.65, 1.08, 1.76, 1.01, 1.86},
                {28, 1.33, 1.48, 1.26, 1.56, 1.18, 1.65, 1.10, 1.75, 1.03, 1.85},
                {29, 1.34, 1.48, 1.27, 1.56, 1.20, 1.65, 1.12, 1.74, 1.05, 1.84},
                {30, 1.35, 1.49, 1.28, 1.57, 1.21, 1.65, 1.14, 1.74, 1.07, 1.83},
                {31, 1.36, 1.50, 1.30, 1.57, 1.23, 1.65, 1.16, 1.74, 1.09, 1.83},
                {32, 1.37, 1.50, 1.31, 1.57, 1.24, 1.65, 1.18, 1.73, 1.11, 1.82},
                {33, 1.38, 1.51, 1.32, 1.58, 1.26, 1.65, 1.19, 1.73, 1.13, 1.81},
                {34, 1.39, 1.51, 1.33, 1.58, 1.27, 1.65, 1.21, 1.73, 1.15, 1.81},
                {35, 1.40, 1.52, 1.34, 1.58, 1.28, 1.65, 1.22, 1.73, 1.16, 1.80},
                {36, 1.41, 1.52, 1.35, 1.59, 1.29, 1.65, 1.24, 1.73, 1.18, 1.80},
                {37, 1.42, 1.53, 1.36, 1.59, 1.31, 1.66, 1.25, 1.72, 1.19, 1.80},
                {38, 1.43, 1.54, 1.37, 1.59, 1.32, 1.66, 1.26, 1.72, 1.21, 1.79},
                {39, 1.43, 1.54, 1.38, 1.60, 1.33, 1.66, 1.27, 1.72, 1.22, 1.79},
                {40, 1.44, 1.54, 1.39, 1.60, 1.34, 1.66, 1.29, 1.72, 1.23, 1.79},
                {45, 1.48, 1.57, 1.43, 1.62, 1.38, 1.67, 1.34, 1.72, 1.29, 1.78},
                {50, 1.50, 1.59, 1.46, 1.63, 1.42, 1.67, 1.38, 1.72, 1.34, 1.77},
                {55, 1.53, 1.60, 1.49, 1.64, 1.45, 1.68, 1.41, 1.72, 1.38, 1.77},
                {60, 1.55, 1.62, 1.51, 1.65, 1.48, 1.69, 1.44, 1.73, 1.41, 1.77},
                {65, 1.57, 1.63, 1.54, 1.66, 1.50, 1.70, 1.47, 1.73, 1.44, 1.77},
                {70, 1.58, 1.64, 1.55, 1.67, 1.52, 1.70, 1.49, 1.74, 1.46, 1.77},
                {75, 1.60, 1.65, 1.57, 1.68, 1.54, 1.71, 1.51, 1.74, 1.49, 1.77},
                {80, 1.61, 1.66, 1.59, 1.69, 1.56, 1.72, 1.53, 1.74, 1.51, 1.77},
                {85, 1.62, 1.67, 1.60, 1.70, 1.57, 1.72, 1.55, 1.75, 1.52, 1.77},
                {90, 1.63, 1.68, 1.61, 1.70, 1.59, 1.73, 1.57, 1.75, 1.54, 1.78},
                {95, 1.64, 1.69, 1.62, 1.71, 1.60, 1.73, 1.58, 1.75, 1.56, 1.78},
                {100, 1.65, 1.69, 1.63, 1.72, 1.61, 1.74, 1.59, 1.76, 1.57, 1.78}};

                if (AmountRows > 0 && _key_FBM == false) NewEstimation();
                else if (AmountRows > 0 && _key_FBM == true) NewEstimation_FMSM();
                _Error = null;
            }
            catch (Exception e)
            {
                _Error = e;
            }
        }

        /// <summary>
        /// Метод, оценивающий параметры
        /// </summary>
        private void NewEstimation()
        {
            double A_Error_Approx = 0, _avgErrorY = 0, _Estimated_Y, _Yi_avgY = 0, _Yi_avgY_2 = 0, _t = 0, Temp = 0, k1 = 0, k2 = 0, SumPFC = 0, _ee1 = 0, _ee1_2 = 0;
            int _RowTest = 0, Position_k1 = 0, Position_k2 = 0, n = 0;
            double[] Temp_PFC = new double[AmountCols - 1];

            try
            {
                Temp = Math.Sqrt(Convert.ToDouble(AmountRows));
                if (Temp == 0) _Standard_ECC = 0;
                else _Standard_ECC = 1 / Temp;
                _Standard_ECC = Math.Round(_Standard_ECC, _NumberDigits);

                if (_DataSet.GetCols == 2)
                {
                    for (int i = 0; i < AmountCols * 2 + 1; i++) _Variance_Analysis[i] = 0;

                    for (int j = 0; j < AmountRows; j++)
                    {
                        for (int i = 0; i < AmountCols; i++)
                        {
                            _Variance_Analysis[i] = _Variance_Analysis[i] + ArrayDataSet[j, i];
                            _Variance_Analysis[i + AmountCols + 1] = _Variance_Analysis[i + AmountCols + 1] + (ArrayDataSet[j, i] * ArrayDataSet[j, i]);
                        }

                        _Variance_Analysis[AmountCols] = _Variance_Analysis[AmountCols] + (ArrayDataSet[j, 0] * ArrayDataSet[j, 1]);

                        if (j == AmountRows - 1)
                        {
                            _Variance_Analysis[AmountCols] = _Variance_Analysis[AmountCols] / AmountRows;
                            _Variance_Analysis[AmountCols] = Math.Round(_Variance_Analysis[AmountCols], _NumberDigits);

                            for (int i = 0; i < AmountCols; i++)
                            {
                                _Variance_Analysis[i] = _Variance_Analysis[i] / AmountRows;
                                _Variance_Analysis[i] = Math.Round(_Variance_Analysis[i], _NumberDigits);
                                _Variance_Analysis[i + AmountCols + 1] = (_Variance_Analysis[i + AmountCols + 1] / AmountRows) - Math.Pow(_Variance_Analysis[i], 2.0);
                                _Variance_Analysis[i + AmountCols + 1] = Math.Round(_Variance_Analysis[i + AmountCols + 1], _NumberDigits);
                                _Variance_Analysis[i + AmountCols * 2 + 1] = Math.Sqrt(_Variance_Analysis[i + AmountCols + 1]);
                                _Variance_Analysis[i + AmountCols * 2 + 1] = Math.Round(_Variance_Analysis[i + AmountCols * 2 + 1], _NumberDigits);
                            }
                        }
                    }

                    for (int i = 0; i < AmountCols; i++) _average_value[i] = _Variance_Analysis[i];
                }
                else if (_DataSet.GetCols > 2)
                {
                    // Подсчет средних значений Y и Xi
                    for (int i = 0; i < AmountCols; i++)
                    {
                        _average_value[i] = 0;

                        for (int j = 0; j < AmountRows; j++)
                        {
                            _average_value[i] = _average_value[i] + ArrayDataSet[j, i];
                        }
                        _average_value[i] = _average_value[i] / AmountRows;
                        _average_value[i] = Math.Round(_average_value[i], _NumberDigits);
                    }
                }

                for (int i = 0; i < AmountCols; i++)
                {
                    RMS_deviation[0, i] = 0;
                    RMS_deviation[1, i] = 0;
                    Max_and_min_Value_DS[i, 0] = ArrayDataSet[0, i];
                    Max_and_min_Value_DS[i, 1] = ArrayDataSet[0, i];
                }
                
                double[] ValueArray = new double[_DataSet.GetCols-1];
                
                // Подсчет коэффициентов эластичности
                for (int i = 1; i < AmountCols; i++)
                {
                    if (_RegressionModel.GetInfoModel["Тип модели"] == "Параболическая модель" || _RegressionModel.GetInfoModel["Тип модели"] == "Адаптивная модель, полином " + _RegressionModel.GetDegreesAdaptiveModel + " степени")
                    {
                        if (i < AmountCols - 1)
                        {
                            ValueArray[0] = _Variance_Analysis[i];
                            Temp = ThePolynomialModel(2, ValueArray);
                            if (Temp != 0) Temp = _average_value[i] / Temp;
                            _coef_elasticity[i - 1] = (_RegressionModel.GetCoefficients[i] + _RegressionModel.GetCoefficients[i + 1] * 2 * _average_value[i]) * (Temp);
                            _coef_elasticity[i - 1] = Math.Round(_coef_elasticity[i - 1], _NumberDigits);
                            i += 1;
                        }
                    }
                    else
                    {
                        if (_average_value[0] != 0) Temp = _average_value[i] / _average_value[0];
                        else Temp = 0;
                        _coef_elasticity[i - 1] = _RegressionModel.GetCoefficients[i] * (Temp);
                        _coef_elasticity[i - 1] = Math.Round(_coef_elasticity[i - 1], _NumberDigits);
                    }
                    Coef_Pair_Cor[i - 1] = 0;
                    _Comparative_Evaluation[i - 1] = 0;
                    Sum_Xi_avgXi_Y_avgY[i - 1] = 0;
                    B_Coef[i - 1] = 0;
                }

                // Подсчет ошибки аппроксимации и дисперсии
                for (int i = 0; i < AmountRows; i++)
                {
                    _Estimated_Y = 0;
                    
                    // Подсчет Yt
                    for (int j = 1; j < AmountCols; j++)
                    {
                        _Estimated_Y = _Estimated_Y + _RegressionModel.GetCoefficients[j] * ArrayDataSet[i, j];
                    }
                    _Estimated_Y = _Estimated_Y + _RegressionModel.GetCoefficients[0];

                    // Подсчет Y - Yt
                    A_Error_Approx = ArrayDataSet[i, 0] - _Estimated_Y;

                    _EstErrY[i] = A_Error_Approx;
                    _EstDispersion = _EstDispersion + Math.Pow(A_Error_Approx, 2.0);
                    if (ArrayDataSet[i, 0] != 0)
                    {
                        Temp = ArrayDataSet[i, 0];
                        if (Temp != 0) Temp = A_Error_Approx / Temp;
                        A_Error_Approx = Temp;
                    }
                    else A_Error_Approx = 0;
                    _Yi_avgY = ArrayDataSet[i, 0] - _average_value[0];
                    _Yi_avgY_2 = _Yi_avgY_2 + Math.Pow(_Yi_avgY, 2.0);

                    if (A_Error_Approx < 0) A_Error_Approx = A_Error_Approx * -1;
                    _avgErrorY = _avgErrorY + A_Error_Approx;

                    for (int j = 0; j < AmountCols; j++)
                    {
                        RMS_deviation[0, j] = RMS_deviation[0, j] + Math.Pow((ArrayDataSet[i, j] - _average_value[j]), 2.0);
                        if (j > 0)
                        {
                            Sum_Xi_avgXi_Y_avgY[j - 1] = Sum_Xi_avgXi_Y_avgY[j - 1] + ((ArrayDataSet[i, j] - _average_value[j]) * (ArrayDataSet[i, 0] - _average_value[0]));
                        }

                        if (Max_and_min_Value_DS[j, 0] < ArrayDataSet[i, j]) Max_and_min_Value_DS[j, 0] = ArrayDataSet[i, j];
                        if (Max_and_min_Value_DS[j, 1] > ArrayDataSet[i, j]) Max_and_min_Value_DS[j, 1] = ArrayDataSet[i, j];
                    }

                    if (i > 0)
                    {
                        _ee1 = _ee1 + (_EstErrY[i] * _EstErrY[i - 1]);
                        _ee1_2 = _ee1_2 + Math.Pow((_EstErrY[i] - _EstErrY[i - 1]), 2.0);
                    }
                    else Max_and_min_Value_DS[AmountCols, 0] = _EstErrY[0];

                    if (Max_and_min_Value_DS[AmountCols, 0] < _EstErrY[i]) Max_and_min_Value_DS[AmountCols, 0] = _EstErrY[i];
                    if (Max_and_min_Value_DS[AmountCols, 1] > _EstErrY[i]) Max_and_min_Value_DS[AmountCols, 1] = _EstErrY[i];
                }

                Temp = _EstDispersion;
                if (Temp != 0) Temp = _ee1 / Temp;
                _CoefAutoCor = Temp;
                _CoefAutoCor = Math.Round(_CoefAutoCor, _NumberDigits);
                Temp = _EstDispersion;
                if (Temp != 0) Temp = _ee1_2 / Temp;
                DW = Temp;
                DW = Math.Round(DW, _NumberDigits);

                _error_approximation = _avgErrorY / AmountRows;
                _error_approximation = _error_approximation * 100;
                _error_approximation = Math.Round(_error_approximation, _NumberDigits);
                _EstDispersion = Math.Round(_EstDispersion, _NumberDigits);

                // Подсчет несмещенной оценки дисперсии
                Temp = (AmountRows - (AmountCols - 1) - 1);
                if (Temp != 0) Temp = _EstDispersion / Temp;
                _UnbEstDispersion = Temp;
                _UnbEstDispersion = Math.Round(_UnbEstDispersion, _NumberDigits);

                // Подсчет RS критерия, проверка нормальности распределения остаточной компоненты
                Temp = Convert.ToDouble(AmountRows - 1);
                if (Temp != 0) Temp = _EstDispersion / Temp;
                if (Temp >= 0) Temp = Math.Sqrt(Temp);
                else Temp = 0;
                if (Temp != 0) Temp = (Max_and_min_Value_DS[AmountCols, 0] - Max_and_min_Value_DS[AmountCols, 1]) / Temp;
                _RS = Temp;
                _RS = Math.Round(_RS, _NumberDigits);

                // Подсчет дисперсии параметров модели 
                for(int i = 0; i < AmountCols; i++)
                {
                    for(int j = 0; j < AmountCols; j++)
                    {
                        _cov_matrix_est[i, j] = _RegressionModel.GetReverseMatrix[i, j] * _UnbEstDispersion;
                    }
                    Temp = _cov_matrix_est[i, i];
                    if (Temp >= 0) Temp = Math.Sqrt(Temp);
                    else Temp = 0;
                    _Disp_Model_Parameters[i] = Temp;
                    _Disp_Model_Parameters[i] = Math.Round(_Disp_Model_Parameters[i], _NumberDigits);

                    Temp = RMS_deviation[0, i] / AmountRows;
                    if (Temp >= 0) Temp = Math.Sqrt(Temp);
                    else Temp = 0;
                    RMS_deviation[1, i] = Temp;
                }

                for(int i = 1; i < AmountCols; i++)
                {
                    n = i - 1;

                    Temp = RMS_deviation[0, i] * RMS_deviation[0, 0];
                    if (Temp >= 0) Temp = Math.Sqrt(Temp);
                    else Temp = 0;
                    if (Temp != 0) Temp = Sum_Xi_avgXi_Y_avgY[n] / Temp;
                    Coef_Pair_Cor[n] = Temp;
                    Temp = RMS_deviation[1, 0];
                    if (Temp != 0) Temp = RMS_deviation[1, i] / Temp;
                    B_Coef[n] = (Temp) * _RegressionModel.GetCoefficients[i];
                    _Comparative_Evaluation[n] = Coef_Pair_Cor[n] * B_Coef[n];

                    Coef_Pair_Cor[n] = Math.Round(Coef_Pair_Cor[n], _NumberDigits);
                    B_Coef[n] = Math.Round(B_Coef[n], _NumberDigits);
                    _Comparative_Evaluation[n] = Math.Round(_Comparative_Evaluation[n], _NumberDigits);
                }

                // Подсчет среднеквадратичного отклонения
                Temp = _UnbEstDispersion;
                if (Temp >= 0) Temp = Math.Sqrt(Temp);
                else Temp = 0;
                _EstStandardDeviation = Temp;
                _EstStandardDeviation = Math.Round(_EstStandardDeviation, _NumberDigits);

                // Подсчет множественного коэффициента корреляции
                Temp = _Yi_avgY_2;
                if (Temp != 0) Temp = 1 - (_EstDispersion / Temp);
                else Temp = 1;
                if (Temp >= 0) Temp = Math.Sqrt(Temp);
                else Temp = 0;
                _CorrelationCoef = Temp;
                _CorrelationCoef = Math.Round(_CorrelationCoef, _NumberDigits);

                // Подсчет эмпирического корреляционного отношения
                Temp = _Yi_avgY_2;
                if (Temp != 0) Temp = (_Yi_avgY_2 - _EstDispersion) / Temp;
                if (Temp >= 0) Temp = Math.Sqrt(Temp);
                else Temp = 0;
                _Empirical_Cor_Ratio = Temp;
                _Empirical_Cor_Ratio = Math.Round(_Empirical_Cor_Ratio, _NumberDigits);

                // Подсчет коэффициента детерминации
                _Coef_Determination = Math.Pow(_CorrelationCoef, 2.0);
                _Coef_Determination = Math.Round(_Coef_Determination, _NumberDigits);

                for (int i = 1; i < AmountCols; i++)
                {
                    n = i - 1;
                    SumPFC = 0;

                    for (int j = 0; j < AmountCols - 1; j++)
                    {
                        Temp_PFC[j] = _Comparative_Evaluation[j];
                        Temp_PFC[n] = 0;
                        SumPFC += Temp_PFC[j];
                    }

                    Temp = 1 - _Coef_Determination;
                    if (Temp != 0) Temp = (_Coef_Determination - SumPFC) / Temp;
                    _Private_F_Criterion[n] = (Temp) * (AmountRows - AmountCols);
                    _Private_F_Criterion[n] = Math.Round(_Private_F_Criterion[n], _NumberDigits);
                }

                // Подсчет среднего значения результативного признака, стандартной ошибки уравнения Sa
                Temp = _RegressionModel.GetReverseMatrix[0, 0];
                if (Temp >= 0) Temp = Math.Sqrt(Temp);
                else Temp = 0;
                _Sy = _EstStandardDeviation * Temp;
                _Sy = Math.Round(_Sy, _NumberDigits);

                // Подсчет индивидуального значения результативного признака
                Temp = 1 + _RegressionModel.GetReverseMatrix[0, 0];
                if (Temp >= 0) Temp = Math.Sqrt(Temp);
                else Temp = 0;
                _indSy = _EstStandardDeviation * Temp;
                _indSy = Math.Round(_indSy, _NumberDigits);

                // Подсчет стандартной ошибки уравнения Sb
                Temp = Math.Sqrt(AmountRows) * _Variance_Analysis[AmountCols * 3];
                if (Temp != 0) Temp = _EstStandardDeviation / Temp;
                _Sb = Temp;
                _Sb = Math.Round(_Sb, _NumberDigits);

                _t = AmountRows - AmountCols;

                // Нахождение значения по таблице Стьюдента для a/2 = 0.025
                _RowTest = T_test.GetLength(0);
                for (int i = 0; i < _RowTest-1; i++)
                {
                    if (T_test[53, 0] <= _t)
                    {
                        _Ttab = T_test[53, 1];
                        break;
                    }

                    if (T_test[i, 0] == _t)
                    {
                        _Ttab = T_test[i, 1];
                        break;
                    }
                    else if (T_test[i, 0] < _t && T_test[i + 1, 0] > _t)
                    {
                        _Ttab = T_test[i, 1];
                        break;
                    }
                }

                // Доверительный интервал с вероятностью 0.95 для среднего значения результативного признака 
                Temp = _Ttab * _Sy;
                _ConfidenceIntervals[0, 0] = _RegressionModel.GetCoefficients[0] - Temp;
                _ConfidenceIntervals[0, 1] = _RegressionModel.GetCoefficients[0] + Temp;

                // Доверительный интервал с вероятностью 0.95 для индивидуального значения результативного признака
                Temp = _Ttab * _indSy;
                _ConfidenceIntervals[1, 0] = _RegressionModel.GetCoefficients[0] - Temp;
                _ConfidenceIntervals[1, 1] = _RegressionModel.GetCoefficients[0] + Temp;

                _ConfidenceIntervals[0, 0] = Math.Round(_ConfidenceIntervals[0, 0], _NumberDigits);
                _ConfidenceIntervals[0, 1] = Math.Round(_ConfidenceIntervals[0, 1], _NumberDigits);
                _ConfidenceIntervals[1, 0] = Math.Round(_ConfidenceIntervals[1, 0], _NumberDigits);
                _ConfidenceIntervals[1, 1] = Math.Round(_ConfidenceIntervals[1, 1], _NumberDigits);

                // Проверка значимости параметров множественного уравнения регрессии
                // Доверительный интервал для коэффициентов уравнения регрессии
                for (int i = 0; i < AmountCols; i++)
                {
                    Temp = _Disp_Model_Parameters[i];
                    if (Temp != 0) Temp = _RegressionModel.GetCoefficients[i] / Temp;
                    _Significance[i] = Temp;
                    _Significance[i] = Math.Round(_Significance[i], _NumberDigits);
                    if (_Significance[i] < 0) _Significance[i] = _Significance[i] * -1;
                    
                    Temp = _Ttab * _Disp_Model_Parameters[i];
                    _ConfidenceIntervals[i + 2, 0] = _RegressionModel.GetCoefficients[i] - Temp;
                    _ConfidenceIntervals[i + 2, 1] = _RegressionModel.GetCoefficients[i] + Temp;

                    _ConfidenceIntervals[i + 2, 0] = Math.Round(_ConfidenceIntervals[i + 2, 0], _NumberDigits);
                    _ConfidenceIntervals[i + 2, 1] = Math.Round(_ConfidenceIntervals[i + 2, 1], _NumberDigits);
                }

                // Подсчет скорректированного коэффициента детерминации
                Temp = Convert.ToDouble(AmountRows - (AmountCols - 1));
                if(Temp != 0) Temp = Convert.ToDouble(AmountRows - 1) / Temp;
                _Odj_Coef_Determination = 1 - ((1 - _Coef_Determination) * Temp);
                _Odj_Coef_Determination = Math.Round(_Odj_Coef_Determination, _NumberDigits);

                // Подсчет F-критерия Фишера
                k1 = Convert.ToDouble(AmountCols);
                k2 = Convert.ToDouble(AmountRows - AmountCols);
                Temp = 1 - _Coef_Determination;
                if (Temp != 0) Temp = _Coef_Determination / Temp;
                _F_Criterion = Temp * (k2 / (k1 - 1));
                _F_Criterion = Math.Round(_F_Criterion, _NumberDigits);

                for (int i = 0; i < 2; i++)
                {
                    k1 = k1 - 1;
                    if (k1 <= 6 && k1 >= 1) Position_k1 = Convert.ToInt32(k1 - 1);
                    else if (k1 == 7) Position_k1 = 5;
                    else if (k1 < 12 && k1 >= 8) Position_k1 = 6;
                    else if (k1 < 24 && k1 >= 12) Position_k1 = 7;
                    else if (k1 == 24) Position_k1 = 8;
                    else Position_k1 = 9;

                    if (k2 >= 1 && k2 <= 30) Position_k2 = Convert.ToInt32(k2 - 1);
                    else if (k2 > 30 && k2 < 60) Position_k2 = 29;
                    else if (k2 == 60) Position_k2 = 30;
                    else Position_k2 = 31;

                    _Fkp[i, 0] = k1;
                    _Fkp[i, 1] = k2;
                    _Fkp[i, 2] = _F_Distribution[Position_k2, Position_k1];
                }
                
                if (AmountCols - 1 > 5)
                {
                    _TabDW_d1 = 1.5;
                    _TabDW_d2 = 1.5;
                }
                else
                {
                    _RowTest = Table_Durbin_Watson.GetLength(0);
                    _TabDW_d1 = ((AmountCols - 1) * 2) - 1;
                    _TabDW_d2 = (AmountCols - 1) * 2;

                    for (int i = 0; i < _RowTest - 1; i++)
                    {
                        if (Table_Durbin_Watson[0, 0] > AmountRows)
                        {
                            _TabDW_d1 = Table_Durbin_Watson[0, Convert.ToInt32(_TabDW_d1)];
                            _TabDW_d2 = Table_Durbin_Watson[0, Convert.ToInt32(_TabDW_d2)];
                            break;
                        }
                        else if (Table_Durbin_Watson[i, 0] == AmountRows)
                        {
                            _TabDW_d1 = Table_Durbin_Watson[i, Convert.ToInt32(_TabDW_d1)];
                            _TabDW_d2 = Table_Durbin_Watson[i, Convert.ToInt32(_TabDW_d2)];
                            break;
                        }
                        else if (AmountRows > Table_Durbin_Watson[i, 0] && AmountRows < Table_Durbin_Watson[i + 1, 0])
                        {
                            _TabDW_d1 = Table_Durbin_Watson[i, Convert.ToInt32(_TabDW_d1)];
                            _TabDW_d2 = Table_Durbin_Watson[i, Convert.ToInt32(_TabDW_d2)];
                            break;
                        }
                        else if (AmountRows > Table_Durbin_Watson[_RowTest-1, 0])
                        {
                            _TabDW_d1 = Table_Durbin_Watson[_RowTest - 1, Convert.ToInt32(_TabDW_d1)];
                            _TabDW_d2 = Table_Durbin_Watson[_RowTest - 1, Convert.ToInt32(_TabDW_d2)];
                            break;
                        }
                    }
                }

                DataEstimationRecording();

                _Error = null;
            }
            catch (Exception e)
            {
                _Report = new string[1];
                _Report[0] = "Ошибка оценки модели.";
                _Error = e;
            }
        }

        /// <summary>
        /// Полулогарифмическое уравнение регрессии
        /// </summary>
        /// <param name="ArrayValue">Массив, содержащий значения факторов</param>
        /// <returns>Значение y от заданный факторов, рассчитанное по данному уравнению</returns>
        private double SemilogModel(double[] ArrayValue)
        {
            double _y = 0;
            int _Length = _DataSet.GetCols - 1;

            try
            {
                for (int j = 0; j < _Length; j++)
                {
                    _y = _y + _RegressionModel.GetCoefficients[j + 1] * Math.Log(ArrayValue[j]);
                }
                _y = _y + _RegressionModel.GetCoefficients[0];
                _Error = null;
            }
            catch (Exception e)
            {
                _y = 0;
                _Error = e;
            }

            return _y;
        }

        /// <summary>
        /// Линейное уравнение регрессии
        /// </summary>
        /// <param name="ArrayValue">Массив, содержащий значения факторов</param>
        /// <returns>Значение y от заданный факторов, рассчитанное по данному уравнению</returns>
        private double DegreesModel(double[] ArrayValue)
        {
            double _y = 1;
            int _Length = _DataSet.GetCols - 1;

            try
            {
                for (int j = 0; j < _Length; j++)
                {
                    _y = _y * Math.Pow(ArrayValue[j], _RegressionModel.GetCoefficients[j + 1]);
                }
                _y = _y * Math.Pow(2.718281, _RegressionModel.GetCoefficients[0]);
                _Error = null;
            }
            catch (Exception e)
            {
                _y = 0;
                _Error = e;
            }

            return _y;
        }

        /// <summary>
        /// Полиномиальное уравнение регрессии степени n
        /// </summary>
        /// <param name="Degrees">Степень полиномиального уравнения</param>
        /// <param name="ArrayValue">Массив, который содержит значения факторов</param>
        /// <returns>Значение зависимой переменной Y от заданных значений факторов, рассчитанное по данному уравнению</returns>
        private double ThePolynomialModel(int Degrees, double[] ArrayValue)
        {
            double _y = 0;
            int _Length = _DataSet.GetCols - 1;

            try
            {
                for (int j = 0; j < _Length; j++)
                {
                    for (int i = 1; i <= Degrees; i++)
                    {
                        _y = _y + _RegressionModel.GetCoefficients[j + i] * Math.Pow(ArrayValue[j], Convert.ToDouble(i));
                    }
                }
                _y = _y + _RegressionModel.GetCoefficients[0];
                _Error = null;
            }
            catch (Exception e)
            {
                _y = 0;
                _Error = e;
            }

            return _y;
        }

        /// <summary>
        /// Метод, оценивающий точечный и интервальный прогноз.
        /// </summary>
        public Dictionary<string, double> Spot_And_Interval_Forecast(double[] Factors)
        {
            int _Length;
            double Spot_Forecast = 0;
            double[,] Operating_Array, Operating_Array_T;
            double Temp = 0;
            _Forecast = new Dictionary<string, double>();

            try
            {
                _Length = _DataSet.GetCols;
                Operating_Array = new double[_Length, 1];
                Operating_Array_T = new double[1, _Length];

                for (int i = 0; i < _Length; i++)
                {
                    if (i == 0)
                    {
                        Operating_Array[i, 0] = 1;
                        Operating_Array_T[0, i] = 1;
                    }
                    else
                    {
                        if (i > Factors.Length - 1)
                        {
                            Operating_Array[i, 0] = 0;
                            Operating_Array_T[0, i] = 0;
                        }
                        else
                        {
                            Operating_Array[i, 0] = Factors[i - 1];
                            Operating_Array_T[0, i] = Factors[i - 1];
                        }
                    }
                }

                Operating_Array_T = _RegressionModel.MatrixMultiplication(Operating_Array_T,_RegressionModel.GetReverseMatrix);
                Operating_Array = _RegressionModel.MatrixMultiplication(Operating_Array_T, Operating_Array);
                
                // Подсчет Yt
                if (_RegressionModel.GetInfoModel["Тип модели"] == "Линейная модель" || _RegressionModel.GetInfoModel["Тип модели"] == "Модель Брауна") Spot_Forecast = ThePolynomialModel(1, Factors);
                else if (_RegressionModel.GetInfoModel["Тип модели"] == "Полулогарифмическая модель") Spot_Forecast = SemilogModel(Factors);
                else if (_RegressionModel.GetInfoModel["Тип модели"] == "Параболическая модель") Spot_Forecast = ThePolynomialModel(2, Factors);
                else if (_RegressionModel.GetInfoModel["Тип модели"] == "Степенная модель") Spot_Forecast = DegreesModel(Factors);
                else if (_RegressionModel.GetInfoModel["Тип модели"] == "Адаптивная модель, полином " + _RegressionModel.GetDegreesAdaptiveModel + " степени") Spot_Forecast = ThePolynomialModel(_RegressionModel.GetDegreesAdaptiveModel, Factors);

                Temp = _EstStandardDeviation * _Ttab * Math.Sqrt(1 + Operating_Array[0, 0]);

                _Forecast["Точечный прогноз"] = Math.Round(Spot_Forecast, _NumberDigits);
                _Forecast["Нижняя граница интервального прогноза"] = Math.Round(Spot_Forecast - Temp, _NumberDigits);
                _Forecast["Верхняя граница интервального прогноза"] = Math.Round(Spot_Forecast + Temp, _NumberDigits);
                _Error = null;
            }
            catch (Exception e)
            {
                _Forecast["Точечный прогноз"] = 0;
                _Forecast["Нижняя граница интервального прогноза"] = 0;
                _Forecast["Верхняя граница интервального прогноза"] = 0;
                _Error = e;
            }

            return _Forecast;
        }

        /// <summary>
        /// Запись данных оценки в ассоциативный массив и формирование отчёта
        /// </summary>
        private void DataEstimationRecording()
        {
            if (_availability)
            {
                double Sum = 0, Temp1 = 0, Temp2 = 0;
                int location = 0, _length = 0, Count = 0, Temp = 0;
                string TempLine = "";
                bool _factors = false, _f = false, _rs = false, _ac = false;

                string[] _ReportTemp;
                _ReportTemp = new string[200];

                _ReportTemp[location] = "Модель: " + _RegressionModel.GetInfoModel["Модель"]; location += 1;
                _ReportTemp[location] = "Тип модели: " + _RegressionModel.GetInfoModel["Тип модели"]; location += 1;
                _ReportTemp[location] = "Тип модели по количеству факторов: " + _RegressionModel.GetInfoModel["Тип модели по количеству факторов"]; location += 1;
                if (_DataSet.GetCols < 3) _ReportTemp[location] = "Вид зависимости: " + _RegressionModel.GetInfoModel["Вид зависимости"]; location += 1;
                _ReportTemp[location] = "Средняя ошибка аппроксимации: " + _error_approximation + "%"; location += 1; _DataEstimation["Средняя ошибка аппроксимации"] = _error_approximation;
                if (_DataSet.GetCols > 2)
                {
                    _ReportTemp[location] = "Оценка дисперсии: " + _EstDispersion; location += 1; _DataEstimation["Оценка дисперсии"] = _EstDispersion;
                    _ReportTemp[location] = "Несмещённая оценка дисперсии: " + _UnbEstDispersion; location += 1; _DataEstimation["Несмещённая оценка дисперсии"] = _UnbEstDispersion;
                    _ReportTemp[location] = "Оценка среднеквадратичного отклонения: " + _EstStandardDeviation; location += 1; _DataEstimation["Оценка среднеквадратичного отклонения"] = _EstStandardDeviation;
                    for (int i = 0; i < _DataSet.GetCols; i++)
                    {
                        _ReportTemp[location] = "Дисперсия коэффициента b" + i + ": " + _Disp_Model_Parameters[i] + " "; 
                        location += 1; 
                        _DataEstimation["Дисперсия коэффициента b" + i] = _Disp_Model_Parameters[i];
                    }
                }
                else
                {
                    _ReportTemp[location] = "Однофакторный дисперсионный анализ"; location += 1;
                    for (int i = 1; i < AmountCols; i++)
                    {
                        if (i == 1)
                        {
                            _ReportTemp[location] = "Среднее значение X: " + _Variance_Analysis[i]; 
                            location += 1;
                            _DataEstimation["Среднее значение X"] = _Variance_Analysis[i];
                        }
                        else
                        {
                            _ReportTemp[location] = "Среднее значение X^" + i + ": " + _Variance_Analysis[i];
                            location += 1;
                            _DataEstimation["Среднее значение X^" + i] = _Variance_Analysis[i];
                        }
                    }
                    _ReportTemp[location] = "Среднее значение Y: " + _Variance_Analysis[0]; location += 1; _DataEstimation["Среднее значение Y"] = _Variance_Analysis[0];
                    _ReportTemp[location] = "Среднее значение X*Y: " + _Variance_Analysis[AmountCols]; location += 1; _DataEstimation["Среднее значение X*Y"] = _Variance_Analysis[AmountCols];
                    for (int i = AmountCols + 2; i < AmountCols * 2 + 1; i++)
                    {
                        if (i == (AmountCols + 2))
                        {
                            _ReportTemp[location] = "Дисперсия D(X): " + _Variance_Analysis[i];
                            location += 1;
                            _DataEstimation["Дисперсия X"] = _Variance_Analysis[i];
                        }
                        else
                        {
                            _ReportTemp[location] = "Дисперсия D(X^" + (i - (AmountCols + 1)) + "): " + _Variance_Analysis[i];
                            location += 1;
                            _DataEstimation["Дисперсия X^" + (i - (AmountCols + 1))] = _Variance_Analysis[i];
                        }
                    }
                    _ReportTemp[location] = "Дисперсия D(Y): " + _Variance_Analysis[AmountCols + 1]; location += 1; _DataEstimation["Дисперсия Y"] = _Variance_Analysis[AmountCols + 1];
                    for (int i = AmountCols * 2 + 2; i < AmountCols * 3 + 1; i++)
                    {
                        if (i == (AmountCols * 2 + 2))
                        {
                            _ReportTemp[location] = "Среднеквадратическое отклонение σ(X): " + _Variance_Analysis[i];
                            location += 1;
                            _DataEstimation["Среднеквадратическое отклонение X"] = _Variance_Analysis[i];
                        }
                        else
                        {
                            _ReportTemp[location] = "Среднеквадратическое отклонение σ(X^" + (i - (AmountCols * 2 + 1)) + "): " + _Variance_Analysis[i];
                            location += 1;
                            _DataEstimation["Среднеквадратическое отклонение X^" + (i - (AmountCols * 2 + 1))] = _Variance_Analysis[i];
                        }
                    }
                    _ReportTemp[location] = "Среднеквадратическое отклонение σ(Y): " + _Variance_Analysis[AmountCols * 2 + 1]; location += 1;
                    _DataEstimation["Среднеквадратическое отклонение Y"] = _Variance_Analysis[AmountCols * 2 + 1];
                }
                for (int i = 1; i < _DataSet.GetCols; i++)
                {
                    _ReportTemp[location] = "Коэффициент эластичности E" + i + ": " + _coef_elasticity[i - 1] + " "; location += 1;
                    _DataEstimation["Коэффициент эластичности E" + i] = _coef_elasticity[i - 1];
                    TempLine = "";
                    if (_coef_elasticity[i - 1] < 1) TempLine = "Влияние X" + i + " на Y не существенно, так как при изменении X" + i + " на 1%, Y изменится менее чем на 1%.";
                    else if (_coef_elasticity[i - 1] == 1) TempLine = "Влияние X" + i + " на Y существенно, так как при изменении X" + i + " на 1%, Y изменится на 1%.";
                    else TempLine = "Влияние X" + i + " на Y существенно, так как при изменении X" + i + " на 1%, Y изменится больше чем на 1%.";
                    _ReportTemp[location] = TempLine; location += 1;
                }
                if (_DataSet.GetCols > 2)
                {
                    _ReportTemp[location] = "Сравнительная оценка влияния анализируемых факторов на результативный признак."; location += 1;
                    for (int i = 1; i < _DataSet.GetCols; i++)
                    {
                        _ReportTemp[location] = "d²" + i + " = " + Coef_Pair_Cor[i - 1] + " * " + B_Coef[i - 1] + " = " + _Comparative_Evaluation[i - 1]; location += 1;
                        Sum = Sum + _Comparative_Evaluation[i - 1];
                    }
                    Sum = Math.Round(Sum, _NumberDigits);
                    if (Sum == _Coef_Determination) _ReportTemp[location] = "∑d²i = R² = " + _Coef_Determination;
                    else
                    {
                        Sum = Math.Round(Sum, _NumberDigits);
                        _Coef_Determination = Math.Round(_Coef_Determination, _NumberDigits);
                        if (Sum == _Coef_Determination) _ReportTemp[location] = "∑d²i = R² = " + _Coef_Determination;
                        else
                        {
                            Sum = Math.Round(Sum, _NumberDigits);
                            _Coef_Determination = Math.Round(_Coef_Determination, _NumberDigits);
                            if (Sum == _Coef_Determination) _ReportTemp[location] = "∑d²i = R² = " + _Coef_Determination;
                            else _ReportTemp[location] = "∑d²i ≠ R²";
                        }
                    }
                    location += 1;
                    _ReportTemp[location] = "Множественный коэффициент корреляции: " + _CorrelationCoef; location += 1;
                    _DataEstimation["Множественный коэффициент корреляции"] = _CorrelationCoef;
                }
                else
                {
                    _ReportTemp[location] = "Эмпирическое корреляционное отношение: " + _Empirical_Cor_Ratio;
                    location += 1;
                    _DataEstimation["Эмпирическое корреляционное отношение"] = _Empirical_Cor_Ratio;
                }

                _ReportTemp[location] = "Коэффициент детерминации: " + _Coef_Determination; location += 1; _DataEstimation["Коэффициент детерминации"] = _Coef_Determination;
                _ReportTemp[location] = "Точность подбора уравнения регрессии: ";
                TempLine = "";
                if (_Coef_Determination >= 0.9) TempLine = "Очень высокая, ";
                else if (_Coef_Determination < 0.9 && _Coef_Determination >= 0.7) TempLine = "Высокая, ";
                else if (_Coef_Determination < 0.7 && _Coef_Determination >= 0.5) TempLine = "Выше средней, ";
                else if (_Coef_Determination < 0.5 && _Coef_Determination >= 0.3) TempLine = "Средняя, ";
                else if (_Coef_Determination < 0.3 && _Coef_Determination >= 0.1) TempLine = "Низкая, ";
                else TempLine = "Нет связи, ";
                _ReportTemp[location] = _ReportTemp[location] + TempLine + (_Coef_Determination * 100) + "%";
                location += 1;

                if (_DataSet.GetCols == 2)
                {
                    _ReportTemp[location] = "Дисперсия ошибки уравнения: " + _UnbEstDispersion; location += 1; _DataEstimation["Дисперсия ошибки уравнения"] = _UnbEstDispersion;
                    _ReportTemp[location] = "Стандартная ошибка уравнения Sy: " + _EstStandardDeviation; location += 1; _DataEstimation["Стандартная ошибка уравнения Sy"] = _EstStandardDeviation;
                    _ReportTemp[location] = "Стандартная ошибка уравнения Sa: " + _Sy; location += 1; _DataEstimation["Стандартная ошибка уравнения Sa"] = _Sy;
                    _ReportTemp[location] = "Стандартная ошибка уравнения Sb: " + _Sb; location += 1; _DataEstimation["Стандартная ошибка уравнения Sb"] = _Sb;
                }
                else if (_DataSet.GetCols >= 2)
                {
                    _ReportTemp[location] = "Среднее значение результативного признака: " + _Sy; location += 1;
                    _DataEstimation["Среднее значение результативного признака"] = _Sy;
                    _ReportTemp[location] = "Доверительный интервал с вероятностью 95% для среднего значения результативного признака: (" + _ConfidenceIntervals[0, 0] + "; " + _ConfidenceIntervals[0, 1] + ")"; location += 1;
                    _DataEstimation["Нижняя граница доверительный интервал для среднего значения результативного признака"] = _ConfidenceIntervals[0, 0];
                    _DataEstimation["Верняя граница доверительный интервал для среднего значения результативного признака"] = _ConfidenceIntervals[0, 1];
                    _ReportTemp[location] = "Индивидуальное значение результативного признака: " + _indSy; location += 1;
                    _DataEstimation["Индивидуальное значение результативного признака"] = _indSy;
                    _ReportTemp[location] = "Доверительный интервал с вероятностью 95% для индивидуального значения результативного признака: (" + _ConfidenceIntervals[1, 0] + "; " + _ConfidenceIntervals[1, 1] + ")"; location += 1;
                    _DataEstimation["Нижняя граница доверительный интервал для индивидуальное значение результативного признака"] = _ConfidenceIntervals[1, 0];
                    _DataEstimation["Верняя граница доверительный интервал для индивидуальное значение результативного признака"] = _ConfidenceIntervals[1, 1];
                }
                _ReportTemp[location] = "Проверка значимости параметров уравнения регрессии"; location += 1;
                Temp = _DataSet.GetRows - _DataSet.GetCols;
                _ReportTemp[location] = "Значение по таблице Стьюдента Tтабл(" + Temp + ";" + 0.025 + ") = " + _Ttab; location += 1; _DataEstimation["Значение по таблице Стьюдента со значимостью 0.025"] = _Ttab;
                for (int i = 0; i < _DataSet.GetCols; i++)
                {
                    _ReportTemp[location] = "Стандартная ошибка коэффициента регрессии b" + i + ": " + _Significance[i]; location += 1; _DataEstimation["Стандартная ошибка коэффициента регрессии b" + i] = _Significance[i];
                    if (_Significance[i] < _Ttab) _ReportTemp[location] = "Статистическая значимость коэффициента регрессии b" + i + " не подтверждается, так как " + _Significance[i] + " < " + _Ttab;
                    else _ReportTemp[location] = "Статистическая значимость коэффициента регрессии b" + i + " подтверждается, так как " + _Significance[i] + " >= " + _Ttab;
                    location += 1;
                }
                _ReportTemp[location] = "Доверительные интервалы для коэффициентов уравнения регрессии."; location += 1;
                for (int i = 0; i < _DataSet.GetCols; i++)
                {
                    _ReportTemp[location] = "Доверительный интервал для b" + i + ": (" + _ConfidenceIntervals[i + 2, 0] + "; " + _ConfidenceIntervals[i + 2, 1] + ")"; location += 1;
                    _DataEstimation["Нижняя граница доверительный интервал для b" + i] = _ConfidenceIntervals[i + 2, 0];
                    _DataEstimation["Верняя граница доверительный интервал для b" + i] = _ConfidenceIntervals[i + 2, 1];
                }
                _ReportTemp[location] = "Скорректированный коэффициент детерминации: " + _Odj_Coef_Determination; location += 1; _DataEstimation["Скорректированный коэффициент детерминации"] = _Odj_Coef_Determination;
                _ReportTemp[location] = "Скорректированная точность подбора уравнения регрессии, с учетом каждого добавленного фактора: ";
                TempLine = "";

                if (_Odj_Coef_Determination >= 0.9) TempLine = "Очень высокая, " + (_Odj_Coef_Determination * 100) + "%";
                else if (_Odj_Coef_Determination < 0.9 && _Odj_Coef_Determination >= 0.7) TempLine = "Высокая, " + (_Odj_Coef_Determination * 100) + "%";
                else if (_Odj_Coef_Determination < 0.7 && _Odj_Coef_Determination >= 0.5) TempLine = "Выше средней, " + (_Odj_Coef_Determination * 100) + "%";
                else if (_Odj_Coef_Determination < 0.5 && _Odj_Coef_Determination >= 0.3) TempLine = "Средняя, " + (_Odj_Coef_Determination * 100) + "%";
                else if (_Odj_Coef_Determination < 0.3 && _Odj_Coef_Determination >= 0.1) TempLine = "Низкая, " + (_Odj_Coef_Determination * 100) + "%";
                else TempLine = "Нет связи, " + (_Odj_Coef_Determination * 100) + "%";
                _ReportTemp[location] = _ReportTemp[location] + TempLine;
                location += 1;

                _ReportTemp[location] = "F-критерий Фишера: " + _F_Criterion; location += 1; _DataEstimation["F-критерий Фишера"] = _F_Criterion;
                _ReportTemp[location] = "Значение по таблице распределения Фишера-Снедоккора Fkp(" + _Fkp[0, 0] + ";" + _Fkp[0, 1] + ") = " + _Fkp[0, 2]; location += 1; _DataEstimation["Значение по таблице распределения Фишера-Снедоккора"] = _Fkp[0, 2];
                if (_F_Criterion < _Fkp[0, 2]) { _ReportTemp[location] = "Так как " + _F_Criterion + " < " + _Fkp[0, 2] + " , то коэффициент детерминации статистически не значим и уравнение регрессии статистически ненадежно."; }
                else { _ReportTemp[location] = "Так как " + _F_Criterion + " >= " + _Fkp[0, 2] + " , то коэффициент детерминации статистически значим и уравнение регрессии статистически надежно."; }
                location += 1;

                _ReportTemp[location] = "Оценка значимости дополнительного включения фактора:"; location += 1;
                _ReportTemp[location] = "Значение по таблице распределения Фишера-Снедоккора Fkp(" + _Fkp[1, 0] + ";" + _Fkp[1, 1] + ") = " + _Fkp[1, 2]; location += 1;
                for (int i = 1; i < _DataSet.GetCols; i++)
                {
                    _ReportTemp[location] = "Fx" + i + " = " + _Private_F_Criterion[i - 1]; location += 1;
                    if (_Private_F_Criterion[i - 1] < _Fkp[1, 2]) _ReportTemp[location] = "Так как " + _Private_F_Criterion[i - 1] + " < " + _Fkp[1, 2] + " , то фактор x" + i + " не целесообразно включать в модель после введения других факторов.";
                    else _ReportTemp[location] = "Так как " + _Private_F_Criterion[i - 1] + " >= " + _Fkp[1, 2] + " , то фактор x" + i + " целесообразно включать в модель после введения других факторов.";
                    location += 1;
                }
                _ReportTemp[location] = "Проверка на наличие автокорреляции остатков."; location += 1;
                _ReportTemp[location] = "Стандартная ошибка коэффициента корреляции: " + _Standard_ECC; location += 1; _DataEstimation["Стандартная ошибка коэффициента корреляции"] = _Standard_ECC;
                _ReportTemp[location] = "Коэффициент автокорреляции: " + _CoefAutoCor; location += 1; _DataEstimation["Коэффициент автокорреляции"] = _CoefAutoCor;
                Temp1 = _Ttab * _Standard_ECC * -1;
                Temp2 = _Ttab * _Standard_ECC;
                Temp1 = Math.Round(Temp1, _NumberDigits);
                Temp2 = Math.Round(Temp2, _NumberDigits);
                if (Temp1 < _CoefAutoCor && _CoefAutoCor < Temp2)
                {
                    _ReportTemp[location] = "Так как " + Temp1 + " < " + _CoefAutoCor + " < " + Temp2 + ", то свойство независимости остатков выполняется, можно считать, что данные не показывают наличие автокорреляции первого порядка. Автокорреляция отсутствует.";
                    _ac = false;
                }
                else if (Temp1 >= _CoefAutoCor)
                {
                    _ReportTemp[location] = "Так как " + Temp1 + " >= " + _CoefAutoCor + ", то свойство независимости остатков не выполняется, можно считать, что данные показывают наличие автокорреляции первого порядка. Автокорреляция присутствует.";
                    _ac = true;
                }
                else if (Temp2 <= _CoefAutoCor)
                {
                    _ReportTemp[location] = "Так как " + Temp2 + " <= " + _CoefAutoCor + ", то свойство независимости остатков не выполняется, можно считать, что данные показывают наличие автокорреляции первого порядка. Автокорреляция присутствует.";
                    _ac = true;
                }
                location += 1;
                _ReportTemp[location] = "Критерий Дарбина-Уотсона: " + DW; location += 1; _DataEstimation["Критерий Дарбина-Уотсона"] = DW;
                Temp1 = 4 - _TabDW_d2;
                _ReportTemp[location] = "Критическое значение d1: " + _TabDW_d1; location += 1; _DataEstimation["Критическое значение d1"] = _TabDW_d1;
                _ReportTemp[location] = "Критическое значение d2: " + _TabDW_d2; location += 1; _DataEstimation["Критическое значение d2"] = _TabDW_d2;
                _ReportTemp[location] = "Критическое значение 4 - d2: " + Temp1; location += 1; _DataEstimation["Критическое значение 4 - d2"] = Temp1;
                if (_TabDW_d1 < DW && _TabDW_d2 < DW && DW < Temp1)
                {
                    _ReportTemp[location] = "Так как " + _TabDW_d1 + " < " + DW + " и " + _TabDW_d2 + " < " + DW + " < " + Temp1 + ", то автокорреляция остатков отсутствует.";
                }
                else
                {
                    _ac = true;
                    if (_TabDW_d1 > DW) _ReportTemp[location] = "Так как " + _TabDW_d1 + " > " + DW;
                    else _ReportTemp[location] = "Так как " + _TabDW_d1 + " < " + DW;

                    if (_TabDW_d2 > DW) _ReportTemp[location] = _ReportTemp[location] + " и " + _TabDW_d2 + " > " + DW;
                    else _ReportTemp[location] = _ReportTemp[location] + " и " + _TabDW_d2 + " < " + DW;

                    if (DW > Temp1) _ReportTemp[location] = _ReportTemp[location] + " > " + Temp1;
                    else _ReportTemp[location] = _ReportTemp[location] + " < " + Temp1;

                    _ReportTemp[location] = _ReportTemp[location] + ", то автокорреляция остатков присутствует.";
                }
                location += 1;
                _ReportTemp[location] = "Проверка нормальности распределения остаточной компоненты"; location += 1;
                _ReportTemp[location] = "RS-критерий: " + _RS; location += 1; _DataEstimation["RS-критерий"] = _RS;
                if (2.7 < _RS && _RS < 3.7) _ReportTemp[location] = "Так как 2.7 < " + _RS + " < 3.7, то выполняется свойство нормального распределения и модель адекватна по нормальности распределения остаточной компоненты.";
                else if (2.7 > _RS) _ReportTemp[location] = "Так как 2.7 > " + _RS + ", то не выполняется свойство нормального распределения и модель неадекватна по нормальности распределения остаточной компоненты.";
                else if (3.7 < _RS) _ReportTemp[location] = "Так как " + _RS + " > 3.7, то не выполняется свойство нормального распределения и модель неадекватна по нормальности распределения остаточной компоненты.";
                location += 1;

                for (int j = 0; j < AmountCols; j++)
                {
                    if(j == 0)
                    {
                        _DataEstimation["Минимальное значение Y"] = Max_and_min_Value_DS[j, 1];
                        _DataEstimation["Максимальное значение Y"] = Max_and_min_Value_DS[j, 0];  
                    }
                    else
                    {
                        _DataEstimation["Минимальное значение X" + j] = Max_and_min_Value_DS[j, 1];
                        _DataEstimation["Максимальное значение X" + j] = Max_and_min_Value_DS[j, 0];  
                    }
                }

                _ReportTemp[location] = "\nОсновные показатели построенной модели:"; location += 1;

                _ReportTemp[location] = "Проверка значимости каждого фактора:"; location += 1;

                for (int i = 1; i < _DataSet.GetCols; i++)
                {
                    if (_RegressionModel.GetCoefficients[i] < 0.0000001 && _RegressionModel.GetCoefficients[i] > -0.0000001) _ReportTemp[location] = "Фактор X" + i + " с коэффициентом " + _RegressionModel.GetCoefficients[i] + " не является значительным, поэтому " + _RegressionModel.GetCoefficients[i] + " * X" + i + " можно убрать из уровнения.";
                    else
                    {
                        _factors = true;
                        _ReportTemp[location] = "Фактор X" + i + " с коэффициентом " + _RegressionModel.GetCoefficients[i] + " является значительным.";
                    }
                    location += 1;
                }

                _ReportTemp[location] = "Точность подбора уравнения регрессии: ";

                if (_Coef_Determination >= 0.9)
                {
                    _ReportTemp[location] = _ReportTemp[location] + "Очень высокая, " + (_Coef_Determination * 100) + "%."; location += 1;
                    _ReportTemp[location] = "Данная модель отлично подходит для прогнозирования, так как включенные факторы влияют на изменение Y в " + (_Coef_Determination * 100) + "% случаев."; location += 1;
                }
                else if (_Coef_Determination < 0.9 && _Coef_Determination >= 0.7)
                {
                    _ReportTemp[location] = _ReportTemp[location] + "Высокая, " + (_Coef_Determination * 100) + "%."; location += 1;
                    _ReportTemp[location] = "Данная модель хорошо подходит для прогнозирования, так как включенные факторы влияют на изменение Y в " + (_Coef_Determination * 100) + "% случаев."; location += 1;
                }
                else if (_Coef_Determination < 0.7 && _Coef_Determination >= 0.5)
                {
                    _ReportTemp[location] = _ReportTemp[location] + "Выше средней, " + (_Coef_Determination * 100) + "%."; location += 1;
                    _ReportTemp[location] = "Данная модель умерено подходит для прогнозирования, так как включенные факторы влияют на изменение Y в " + (_Coef_Determination * 100) + "% случаев."; location += 1;
                }
                else if (_Coef_Determination < 0.5 && _Coef_Determination >= 0.3)
                {
                    _ReportTemp[location] = _ReportTemp[location] + "Средняя, " + (_Coef_Determination * 100) + "%."; location += 1;
                    _ReportTemp[location] = "Данная модель плохо подходит для прогнозирования, так как включенные факторы влияют на изменение Y только в " + (_Coef_Determination * 100) + "% случаев."; location += 1;
                }
                else if (_Coef_Determination < 0.3 && _Coef_Determination >= 0.1)
                {
                    _ReportTemp[location] = _ReportTemp[location] + "Низкая, " + (_Coef_Determination * 100) + "%."; location += 1;
                    _ReportTemp[location] = "Данная модель нежелательная для прогнозирования, так как включенные факторы влияют на изменение Y только в " + (_Coef_Determination * 100) + "% случаев."; location += 1;
                }
                else
                {
                    _ReportTemp[location] = _ReportTemp[location] + "Нет связи, " + (_Coef_Determination * 100) + "%."; location += 1;
                    _ReportTemp[location] = "Данная модель не подходит для прогнозирования, так как включенные факторы влияют на изменение Y только в " + (_Coef_Determination * 100) + "% случаев."; location += 1;
                }

                _ReportTemp[location] = "Проверка значимости всех факторов: "; location += 1;
                if (_F_Criterion < _Fkp[0, 2]) _ReportTemp[location] = "Так как F-критерий Фишера = " + _F_Criterion + " и " + _F_Criterion + " < " + _Fkp[0, 2] + " , то коэффициент детерминации статистически не значим и уравнение регрессии статистически ненадежно.";
                else
                {
                    _f = true;
                    _ReportTemp[location] = "Так как F-критерий Фишера = " + _F_Criterion + " и " + _F_Criterion + " >= " + _Fkp[0, 2] + " , то коэффициент детерминации статистически значим и уравнение регрессии статистически надежно.";
                }
                location += 1;

                _ReportTemp[location] = "Проверка нормальности распределения остаточной компоненты:"; location += 1;
                _ReportTemp[location] = "RS-критерий: " + _RS; location += 1;
                if (2.7 < _RS && _RS < 3.7)
                {
                    _rs = true;
                    _ReportTemp[location] = "Так как 2.7 < " + _RS + " < 3.7, то выполняется свойство нормального распределения и модель адекватна по нормальности распределения остаточной компоненты.";
                }
                else if (2.7 > _RS) _ReportTemp[location] = "Так как 2.7 > " + _RS + ", то не выполняется свойство нормального распределения и модель неадекватна по нормальности распределения остаточной компоненты.";
                else if (3.7 < _RS) _ReportTemp[location] = "Так как " + _RS + " > 3.7, то не выполняется свойство нормального распределения и модель неадекватна по нормальности распределения остаточной компоненты.";
                location += 1;

                _ReportTemp[location] = "Проверка на наличие автокорреляции остатков: ";
                if (_ac == false)
                {
                    _ReportTemp[location] = _ReportTemp[location] + "Автокорреляция отсутствует.";
                }
                else
                {
                    _ReportTemp[location] = _ReportTemp[location] + "Автокорреляция присутствует.";
                }
                location += 1;

                _ReportTemp[location] = "\nЗаключение:"; location += 1;

                if (_RegressionModel.GetInfoModel["Тип модели"] != "Модель Брауна")
                {
                    if (_ac == false)
                    {
                        if (_factors)
                        {
                            if (_Coef_Determination >= 0.5)
                            {
                                if (_f == false)
                                {
                                    _ReportTemp[location] = "Критерий Фишера показывает, что коэффициент детерминации статистически не значим и уравнение регрессии статистически ненадежно."; location += 1;
                                    _ReportTemp[location] = "Данная модель умерено подходит для прогнозирования."; location += 1;
                                }
                                else if (_rs == false)
                                {
                                    _ReportTemp[location] = "Так как не выполняется свойство нормального распределения, то модель неадекватна по нормальности распределения остаточной компоненты."; location += 1;
                                    _ReportTemp[location] = "Данная модель плохо подходит для прогнозирования."; location += 1;
                                }
                                else
                                {
                                    _ReportTemp[location] = "Так как все основные параметры имеют положительную оценку, то данная модель хорошо подходит для прогнозирования."; location += 1;
                                }
                            }
                            else
                            {
                                _ReportTemp[location] = "Так как качество подобранной модели ниже 50%, то прогнозирование по данной модели будет неточным."; location += 1;
                                _ReportTemp[location] = "Данная модель плохо подходит для прогнозирования."; location += 1;
                            }
                        }
                        else
                        {
                            _ReportTemp[location] = "Так как все факторы не значимы, то прогнозирование невозможно."; location += 1;
                            _ReportTemp[location] = "Данная модель не подходит для прогнозирования."; location += 1;
                        }
                    }
                    else
                    {
                        _ReportTemp[location] = "Так как автокорреляция присутствует, то имеет место постоянное однонаправленное воздействие на объясняемую переменную не учтённых в модели факторов. Это влияет на качество оценок модели, делая их неэффективными."; location += 1;
                        _ReportTemp[location] = "Данная модель не подходит для прогнозирования."; location += 1;
                    }
                }
                else
                {
                    AvgErrorApproximation = _RegressionModel.GetAvgErrorApproximation;
                    if (AvgErrorApproximation <= 5)
                    {
                        _ReportTemp[location] = "Так как средняя относительная ошибка аппроксимации равна " + AvgErrorApproximation + " <= 5%."; location += 1;
                        _ReportTemp[location] = "Данная модель хорошо подходит для прогнозирования."; location += 1;
                    }
                    else if (AvgErrorApproximation <= 15)
                    {
                        _ReportTemp[location] = "Так как средняя относительная ошибка аппроксимации равна " + AvgErrorApproximation + " <= 15%."; location += 1;
                        _ReportTemp[location] = "Данная модель подходит для прогнозирования."; location += 1;
                    }
                    else
                    {
                        _ReportTemp[location] = "Так как средняя относительная ошибка аппроксимации равна " + AvgErrorApproximation + " > 15%."; location += 1;
                        _ReportTemp[location] = "Данная модель не подходит для прогнозирования."; location += 1;
                    }
                }

                _length = _ReportTemp.Length;

                for (int i = 0; i < _length; i++)
                {
                    if (string.IsNullOrEmpty(_ReportTemp[i])) Count += 1;
                }

                _Report = new string[_length - Count];
                Count = 0;
                for (int i = 0; i < _length; i++)
                {
                    if (!string.IsNullOrEmpty(_ReportTemp[i]))
                    {
                        _Report[Count] = _ReportTemp[i];
                        Count += 1;
                    }
                }
            }
        }

        /// <summary>
        /// Определение наиболее успешной модели
        /// </summary>
        /// <param name="data_set">Данные из класса DataSet</param>
        /// <param name="RegressionModel">Данные из класса RegressionModel</param>
        private string FindingMostSuitableModel(DataSet data_set, RegressionModel RegressionModel)
        {
            int Count1 = 0, Count2 = 0, Temp = 0, location = 0;
            double TempValue = 0, Temp1 = 0;
            _TempReport = new string[56];
            string[,] Summary_Table_Temp;
            string Result = "";

            try
            {
                _RegressionModel = RegressionModel;
                Summary_Table = new string[5, 4];

                _TempReport[location] = "Основные показатели моделей: "; location += 1;
                _key_FBM = true;

                for (int i = 0; i < 5; i++)
                {
                    switch (i)
                    {
                        case 0: _RegressionModel.BuildLinearModel(data_set); break;
                        case 1: _RegressionModel.BuildSemilogModel(data_set); break;
                        case 2: _RegressionModel.BuildParabolicModel(data_set); break;
                        case 3: _RegressionModel.BuildDegreesModel(data_set); break;
                        case 4: _RegressionModel.BuildAdaptiveModel(data_set); break;
                        default: break;
                    }

                    if (_RegressionModel.GetError != null)
                    {
                        _TempReport[location] = "Тип модели: ";
                        switch (i)
                        {
                            case 0: _TempReport[location] = _TempReport[location] + "Линейная модель"; break;
                            case 1: _TempReport[location] = _TempReport[location] + "Полулогарифмическая модель"; break;
                            case 2: _TempReport[location] = _TempReport[location] + "Параболическая модель"; break;
                            case 3: _TempReport[location] = _TempReport[location] + "Степенная модель"; break;
                            case 4: _TempReport[location] = _TempReport[location] + "Адаптивная модель"; break;
                            default: break;
                        }
                        location += 1;
                        _TempReport[location] = "Набор данных некорректен, пожалуйста, проверьте данные!"; location += 1;
                        _TempReport[location] = "Невозможно построить модель.\n"; location += 1;
                    }
                    else if (_RegressionModel.GetDeterminant == 0)
                    {
                        _TempReport[location] = "Тип модели: " + _RegressionModel.GetInfoModel["Тип модели"]; location += 1;
                        _TempReport[location] = "Для текущего набора данных определитель обратной матрицы равен 0."; location += 1;
                        _TempReport[location] = "Невозможно построить модель.\n"; location += 1;
                    }
                    else
                    {
                        Estimate(data_set, _RegressionModel);

                        Temp1 = 4 - _TabDW_d2;
                        if (_F_Criterion < _Fkp[0, 2]) _Estimat_Fkp = false;
                        else _Estimat_Fkp = true;
                        if (_TabDW_d1 < DW && _TabDW_d2 < DW && DW < Temp1) _EstimatAutoCor = true;
                        else _EstimatAutoCor = false;

                        Summary_Table[i, 0] = Convert.ToString(_RegressionModel.GetInfoModel["Тип модели"]);
                        Summary_Table[i, 1] = Convert.ToString(_Odj_Coef_Determination);
                        Summary_Table[i, 2] = Convert.ToString(_EstimatAutoCor);
                        Summary_Table[i, 3] = Convert.ToString(_Estimat_Fkp);

                        _TempReport[location] = "Тип модели: " + _RegressionModel.GetInfoModel["Тип модели"]; location += 1;
                        _TempReport[location] = "Модель: " + _RegressionModel.GetInfoModel["Модель"]; location += 1;
                        _TempReport[location] = "Скорректированный коэффициент детерминации: " + _Odj_Coef_Determination; location += 1;

                        _TempReport[location] = "F-критерий Фишера: " + _F_Criterion; location += 1;
                        _TempReport[location] = "Значение по таблице распределения Фишера-Снедоккора Fkp(" + _Fkp[0, 0] + ";" + _Fkp[0, 1] + ") = " + _Fkp[0, 2]; location += 1;
                        if (_F_Criterion < _Fkp[0, 2]) _TempReport[location] = "Так как " + _F_Criterion + " < " + _Fkp[0, 2] + " , то коэффициент детерминации статистически не значим и уравнение регрессии статистически ненадежно.";
                        else _TempReport[location] = "Так как " + _F_Criterion + " >= " + _Fkp[0, 2] + " , то коэффициент детерминации статистически значим и уравнение регрессии статистически надежно.";
                        location += 1;

                        _TempReport[location] = "Критерий Дарбина-Уотсона: " + DW; location += 1;
                        
                        _TempReport[location] = "Критическое значение d1: " + _TabDW_d1; location += 1;
                        _TempReport[location] = "Критическое значение d2: " + _TabDW_d2; location += 1;
                        _TempReport[location] = "Критическое значение 4 - d2: " + Temp1; location += 1;
                        if (_TabDW_d1 < DW && _TabDW_d2 < DW && DW < Temp1)
                        {
                            _TempReport[location] = "Так как " + _TabDW_d1 + " < " + DW + " и " + _TabDW_d2 + " < " + DW + " < " + Temp1 + ", то автокорреляция остатков отсутствует.";
                        }
                        else
                        {
                            if (_TabDW_d1 > DW) _TempReport[location] = "Так как " + _TabDW_d1 + " > " + DW;
                            else _TempReport[location] = "Так как " + _TabDW_d1 + " < " + DW;

                            if (_TabDW_d2 > DW) _TempReport[location] = _TempReport[location] + " и " + _TabDW_d2 + " > " + DW;
                            else _TempReport[location] = _TempReport[location] + " и " + _TabDW_d2 + " < " + DW;

                            if (DW > Temp1) _TempReport[location] = _TempReport[location] + " > " + Temp1;
                            else _TempReport[location] = _TempReport[location] + " < " + Temp1;

                            _TempReport[location] = _TempReport[location] + ", то автокорреляция остатков присутствует.";
                        }
                        _TempReport[location] = _TempReport[location] + "\n";

                        location += 1;
                    }
                }

                _key_FBM = false;

                for (int i = 0; i < 5; i++)
                {
                    if (Convert.ToBoolean(Summary_Table[i, 2]) == true) Count1 += 1;
                }

                if (Count1 > 0)
                {
                    Summary_Table_Temp = new string[Count1, 4];

                    for (int i = 0; i < 5; i++)
                    {
                        if (Convert.ToBoolean(Summary_Table[i, 2]) == true)
                        {
                            Summary_Table_Temp[Temp, 0] = Summary_Table[i, 0];
                            Summary_Table_Temp[Temp, 1] = Summary_Table[i, 1];
                            Summary_Table_Temp[Temp, 2] = Summary_Table[i, 2];
                            Summary_Table_Temp[Temp, 3] = Summary_Table[i, 3];
                            Temp += 1;
                        }
                    }

                    Summary_Table = new string[Count1, 4];

                    for (int i = 0; i < Count1; i++)
                    {
                        Summary_Table[i, 0] = Summary_Table_Temp[i, 0];
                        Summary_Table[i, 1] = Summary_Table_Temp[i, 1];
                        Summary_Table[i, 2] = Summary_Table_Temp[i, 2];
                        Summary_Table[i, 3] = Summary_Table_Temp[i, 3];
                    }

                    for (int i = 0; i < Count1; i++)
                    {
                        if (Convert.ToBoolean(Summary_Table[i, 3]) == true) Count2 += 1;
                    }

                    if (Count2 > 0)
                    {
                        Temp = 0;
                        Summary_Table_Temp = new string[Count2, 4];

                        for (int i = 0; i < Count1; i++)
                        {
                            if (Convert.ToBoolean(Summary_Table[i, 3]) == true)
                            {
                                Summary_Table_Temp[Temp, 0] = Summary_Table[i, 0];
                                Summary_Table_Temp[Temp, 1] = Summary_Table[i, 1];
                                Summary_Table_Temp[Temp, 2] = Summary_Table[i, 2];
                                Summary_Table_Temp[Temp, 3] = Summary_Table[i, 3];
                                Temp += 1;
                            }
                        }

                        Summary_Table = new string[Count2, 4];

                        for (int i = 0; i < Count2; i++)
                        {
                            Summary_Table[i, 0] = Summary_Table_Temp[i, 0];
                            Summary_Table[i, 1] = Summary_Table_Temp[i, 1];
                            Summary_Table[i, 2] = Summary_Table_Temp[i, 2];
                            Summary_Table[i, 3] = Summary_Table_Temp[i, 3];
                        }

                        TempValue = Convert.ToDouble(Summary_Table[0, 1]);
                        for (int i = 1; i < Count2; i++) if (TempValue < Convert.ToDouble(Summary_Table[i, 1])) TempValue = Convert.ToDouble(Summary_Table[i, 1]);

                        for (int i = 0; i < Count2; i++)
                        {
                            if (TempValue == Convert.ToDouble(Summary_Table[i, 1])) Result = Summary_Table[i, 0];
                        }
                    }
                    else
                    {
                        TempValue = Convert.ToDouble(Summary_Table[0, 1]);
                        for (int i = 1; i < Count1; i++) if (TempValue < Convert.ToDouble(Summary_Table[i, 1])) TempValue = Convert.ToDouble(Summary_Table[i, 1]);

                        for (int i = 0; i < Count1; i++)
                        {
                            if (TempValue == Convert.ToDouble(Summary_Table[i, 1])) Result = Summary_Table[i, 0];
                        }
                    }
                }
                else
                {
                    TempValue = Convert.ToDouble(Summary_Table[0, 1]);
                    for (int i = 1; i < 5; i++) if (TempValue < Convert.ToDouble(Summary_Table[i, 1])) TempValue = Convert.ToDouble(Summary_Table[i, 1]);

                    for (int i = 0; i < 5; i++)
                    {
                        if (TempValue == Convert.ToDouble(Summary_Table[i, 1])) Result = Summary_Table[i, 0];
                    }
                }

                if (Result == null) Result = "null";
                _Error = null;

                return Result;
            }
            catch (Exception e)
            {
                _Report = new string[1];
                _Report[0] = "Ошибка оценки модели.";
                _Error = e;

                return "";
            }
        }

        /// <summary>
        /// Поиск, построение и анализ наиболее успешной модели
        /// </summary>
        /// <param name="data_set">Данные из класса DataSet</param>
        /// <param name="RegressionModel">Данные из класса RegressionModel</param>
        public void MostSuitableModel(DataSet data_set, RegressionModel RegressionModel)
        {
            int _length = 0, Count = 0, location = 0;
            string[] _ReportTemp;
            _ReportTemp = new string[300];
            string Result = "";

            Result = FindingMostSuitableModel(data_set, RegressionModel);

            if (_Error == null)
            {
                switch (Result)
                {
                    case "Линейная модель":
                        _RegressionModel.BuildLinearModel(data_set);
                        _ReportTemp[0] = "Для текущего набора данных наиболее успешной является линейная модель: " + _RegressionModel.GetInfoModel["Модель"] + "\n";
                        break;
                    case "Полулогарифмическая модель":
                        _RegressionModel.BuildSemilogModel(data_set);
                        _ReportTemp[0] = "Для текущего набора данных наиболее успешной является полулогарифмическая модель: " + _RegressionModel.GetInfoModel["Модель"] + "\n";
                        break;
                    case "Параболическая модель":
                        _RegressionModel.BuildParabolicModel(data_set);
                        _ReportTemp[0] = "Для текущего набора данных наиболее успешной является параболическая модель: " + _RegressionModel.GetInfoModel["Модель"] + "\n";
                        break;
                    case "Степенная модель":
                        _RegressionModel.BuildDegreesModel(data_set);
                        _ReportTemp[0] = "Для текущего набора данных наиболее успешной является степенная модель: " + _RegressionModel.GetInfoModel["Модель"] + "\n";
                        break;
                    case "null":
                        _ReportTemp[0] = "Для текущего набора данных невозможно построить ни одну из моделей.\n";
                        break;
                    default:
                        _RegressionModel.BuildAdaptiveModel(data_set);
                        _ReportTemp[0] = "Для текущего набора данных наиболее успешной является модель полинома " + _RegressionModel.GetDegreesAdaptiveModel + " степени: " + _RegressionModel.GetInfoModel["Модель"] + "\n";
                        break;
                }

                location = _TempReport.Length + 1;

                for (int i = 1; i < location; i++)
                {
                    _ReportTemp[i] = _TempReport[i - 1];
                }

                if (Result != "null")
                {
                    Estimate(data_set, _RegressionModel);
                    
                    _length = _Report.Length;

                    for (int i = 0; i < _length; i++)
                    {
                        _ReportTemp[location] = _Report[i];
                        location += 1;
                    }
                }

                _length = _ReportTemp.Length;

                for (int i = 0; i < _length; i++)
                {
                    if (string.IsNullOrEmpty(_ReportTemp[i])) Count += 1;
                }

                _Report = new string[_length - Count];
                Count = 0;
                for (int i = 0; i < _length; i++)
                {
                    if (!string.IsNullOrEmpty(_ReportTemp[i]))
                    {
                        _Report[Count] = _ReportTemp[i];
                        Count += 1;
                    }
                }
            }
        }
        
        /// <summary>
        /// Метод, оценивающий параметры наиболее успешной модели
        /// </summary>
        private void NewEstimation_FMSM()
        {
            double A_Error_Approx = 0, _Estimated_Y, _Yi_avgY = 0, _Yi_avgY_2 = 0, Temp = 0, k1 = 0, k2 = 0, _ee1 = 0, _ee1_2 = 0;
            int _RowTest = 0, Position_k1 = 0, Position_k2 = 0;
            double[] Temp_PFC = new double[AmountCols - 1];

            try
            {
                Temp = Math.Sqrt(Convert.ToDouble(AmountRows));
                if (Temp == 0) _Standard_ECC = 0;
                else _Standard_ECC = 1 / Temp;
                _Standard_ECC = Math.Round(_Standard_ECC, _NumberDigits);

                if (_DataSet.GetCols == 2)
                {
                    for (int i = 0; i < AmountCols * 2 + 1; i++) _Variance_Analysis[i] = 0;

                    for (int j = 0; j < AmountRows; j++)
                    {
                        for (int i = 0; i < AmountCols; i++)
                        {
                            _Variance_Analysis[i] = _Variance_Analysis[i] + ArrayDataSet[j, i];
                            _Variance_Analysis[i + AmountCols + 1] = _Variance_Analysis[i + AmountCols + 1] + (ArrayDataSet[j, i] * ArrayDataSet[j, i]);
                        }

                        _Variance_Analysis[AmountCols] = _Variance_Analysis[AmountCols] + (ArrayDataSet[j, 0] * ArrayDataSet[j, 1]);

                        if (j == AmountRows - 1)
                        {
                            _Variance_Analysis[AmountCols] = _Variance_Analysis[AmountCols] / AmountRows;
                            _Variance_Analysis[AmountCols] = Math.Round(_Variance_Analysis[AmountCols], _NumberDigits);

                            for (int i = 0; i < AmountCols; i++)
                            {
                                _Variance_Analysis[i] = _Variance_Analysis[i] / AmountRows;
                                _Variance_Analysis[i] = Math.Round(_Variance_Analysis[i], _NumberDigits);
                                _Variance_Analysis[i + AmountCols + 1] = (_Variance_Analysis[i + AmountCols + 1] / AmountRows) - Math.Pow(_Variance_Analysis[i], 2.0);
                                _Variance_Analysis[i + AmountCols + 1] = Math.Round(_Variance_Analysis[i + AmountCols + 1], _NumberDigits);
                                _Variance_Analysis[i + AmountCols * 2 + 1] = Math.Sqrt(_Variance_Analysis[i + AmountCols + 1]);
                                _Variance_Analysis[i + AmountCols * 2 + 1] = Math.Round(_Variance_Analysis[i + AmountCols * 2 + 1], _NumberDigits);
                            }
                        }
                    }

                    for (int i = 0; i < AmountCols; i++) _average_value[i] = _Variance_Analysis[i];
                }
                else if (_DataSet.GetCols > 2)
                {
                    // Подсчет средних значений Y и Xi
                    for (int i = 0; i < AmountCols; i++)
                    {
                        _average_value[i] = 0;

                        for (int j = 0; j < AmountRows; j++)
                        {
                            _average_value[i] = _average_value[i] + ArrayDataSet[j, i];
                        }
                        _average_value[i] = _average_value[i] / AmountRows;
                        _average_value[i] = Math.Round(_average_value[i], _NumberDigits);
                    }
                }

                // Подсчет ошибки аппроксимации и дисперсии
                for (int i = 0; i < AmountRows; i++)
                {
                    _Estimated_Y = 0;

                    // Подсчет Yt
                    for (int j = 1; j < AmountCols; j++)
                    {
                        _Estimated_Y = _Estimated_Y + _RegressionModel.GetCoefficients[j] * ArrayDataSet[i, j];
                    }
                    _Estimated_Y = _Estimated_Y + _RegressionModel.GetCoefficients[0];

                    // Подсчет Y - Yt
                    A_Error_Approx = ArrayDataSet[i, 0] - _Estimated_Y;

                    _EstErrY[i] = A_Error_Approx;
                    _EstDispersion = _EstDispersion + Math.Pow(A_Error_Approx, 2.0);

                    _Yi_avgY = ArrayDataSet[i, 0] - _average_value[0];
                    _Yi_avgY_2 = _Yi_avgY_2 + Math.Pow(_Yi_avgY, 2.0);

                    if (i > 0)
                    {
                        _ee1 = _ee1 + (_EstErrY[i] * _EstErrY[i - 1]);
                        _ee1_2 = _ee1_2 + Math.Pow((_EstErrY[i] - _EstErrY[i - 1]), 2.0);
                    }
                }

                Temp = _EstDispersion;
                if (Temp != 0) Temp = _ee1 / Temp;
                _CoefAutoCor = Temp;
                _CoefAutoCor = Math.Round(_CoefAutoCor, _NumberDigits);
                Temp = _EstDispersion;
                if (Temp != 0) Temp = _ee1_2 / Temp;
                DW = Temp;
                DW = Math.Round(DW, _NumberDigits);

                _EstDispersion = Math.Round(_EstDispersion, _NumberDigits);

                // Подсчет множественного коэффициента корреляции
                Temp = _Yi_avgY_2;
                if (Temp != 0) Temp = 1 - (_EstDispersion / Temp);
                else Temp = 1;
                if (Temp >= 0) Temp = Math.Sqrt(Temp);
                else Temp = 0;
                _CorrelationCoef = Temp;
                _CorrelationCoef = Math.Round(_CorrelationCoef, _NumberDigits);

                // Подсчет коэффициента детерминации
                _Coef_Determination = Math.Pow(_CorrelationCoef, 2.0);
                _Coef_Determination = Math.Round(_Coef_Determination, _NumberDigits);

                // Подсчет скорректированного коэффициента детерминации
                Temp = Convert.ToDouble(AmountRows - (AmountCols - 1));
                if (Temp != 0) Temp = Convert.ToDouble(AmountRows - 1) / Temp;
                _Odj_Coef_Determination = 1 - ((1 - _Coef_Determination) * Temp);
                _Odj_Coef_Determination = Math.Round(_Odj_Coef_Determination, _NumberDigits);

                // Подсчет F-критерия Фишера
                k1 = Convert.ToDouble(AmountCols);
                k2 = Convert.ToDouble(AmountRows - AmountCols);
                Temp = 1 - _Coef_Determination;
                if (Temp != 0) Temp = _Coef_Determination / Temp;
                _F_Criterion = Temp * (k2 / (k1 - 1));
                _F_Criterion = Math.Round(_F_Criterion, _NumberDigits);

                for (int i = 0; i < 2; i++)
                {
                    k1 = k1 - 1;
                    if (k1 <= 6 && k1 >= 1) Position_k1 = Convert.ToInt32(k1 - 1);
                    else if (k1 == 7) Position_k1 = 5;
                    else if (k1 < 12 && k1 >= 8) Position_k1 = 6;
                    else if (k1 < 24 && k1 >= 12) Position_k1 = 7;
                    else if (k1 == 24) Position_k1 = 8;
                    else Position_k1 = 9;

                    if (k2 >= 1 && k2 <= 30) Position_k2 = Convert.ToInt32(k2 - 1);
                    else if (k2 > 30 && k2 < 60) Position_k2 = 29;
                    else if (k2 == 60) Position_k2 = 30;
                    else Position_k2 = 31;

                    _Fkp[i, 0] = k1;
                    _Fkp[i, 1] = k2;
                    _Fkp[i, 2] = _F_Distribution[Position_k2, Position_k1];
                }

                if (AmountCols - 1 > 5)
                {
                    _TabDW_d1 = 1.5;
                    _TabDW_d2 = 1.5;
                }
                else
                {
                    _RowTest = Table_Durbin_Watson.GetLength(0);
                    _TabDW_d1 = ((AmountCols - 1) * 2) - 1;
                    _TabDW_d2 = (AmountCols - 1) * 2;

                    for (int i = 0; i < _RowTest - 1; i++)
                    {
                        if (Table_Durbin_Watson[0, 0] > AmountRows)
                        {
                            _TabDW_d1 = Table_Durbin_Watson[0, Convert.ToInt32(_TabDW_d1)];
                            _TabDW_d2 = Table_Durbin_Watson[0, Convert.ToInt32(_TabDW_d2)];
                            break;
                        }
                        else if (Table_Durbin_Watson[i, 0] == AmountRows)
                        {
                            _TabDW_d1 = Table_Durbin_Watson[i, Convert.ToInt32(_TabDW_d1)];
                            _TabDW_d2 = Table_Durbin_Watson[i, Convert.ToInt32(_TabDW_d2)];
                            break;
                        }
                        else if (AmountRows > Table_Durbin_Watson[i, 0] && AmountRows < Table_Durbin_Watson[i + 1, 0])
                        {
                            _TabDW_d1 = Table_Durbin_Watson[i, Convert.ToInt32(_TabDW_d1)];
                            _TabDW_d2 = Table_Durbin_Watson[i, Convert.ToInt32(_TabDW_d2)];
                            break;
                        }
                        else if (AmountRows > Table_Durbin_Watson[_RowTest - 1, 0])
                        {
                            _TabDW_d1 = Table_Durbin_Watson[_RowTest - 1, Convert.ToInt32(_TabDW_d1)];
                            _TabDW_d2 = Table_Durbin_Watson[_RowTest - 1, Convert.ToInt32(_TabDW_d2)];
                            break;
                        }
                    }
                }

                _Error = null;
            }
            catch (Exception e)
            {
                _Report = new string[1];
                _Report[0] = "Ошибка оценки модели.";
                _Error = e;
            }
        }

        #endregion
    }
}

