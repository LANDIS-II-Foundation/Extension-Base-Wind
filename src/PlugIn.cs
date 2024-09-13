//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.Metadata;
using System.Collections.Generic;
using System.IO;
using System;

namespace Landis.Extension.OriginalWind
{
    ///<summary>
    /// A disturbance plug-in that simulates wind disturbance.
    /// </summary>

    public class PlugIn
        : ExtensionMain
    {
        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:wind");
        public static MetadataTable<EventsLog> WindEventLog;
        public static MetadataTable<SummaryLog> WindSummaryLog;
        public static readonly string ExtensionName = "Original Wind";
        
        private string mapNameTemplate;
        private IInputParameters parameters;
        private static ICore modelCore;
        private int summaryTotalSites;
        private int summaryEventCount;


        //---------------------------------------------------------------------

        public PlugIn()
            : base("Original Wind", ExtType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        public override void AddCohortData()
        {
            return;
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            InputParameterParser parser = new InputParameterParser();
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the plug-in with a data file.
        /// </summary>
        public override void Initialize()
        {
            PlugIn.ModelCore.UI.WriteLine("Initializing Original Wind ...");

            List<string> colnames = new List<string>();
            foreach (IEcoregion ecoregion in modelCore.Ecoregions)
            {
                colnames.Add(ecoregion.Name);
            }

            Timestep = parameters.Timestep;
            mapNameTemplate = parameters.MapNamesTemplate;

            SiteVars.Initialize();
            Event.Initialize(parameters.EventParameters, parameters.WindSeverities);

            modelCore.UI.WriteLine("   Opening and Initializing wind log files \"{0}\" and \"{1}\"...", parameters.EventLogFileName, parameters.SummaryLogFileName);
            //ExtensionMetadata.ColumnNames = colnames;
            MetadataHandler.InitializeMetadata(parameters.Timestep, parameters.MapNamesTemplate, parameters.SummaryLogFileName, parameters.EventLogFileName);

        }

        //---------------------------------------------------------------------

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {
            ModelCore.UI.WriteLine("Processing landscape for wind events ...");

            SiteVars.Event.SiteValues = null;
            SiteVars.Severity.ActiveSiteValues = 0;

            int eventCount = 0;
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) {
                Event windEvent = Event.Initiate(site, Timestep);
                if (windEvent != null) {
                    LogEvent(PlugIn.ModelCore.CurrentTime, windEvent);
                    eventCount++;
                    summaryEventCount++;

                }
            }


            //ModelCore.UI.WriteLine("  Wind events: {0}", eventCount);

            //  Write wind severity map
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, PlugIn.modelCore.CurrentTime);
            Dimensions dimensions = new Dimensions(modelCore.Landscape.Rows, modelCore.Landscape.Columns);
            using (IOutputRaster<BytePixel> outputRaster = modelCore.CreateRaster<BytePixel>(path, dimensions))
            {
                BytePixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites) {
                    if (site.IsActive) {
                        if (SiteVars.Disturbed[site])
                            pixel.MapCode.Value = (byte) (SiteVars.Severity[site] + 1);
                        else
                            pixel.MapCode.Value = 1;
                    }
                    else {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            WriteSummaryLog(PlugIn.modelCore.CurrentTime);
            summaryTotalSites = 0;
            summaryEventCount = 0;
        }

        //---------------------------------------------------------------------

        private void LogEvent(int   currentTime,
                              Event windEvent)
        {

            WindEventLog.Clear();
            EventsLog el = new EventsLog();
            el.Time = currentTime;
            el.InitRow = windEvent.StartLocation.Row;
            el.InitColumn = windEvent.StartLocation.Column;
            el.TotalSites = windEvent.Size;
            el.DamagedSites = windEvent.SitesDamaged;
            el.CohortsKilled = windEvent.CohortsKilled;
            el.MeanSeverity = windEvent.Severity;

            summaryTotalSites += windEvent.SitesDamaged;
            WindEventLog.AddObject(el);
            WindEventLog.WriteToFile();


        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int currentTime)
        {
            WindSummaryLog.Clear();
            SummaryLog sl = new SummaryLog();
            sl.Time = currentTime;
            sl.TotalSitesDisturbed = summaryTotalSites;
            sl.NumberEvents = summaryEventCount;

            WindSummaryLog.AddObject(sl);
            WindSummaryLog.WriteToFile();
        }
    }
}
