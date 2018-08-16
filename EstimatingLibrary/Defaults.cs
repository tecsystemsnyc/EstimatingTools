using System.Collections.Generic;

namespace EstimatingLibrary
{
    public static class Defaults
    {
        public static List<string> Scope = new List<string>
        {
            "Provide a seamless extension to the existing Honeywell system provided by T.E.C.Systems.",
            "System components shall be manufactured by Honeywell Automation and will be designed and installed by T.E.C. Systems, Inc.",
            "Provide the engineering resources required for the creation of design, installation and as-built documentation as required for the submittal and training phases of the project.",
            "Provide the programming resources required.",
            "Provide field technical resources required for system commissioning, calibration, start-up and testing.",
            "Provide training in accordance with specification requirements.",
            "Provide one-year parts and labor warranty."
        };

        public static List<string> Notes = new List<string>
        {
            "Use tax is included; this project is assumed to be a capital improvement project."
        };

        public static List<string> Exclusions = new List<string>
        {
            "Overtime labor.",
            "Power wiring.",
            "Access doors.",
            "Demolition.",
            "Patching, painting and debris removal.",
            "Automatic louver dampers, smoke dampers, fire/smoke dampers, and end switches.",
            "Steam-fitting and Sheet-metal labor.",
            "Installation of valves, wells, and dampers.",
            "Self-contained valves.",
            "Provision of smoke detection and fire alarm system components.",
            "Payment and Performance Bonds.",
            "Smoke purge and UL-864 systems."
        };

        public static List<(string description, string url)> BidToDoList = new List<(string description, string url)>
        {
            ("Fill out bid info", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Fill-Out-Bid-Info"),
            ("Populate riser", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Populate-Riser"),
            ("Create systems from sequences", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Create-System-Instances"),
            ("Verify valve selections", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Verify-Valve-Selections"),
            ("Verify electrical runs", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Verify-Electrical-Runs"),
            ("Verify proposal", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Verify-Proposal"),
            ("Add systems' miscellaneous costs", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Add-System-Miscellaneous-Costs"),
            ("Create system instances", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Create-System-Instances"),
            ("Create and verify controllers", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Create-and-Verify-Controllers"),
            ("Create network electrical runs", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Create-Network-Electrical-Runs"),
            ("Add any miscellaneous costs for the bid", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Add-Bid-Miscellaneous-Costs"),
            ("Submit quotes", "https://github.com/tecsystemsnyc/EstimatingTools/wiki/Submit-Quotes"),
        };
    }
}
