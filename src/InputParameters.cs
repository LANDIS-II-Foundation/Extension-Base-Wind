//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.Utilities;
using System.Collections.Generic;

namespace Landis.Extension.BaseWind
{
	/// <summary>
	/// Parameters for the plug-in.
	/// </summary>
	public class InputParameters
		: IInputParameters
	{
		private int timestep;
		private IEventParameters[] eventParameters;
		private List<ISeverity> severities;
		private string mapNamesTemplate;
		private string summaryLogFileName;
        private string eventLogFileName;

        //---------------------------------------------------------------------

        /// <summary>
        /// Timestep (years)
        /// </summary>
        public int Timestep
		{
			get {
				return timestep;
			}
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                timestep = value;
            }
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Wind event parameters for each ecoregion.
		/// </summary>
		/// <remarks>
		/// Use Ecoregion.Index property to index this array.
		/// </remarks>
		public IEventParameters[] EventParameters
		{
			get {
				return eventParameters;
			}
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Definitions of wind severities.
		/// </summary>
		public List<ISeverity> WindSeverities
		{
			get {
				return severities;
			}
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Template for the filenames for output maps.
		/// </summary>
		public string MapNamesTemplate
		{
			get {
				return mapNamesTemplate;
			}
            set {
                MapNames.CheckTemplateVars(value);
                mapNamesTemplate = value;
            }
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Name of summary log file.
		/// </summary>
		public string SummaryLogFileName
		{
			get {
				return summaryLogFileName;
			}
            set {
                if (value == null)
                    throw new InputValueException(value.ToString(), "Value must be a file path.");
                summaryLogFileName = value;
            }
		}

        //---------------------------------------------------------------------

        /// <summary>
        /// Name of event log file.
        /// </summary>
        public string EventLogFileName
        {
            get
            {
                return eventLogFileName;
            }
            set
            {
                if (value == null)
                    throw new InputValueException(value.ToString(), "Value must be a file path.");
                eventLogFileName = value;
            }
        }
        //---------------------------------------------------------------------

        public InputParameters(int ecoregionCount)
        {
            eventParameters = new IEventParameters[ecoregionCount];
            severities = new List<ISeverity>();
        }

	}
}
