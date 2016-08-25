namespace RemoteTechAutoScan
{
    using System.Linq;

    public class RemoteTechAutoScanPartModule : PartModule
    {
        [KSPField()]
        public bool isActive;

        private static int counter = 1;

        [KSPAction("Turn on auto scan", actionGroup = KSPActionGroup.Custom01)]
        public void TurnOnAutoScan()
        {
            isActive = true;
        }

        [KSPAction("Turn off auto scan")]
        public void TurnOffAutoScan()
        {
            isActive = false;
        }

        [KSPAction("Toggle auto scan")]
        public void ToggleAutoScan()
        {
            isActive = !isActive;
        }

        public override void OnUpdate()
        {
            if(counter++ % 100 != 0)
            {
                return;
            }

            counter = 0;

            var vessel = FlightGlobals.ActiveVessel;
            switch (vessel.situation)
            {
                case Vessel.Situations.LANDED:   
                case Vessel.Situations.SUB_ORBITAL:
                case Vessel.Situations.ORBITING:
                case Vessel.Situations.ESCAPING:
                    if (isActive)
                    {
                        AutoScanEngine.Instance.DoAutoScan();
                    }

                    break;
                case Vessel.Situations.SPLASHED:
                case Vessel.Situations.DOCKED:
                case Vessel.Situations.FLYING:
                case Vessel.Situations.PRELAUNCH:
                default:
                    return;
            }
        }
    }
}