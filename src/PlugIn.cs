//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.Metadata;
using System.Collections.Generic;
using System.IO;
using System;

namespace Landis.Extension.BaseWind
{
    ///<summary>
    /// A disturbance plug-in that simulates wind disturbance.
    /// </summary>

    public class PlugIn
        : ExtensionMain
    {
        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:wind");
        public static MetadataTable<EventsLog> eventLog;

        public static readonly string ExtensionName = "Base Wind";
        
        private string mapNameTemplate;
        //private StreamWriter log;
        private IInputParameters parameters;
        private static ICore modelCore;


        //---------------------------------------------------------------------

        public PlugIn()
            : base("Base Wind", ExtType)
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
        /// <param name="dataFile">
        /// Path to the file with initialization data.
        /// </param>
        /// <param name="startTime">
        /// Initial timestep (year): the timestep that will be passed to the
        /// first call to the component's Run method.
        /// </param>
        public override void Initialize()
        {

            MetadataHandler.InitializeMetadata(parameters.Timestep, parameters.MapNamesTemplate, ModelCore);

            Timestep = parameters.Timestep;
            mapNameTemplate = parameters.MapNamesTemplate;

            SiteVars.Initialize();
            Event.Initialize(parameters.EventParameters, parameters.WindSeverities);

            //ModelCore.UI.WriteLine("   Opening wind log file \"{0}\" ...", parameters.LogFileName);
            //log = Landis.Data.CreateTextFile(parameters.LogFileName);
            //log.AutoFlush = true;
            //log.WriteLine("Time,Initiation Site,Total Sites,Damaged Sites,Cohorts Killed,Mean Severity");
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
                }
            }
            ModelCore.UI.WriteLine("  Wind events: {0}", eventCount);

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
        }

        //---------------------------------------------------------------------

        private void LogEvent(int   currentTime,
                              Event windEvent)
        {
            //log.WriteLine("{0},\"{1}\",{2},{3},{4},{5:0.0}",
            //              currentTime,
            //              windEvent.StartLocation,
            //              windEvent.Size,
            //              windEvent.SitesDamaged,
            //              windEvent.CohortsKilled,
            //              windEvent.Severity);

            eventLog.Clear();
            EventsLog el = new EventsLog();
            el.Time = currentTime;
            el.InitRow = windEvent.StartLocation.Row;
            el.InitColumn = windEvent.StartLocation.Column;
            el.TotalSites = windEvent.Size;
            el.DamagedSites = windEvent.SitesDamaged;
            el.CohortsKilled = windEvent.CohortsKilled;
            el.MeanSeverity = windEvent.Severity;


            eventLog.AddObject(el);
            eventLog.WriteToFile();

        }


    }
}
