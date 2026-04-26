using System;
using System.Threading.Tasks;
using MyShop.Models;

namespace MyShop.Services;

public interface IReportService
{
    Task<ReportData> GetReportDataAsync(DateTime start, DateTime end, string granularity, string categoryScope = "All");
}
