using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingUtilitiesLibrary.Exports
{
    public static class Exporter
    {
        public static void GenerateTurnover(string path, TECBid bid, TECEstimator estimate, bool openOnComplete = true)
        {
            Turnover.GenerateTurnoverExport(path, bid, estimate, openOnComplete);
        }
        public static void GenerateSummary(string path, TECBid bid, TECEstimator estimate, bool openOnComplete = true)
        {
            Turnover.GenerateSummaryExport(path, bid, estimate, openOnComplete);
        }
        public static void GenerateBOM(string path, TECBid bid, bool openOnComplete = true)
        {
            Turnover.GenerateBOM(path, bid, openOnComplete);
        }
        public static void GenerateTemplateSummary(string path, TECTemplates templates, bool openOnComplete = true)
        {
            Templates.Export(path, templates, openOnComplete);
        }
        public static void GeneratePointsList(string path, TECBid bid, bool openOnComplete = true)
        {
            PointsList.ExportPointsList(path, bid, openOnComplete);
        }
        public static void GenerateBudget(string path, TECBid bid, bool openOnComplete = true)
        {
            Budget.GenerateReport(path, bid, openOnComplete);
        }
    }
}
