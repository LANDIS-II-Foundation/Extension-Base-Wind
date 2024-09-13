//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Library.UniversalCohorts;

namespace Landis.Extension.OriginalWind
{
    public static class SiteVars
    {

        //---------------------------------------------------------------------

        public static void Initialize()
        {
            Event        = PlugIn.ModelCore.Landscape.NewSiteVar<Event>(InactiveSiteMode.DistinctValues);
            TimeOfLastEvent = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            Severity        = PlugIn.ModelCore.Landscape.NewSiteVar<byte>();
            Disturbed      = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();

            PlugIn.ModelCore.RegisterSiteVar(SiteVars.TimeOfLastEvent, "Wind.TimeOfLastEvent");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Severity, "Wind.Severity");

            Cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.UniversalCohorts");

        }

        //---------------------------------------------------------------------
        public static ISiteVar<ISiteCohorts> Cohorts { get; private set; }
        //---------------------------------------------------------------------

        public static ISiteVar<Event> Event { get; private set; }

        //---------------------------------------------------------------------

        public static ISiteVar<int> TimeOfLastEvent { get; private set; }

        //---------------------------------------------------------------------

        public static ISiteVar<byte> Severity { get; private set; }
        //---------------------------------------------------------------------

        public static ISiteVar<bool> Disturbed { get; private set; }
    }
}
