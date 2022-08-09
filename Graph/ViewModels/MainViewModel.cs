using CsvHelper;
using CsvHelper.Configuration;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;

namespace Graph.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        #region variables
        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };
        public event PropertyChangedEventHandler PropertyChanged;
        private SeriesCollection seriesCollection { get; set; }

        private Model.GraphRecord selectedGraphRecord;

        private double xLabel;
        private double yLabel;

        private ObservableCollection<Model.GraphRecord> graphRecords;

        public Func<double, string> yformatter;
        #endregion

        #region get
        public Func<double, string> YFormatter
        {
            get
            {
                return yformatter = value => value.ToString();
            }
        }
        public ObservableCollection<Model.GraphRecord> GraphRecords
        {
            get
            {
                if (graphRecords == null)
                {
                    graphRecords = new ObservableCollection<Model.GraphRecord>();
                    return graphRecords;
                }
                else return graphRecords;
            }
            set { graphRecords = value; SyncCollecctions(); OnPropertyChanged("GraphRecords"); }
        }

        public SeriesCollection SeriesCollection
        {
            get
            {
                if (seriesCollection == null)
                {
                    seriesCollection = new SeriesCollection(new LineSeries()
                    {
                        LineSmoothness = 0,
                        Title = "TestGraph",
                        Fill = System.Windows.Media.Brushes.Transparent
                    });
                    return seriesCollection;
                }
                else return seriesCollection;
            }
            set { seriesCollection = value; OnPropertyChanged("SeriesCollection"); }
        }

        public double XLabel
        {
            get { return xLabel; }
            set { xLabel = value; OnPropertyChanged("XLabel"); }
        }

        public double YLabel
        {
            get { return yLabel; }
            set { yLabel = value; OnPropertyChanged("YLabel"); }
        }

        public Model.GraphRecord GraphRecord
        {
            get { return selectedGraphRecord; }
            set { selectedGraphRecord = value; SyncCollecctions(); OnPropertyChanged("GraphRecord"); }
        }

        #endregion

        #region functions
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        //Метод синкронизации точек в листе с графиком
        private void SyncCollecctions()
        {
            switch (SeriesCollection.Count)
            {
                case 0:
                    foreach (var record in GraphRecords)
                    {
                        var tempSeries = new LineSeries()
                        {
                            Values = new ChartValues<ObservablePoint>() { new ObservablePoint(record.SecondPoint, record.FirstPoint) },
                            LineSmoothness = 0,
                            Title = "TestGraph",
                            Fill = System.Windows.Media.Brushes.Transparent
                        };
                        SeriesCollection.Add(tempSeries);
                    }
                    break;
                default:
                    SeriesCollection[0].Values.Clear();
                    foreach (var record in GraphRecords)
                    {
                        SeriesCollection[0].Values.Add(new ObservablePoint(record.SecondPoint, record.FirstPoint));
                    }
                    break;
            }

        }

        // Метод добавления точки в лист
        private void AddRecord()
        {
            GraphRecords.Add(new Model.GraphRecord() { FirstPoint = XLabel, SecondPoint = YLabel });
            SyncCollecctions();
        }

        private void RemoveRecord()
        {
            if (GraphRecord != null)
            {
                GraphRecords.Remove(GraphRecord);
                SyncCollecctions();
                MessageBox.Show("Точка успешно удалена");
            }
            else
            {
                MessageBox.Show("Ошибка удаления, возможно вы не выбрали точку");
            }

        }
        // Сохранение точек в формате .csv таблицы
        private void SaveToFile()
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "csv File|*.csv",
                Title = "Сохраните таблицу"
            };

            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                using (TextWriter writer = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                {
                    var csv = new CsvWriter(writer, config);
                    csv.WriteRecords(GraphRecords);
                }
            }
        }
        // чтение из файла
        private void ReadFromFile()
        {
            SyncCollecctions();
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "csv File|*.csv",
                Title = "Откройте таблицу"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var reader = new StreamReader(openFileDialog.FileName))
                    using (var csv = new CsvReader(reader, config))
                    {
                        var tempRecords = csv.GetRecords<Model.GraphRecord>().ToList();
                        GraphRecords.Clear();
                        foreach (var record in tempRecords)
                        {
                            var tempRecord = new Model.GraphRecord()
                            {
                                FirstPoint = Convert.ToDouble(record.FirstPoint),
                                SecondPoint = Convert.ToDouble(record.SecondPoint)
                            };
                            GraphRecords.Add(tempRecord);
                        }
                        SeriesCollection.Clear();
                        SeriesCollection.Add(new LineSeries()
                        {
                            Values = new ChartValues<ObservablePoint>(),
                            LineSmoothness = 0,
                            Title = "TestGraph",
                            Fill = System.Windows.Media.Brushes.Transparent
                        });
                        foreach (var record in GraphRecords)
                        {
                            SeriesCollection[0].Values.Add(new ObservablePoint(record.SecondPoint, record.FirstPoint));
                        }

                    }
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        #endregion

        #region commands

        public Command.MyCommand AddButton
        {
            get { return new Command.MyCommand((o) => { AddRecord(); }); }
        }

        public Command.MyCommand RemoveButton
        {
            get
            {
                return new Command.MyCommand((o) =>
            {
                try
                {
                    RemoveRecord(); SyncCollecctions();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
            }
        }

        public Command.MyCommand SaveButton
        {
            get
            {
                return new Command.MyCommand((o) =>
                {

                    try
                    {
                        SaveToFile();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
            }
        }


        public Command.MyCommand LoadButton
        {
            get
            {
                return new Command.MyCommand((o) =>
                {
                    try
                    {
                        ReadFromFile();
                        SyncCollecctions();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
            }
        }
        #endregion
    }
}
