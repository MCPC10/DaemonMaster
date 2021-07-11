/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: Config
//  
//  This file is part of DeamonMaster.
// 
//  DeamonMaster is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//   DeamonMaster is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with DeamonMaster.  If not, see <http://www.gnu.org/licenses/>.
/////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;

namespace DaemonMaster.Config
{
    public class Config
    {
        private uint _updateInterval = 5;

        public uint UpdateInterval
        {
            get
            {
                if (_updateInterval < 1)
                    return 1;

                return _updateInterval;
            }

            set => _updateInterval = value;
        }

        public string Language { get; set; } = "windows";

        public bool DisableCheckForUpdates { get; set; } = false;

        public bool ActivateLegacyFunctions { get; set; } = false;
        public bool UseExperimentalFunctions { get; set; } = false;

        [JsonIgnore]
        public bool UseOldNameBasedSearchSystemWithTheNewSystem { get; set; } = false;

        [JsonProperty("UseOldNameBasedSearchSystemWithTheNewSystem")]
        private bool UseOldNameBasedSearchSystemWithTheNewSystemAlternateSetter
        {
            set => UseCompatibilityModeForSearchSystem = value;
        }

        public bool UseCompatibilityModeForSearchSystem { get; set; } = false;
        public bool UnlockInteractiveServiceCreationOnNotSupportedSystem { get; set; } = false;
    }
}
