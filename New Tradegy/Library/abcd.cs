using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static New_Tradegy.Library.g;
using System.Windows.Forms.DataVisualization.Charting;

namespace New_Tradegy.Library
{
    internal class abcd
    {
    }
}
//        // clic -> delete corresponding chartarea and annotation -> call mdm & mds
//        //  displayList does not change -> add missing chartarea or annotation
//        // relocate

//        // resuffle(keys) -> new displayList -> call mdm & mds
//        //  delete chartarea and annotation not in displayList
//        //  add missing chartarea or annotation
//        // relocate

//        // post -> 200 stocks (included stocks -> eval_stock -> new displayList)
//        //  not included but in the displayList -> update last point or add a new point



//        private async Task UpdateChartAsync(Chart chart, List<string> displayList, List<StockData> stocks)
//        {
//            await Task.Run(() =>
//            {
//                foreach (var stock in stocks)
//                {
//                    if (!displayList.Contains(stock.Name)) continue; // Only update existing stocks

//                    // Update series
//                    var series = chart.Series.FirstOrDefault(s => s.Name == stock.Name);
//                    if (series != null)
//                    {
//                        series.Points.AddXY(stock.Time, stock.Value);
//                        if (series.Points.Count > 100) // Limit points
//                            series.Points.RemoveAt(0);
//                    }

//                    // Update annotations
//                    var annotation = chart.Annotations.FirstOrDefault(a => a.Name == stock.Name);
//                    if (annotation != null)
//                    {
//                        annotation.Text = stock.AnnotationText;
//                        annotation.X = stock.AnnotationX;
//                        annotation.Y = stock.AnnotationY;
//                    }
//                }
//            });
//        }




//        private async Task AddVeryActiveStocksToChart1Async(List<StockData> veryActiveStocks)
//        {
//            await Task.Run(() =>
//            {
//                foreach (var stock in veryActiveStocks)
//                {
//                    if (!chart1DisplayList.Contains(stock.Name))
//                    {
//                        // Add to chart1
//                        chart1DisplayList.Add(stock.Name);
//                        AddStockToChart(chart1, stock);

//                        // Handle reshuffling if chart1 exceeds 26 stocks
//                        if (chart1DisplayList.Count > 26)
//                        {
//                            // Remove the least active stock (custom logic for removal)
//                            var stockToRemove = DetermineLeastActiveStock(chart1DisplayList);
//                            RemoveStockFromChart(chart1, stockToRemove);
//                            chart1DisplayList.Remove(stockToRemove);
//                        }
//                    }
//                }
//            });
//        }




//        private string DetermineLeastActiveStock(List<string> displayList)
//        {
//            // Custom logic to determine the least active stock
//            // For example, based on trading volume or other metrics
//            return displayList.OrderBy(s => GetStockActivityMetric(s)).FirstOrDefault();
//        }



//        private void RemoveStockFromChart(Chart chart, string stockName)
//        {
//            // Remove series
//            var series = chart.Series.FirstOrDefault(s => s.Name == stockName);
//            if (series != null) chart.Series.Remove(series);

//            // Remove annotation
//            var annotation = chart.Annotations.FirstOrDefault(a => a.Name == stockName);
//            if (annotation != null) chart.Annotations.Remove(annotation);
//        }




//        public async Task UpdateChartsWithReshufflingAsync()
//        {
//            // Download stocks (simulate with a method)
//            var stocks = await DownloadStockDataAsync();

//            // Determine very active stocks
//            var veryActiveStocks = post(stocks);

//            // Separate KODEX and general stocks
//            var kodexStocks = stocks.Where(s => s.Name.StartsWith("KODEX")).ToList();
//            var generalStocks = stocks.Where(s => !s.Name.StartsWith("KODEX")).ToList();

//            // Update chart1 and chart2 in parallel
//            await Task.WhenAll(
//                UpdateChartAsync(chart1, chart1DisplayList, stocks),
//                UpdateChartAsync(chart2, displayList, stocks),
//                AddVeryActiveStocksToChart1Async(veryActiveStocks)
//            );

//            // Log completion
//            Console.WriteLine("Charts updated with reshuffling.");
//        }


//    }
//}
