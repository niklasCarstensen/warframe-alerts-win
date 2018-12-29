﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warframe_Alerts
{
    public class configData
    {
        // TODO: Add your variables here
        public bool GameDetection;
        public int UpdateInterval;
        public bool startMinimized;
        public bool desktopNotifications;
        public List<string> Filters;
        public bool VoidTraderArrived;
        public List<string> idList;

        public configData()
        {
            // TODO: Add initilization logic here
            UpdateInterval = 1 * 60 * 1000;
            GameDetection = true;
            startMinimized = false;
            Filters = new List<string>();
            desktopNotifications = true;
            idList = new List<string>();
        }
    }
}
