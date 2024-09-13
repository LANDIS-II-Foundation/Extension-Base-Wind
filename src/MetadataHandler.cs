using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;
using Landis.Utilities;
using Landis.Core;
using System.IO;

namespace Landis.Extension.OriginalWind
{
    public static class MetadataHandler
    {
        
        public static ExtensionMetadata Extension {get; set;}

        public static void InitializeMetadata(int Timestep, string MapFileName, string windSummaryLogFileName, string windEventLogFileName)
        {
            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata() {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime
            };

            Extension = new ExtensionMetadata(PlugIn.ModelCore){
                Name = PlugIn.ExtensionName,
                TimeInterval = Timestep, 
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

            CreateDirectory(windEventLogFileName);
            CreateDirectory(windSummaryLogFileName);

            PlugIn.WindEventLog = new MetadataTable<EventsLog>(windEventLogFileName);
            PlugIn.WindSummaryLog = new MetadataTable<SummaryLog>(windSummaryLogFileName);

            PlugIn.ModelCore.UI.WriteLine("   Generating wind event table...");
            OutputMetadata tblOut_events = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "WindEventLog",
                FilePath = PlugIn.WindEventLog.FilePath,
                Visualize = false,
            };
            tblOut_events.RetriveFields(typeof(EventsLog));
            Extension.OutputMetadatas.Add(tblOut_events);

            PlugIn.ModelCore.UI.WriteLine("   Generating wind summary table...");
            OutputMetadata tblOut_summary = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "WindSummaryLog",
                FilePath = PlugIn.WindSummaryLog.FilePath,
                Visualize = true,
            };

            tblOut_summary.RetriveFields(typeof(SummaryLog));
            Extension.OutputMetadatas.Add(tblOut_summary);

            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            OutputMetadata mapOut_Severity = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "severity",
                FilePath = @MapFileName,
                Map_DataType = MapDataType.Ordinal,
                Map_Unit = FieldUnits.Severity_Rank,
                Visualize = true,
            };
            Extension.OutputMetadatas.Add(mapOut_Severity);

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);

        }

        public static void CreateDirectory(string path)
        {
            path = path.Trim(null);
            if (path.Length == 0)
                throw new ArgumentException("path is empty or just whitespace");
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Landis.Utilities.Directory.EnsureExists(dir);
            }

            return;
        }

    }
}
