
namespace RemoteTechAutoScan
{
    using RemoteTech.Modules;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    using RT = RemoteTech.API;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RemoteTechAutoScanStartup : MonoBehaviour
    {
        private int counter; 
        public RemoteTechAutoScanStartup()
        {
            DontDestroyOnLoad(this);
        }

        public void FixedUpdate()
        {
           

        }

        public void Update()
        {
            // runs very often so we slow it down
            if (counter++ % 10 != 0)
            {
                return;
            }

            // if Remote tech is turned off, then just quit
            if (!RT.API.IsRemoteTechEnabled())
            {
                return;
            }

            // Get the active vessel
            Vessel vessel = FlightGlobals.ActiveVessel;
            var id = vessel.id;

            // Do we have manual control?
            if (RT.API.HasLocalControl(id))
            {
                return;
            }

            // Do we have a valid Connection?
            if(RT.API.HasAnyConnection(id))
            {
                return;
            }

            // Vessel doesn't have an active connection. 
            DoPartSearch(vessel);
        }

        private void DoPartSearch(Vessel vessel)
        {
            // Get all the antennas from this ship
            var communication = new List<ModuleRTAntenna>();
            foreach(var p in vessel.Parts)
            {
                var pm = p.Modules;
                for(int i=0; i < pm.Count; i++)
                {
                    var antenna = pm[i] as ModuleRTAntenna;
                    if(antenna != null)
                    {
                            communication.Add(antenna);
                    }
                }
            }

            // order them in priority, range first
            var dish = communication.OrderByDescending(x => x.RTDishRange).ThenBy(x => x.isActiveAndEnabled).FirstOrDefault();
            communication.Clear(); // clean up the list
            communication = null;
            var dishTarget = dish.RTAntennaTarget;

            // Assume that the current target is the last point we started at.
            if (FlightGlobals.Vessels != null && FlightGlobals.Vessels.Count > 1)
            {
                var vessels = FlightGlobals.Vessels.OrderBy(x => x.id).ToList();

                // loop until we find the current target.
                var loopVessels = 0;
                while (loopVessels < vessels.Count && dishTarget != vessels[loopVessels].id)
                {
                    loopVessels++;
                }

                // add one or, go back to the start of the list
                if (loopVessels + 1 > vessels.Count - 1)
                {
                    loopVessels = 0;
                }
                else
                {
                    loopVessels = loopVessels + 1;
                }

                // point the dish at this target. if it works, it works, if it doesn't we go around the loop again!
                dish.RTAntennaTarget = vessels[loopVessels].id;

                if (!dish.isActiveAndEnabled)
                {
                    // Activate it 
                    dish.isEnabled = true;
                }

                vessels.Clear();
                vessels = null;
            }

           
        }
    }
}