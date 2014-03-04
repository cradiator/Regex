using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regex
{
    class AutomataStatus
    {
        public AutomataStatus()
        {
            s_iCurrentStatusNumber++;
            m_iStatusNumber = s_iCurrentStatusNumber;
            m_mapInputTargetStatus = new Dictionary<char, List<AutomataStatus>>();
        }

        public void AddTransition(char cInputAscii, AutomataStatus targetStatus)
        {
            // Get the target status list for cInputAscii.
            List<AutomataStatus> list;
            if (m_mapInputTargetStatus.ContainsKey(cInputAscii))
            {
                list = m_mapInputTargetStatus[cInputAscii];
            }
            else
            {
                list = new List<AutomataStatus>();
                m_mapInputTargetStatus.Add(cInputAscii, list);
            }

            list.Add(targetStatus);
        }

        // Dictionary<input, target_status>
        Dictionary<char, List<AutomataStatus>> m_mapInputTargetStatus;
        private int m_iStatusNumber;
        private static int s_iCurrentStatusNumber = 0;
    }
}
