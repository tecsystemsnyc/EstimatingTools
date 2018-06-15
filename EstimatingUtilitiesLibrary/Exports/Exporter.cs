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
        public static string TurnoverDefaultName(TECBid bid)
        {
            return String.Format("{0} Turnover", bid.BidNumber);
        }
        public static string SummaryDefaultName(TECBid bid)
        {
            return String.Format("{0} Summary", bid.BidNumber);
        }
        public static string BOMDefaultName(TECBid bid)
        {
            return String.Format("{0} BOM", bid.BidNumber);
        }
        public static string TemplateSummaryDefaultName()
        {
            return String.Format("{0} TemplateSummary", DateTime.Now.ToString());
        }
        public static string PointsListDefaultName(TECBid bid)
        {
            return String.Format("{0} Points List", bid.BidNumber);
        }
        public static string BudgetDefaultName(TECBid bid)
        {
            return String.Format("{0} Budget", bid.BidNumber);
        }
        public static string ProposalDefaultName(TECBid bid)
        {
            return String.Format("{0} Proposal", bid.Name);
        }

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
        public static void GeneratePointsListCSV(string path, TECBid bid, bool openOnComplete = true)
        {
            CSVWriter writer = new CSVWriter(path);
            writer.BidPointsToCSV(bid);
            if (openOnComplete)
            {
                System.Diagnostics.Process.Start(path);
            }
        }
        public static void GenerateBudget(string path, TECBid bid, bool openOnComplete = true)
        {
            Budget.GenerateReport(path, bid, openOnComplete);
        }
        public static void GenerateProposal(string path, TECBid bid, TECEstimator estimate, bool openOnComplete = true)
        {
            ScopeWordDocumentBuilder.CreateScopeWordDocument(bid, estimate, path, openOnComplete);
        }
    }
}
