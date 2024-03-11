using MissionPlanner;
using MissionPlanner.Plugin;
using MissionPlanner.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using MissionPlanner.Controls.PreFlight;
using MissionPlanner.Controls;
using System.Linq;
using GMap.NET;
using System.Windows.Forms.PropertyGridInternal;
using MissionPlanner.GCSViews;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static MissionPlanner.Utilities.Pelco;
using MissionPlanner.Warnings;
using System.ComponentModel;
using CsvHelper;
using static MissionPlanner.Utilities.GStreamer;

namespace VSTE
{
    public class VSTE : Plugin
    {
        public override string Name
        {
            get { return "VSTE"; }
        }
        public override string Version
        {
            get { return "0.0.1.0"; }
        }
        public override string Author
        {
            //Caelan Desmond, Brandon Fenske, Joshua Budd, Lance Wilhelm, Maaz Chohan
            get { return "ASU Capstone Team"; }
        }

        //[DebuggerHidden]
        public override bool Init()
		//Init called when the plugin dll is loaded
        {
            loopratehz = 1;  //Loop runs every second (The value is in Hertz, so 2 means every 500ms, 0.1f means every 10 second...) 
            
            return true;	 // If it is false then plugin will not load
        }

        public override bool Loaded()
		//Loaded called after the plugin dll successfully loaded
        {
            TowerButtonSetup();
            FDActionTabSetup();
            FPCommandSetup();
            //TODO Later
            /*
            //HUD UI
            //Host.cs.customfield0;
            //Sound stuff try warning manager
            //Not Testable till grounded Drone test
            //CustomWarning cw = new CustomWarning();
            //CustomWarning.defaultsrc = Host.cs.customfield0;
            //cw.RepeatTime = 0;
            //cw.ConditionType = CustomWarning.Conditional.EQ;
            //cw.type = CustomWarning.WarningType.SpeakAndText;
            */
            

            return true;     //If it is false plugin will not start (loop will not called)
        }

        private void TowerButtonSetup()
        {
            var FDbut = new ToolStripMenuItem("Edit Tower Locations");
            FDbut.Click += EditTower_Click;
            Host.FDMenuMap.Items.Add(FDbut);

            var FPbut = new ToolStripMenuItem("Edit Tower Locations");
            FPbut.Click += EditTower_Click;
            Host.FPMenuMap.Items.Add(FPbut);
        }

        private void FDActionTabSetup()
        {
            ComboBox VSTEdropDown = new ComboBox();
            VSTEdropDown.DropDownStyle = ComboBoxStyle.DropDownList;
            VSTEdropDown.DropDownWidth = 150;
            VSTEdropDown.FormattingEnabled = true;
            VSTEdropDown.Items.Clear();//Removes the blank spot
            VSTEdropDown.Items.Add("ACTIVE");
            VSTEdropDown.Items.Add("PASSIVE");
            VSTEdropDown.Items.Add("STANDBY");
            Host.MainForm.FlightData.tabActions.Controls[0].Controls.Add(VSTEdropDown);
            MyButton setModeActionBut = new MyButton();
            setModeActionBut.Name = "SetVSTEmodeSetter";
            setModeActionBut.Text = "Set VSTE Mode";
            setModeActionBut.Click += SetVSTEmode_Click;
            Host.MainForm.FlightData.tabActions.Controls[0].Controls.Add(setModeActionBut);
        }

        private void FPCommandSetup()
        {
            //Based on MavCommandSelection.cs
            Dictionary<string, string[]> commands = new Dictionary<string, string[]>();
            Dictionary<string, ushort> extraCommands = new Dictionary<string, ushort>();
            string cmdName = "VSTE_ACTIVE";
            ushort cmdId = 1;
            string[] p = new string[7];
            p[0] = "Time Interval";
            p[1] = "";
            p[2] = "";
            p[3] = "";
            p[4] = "";
            p[5] = "";
            p[6] = "";
            commands.Add(cmdName, p);
            if (!Enum.IsDefined(typeof(MAVLink.MAV_CMD), cmdId))
            {
                extraCommands.Add(cmdName, cmdId);
            }
            cmdName = "VSTE_PASSIVE";
            cmdId = 2;
            p = new string[7];
            p[0] = "Distance Interval";
            commands.Add(cmdName, p);
            if (!Enum.IsDefined(typeof(MAVLink.MAV_CMD), cmdId))
            {
                extraCommands.Add(cmdName, cmdId);
            }
            cmdName = "VSTE_STANDBY";
            cmdId = 3;
            p = new string[7];
            p[0] = "";
            commands.Add(cmdName, p);
            if (!Enum.IsDefined(typeof(MAVLink.MAV_CMD), cmdId))
            {
                extraCommands.Add(cmdName, cmdId);
            }
            Settings.Instance["PlannerExtraCommand"] = JsonConvert.SerializeObject(commands);
            Settings.Instance["PlannerExtraCommandIDs"] = JsonConvert.SerializeObject(extraCommands);
            Host.MainForm.FlightPlanner.updateCMDParams();
        }

        void EditTower_Click(object sender, EventArgs e)
        {
            //Input Section
            string Name = "";
            PointLatLng pointLatLng = Host.FDMenuMapPosition;
            if (pointLatLng == null)//If menu was selected from the FPMenu
            {
                pointLatLng = Host.FPMenuMapPosition;
            }
            double Lat = pointLatLng.Lat;
            double Long = pointLatLng.Lng;
            double Height = 0;
            double Range = 25;
            InputBox.Show(title: "Tower Information", promptText: "Tower Name", ref Name);
            InputBox.Show(title:"Tower Information", promptText:"Input Latitude", ref Lat);
            InputBox.Show(title:"Tower Information", promptText:"Input Longitude", ref Long);
            InputBox.Show(title: "Tower Information", promptText: "Input Height", ref Height);
            InputBox.Show(title:"Tower Information", promptText:"Input Projected Range", ref Range);
            Tower newTower = new Tower(Name, Lat, Long, Height, Range);

            //Add to csv
            CustomMessageBox.Show(Directory.GetCurrentDirectory());

            using (var reader = new StreamReader("C:\\Users\\caela\\source\\repos\\MissionPlanner\\Plugins\\VSTE MP Plugin Version 0.0.0.1\\VSTE\\TowerConfig.csv"))
            {
                CustomMessageBox.Show(reader.ReadToEnd());
                reader.Close();
            }

            using (var writer = new StreamWriter("C:\\Users\\caela\\source\\repos\\MissionPlanner\\Plugins\\VSTE MP Plugin Version 0.0.0.1\\VSTE\\TowerConfig.csv"))
            {

            }

            //"TowerConfig.csv"

        }

        void SetVSTEmode_Click(object sender, EventArgs e)
        {
            CustomMessageBox.Show("I made it");
        }
        public override bool Loop()
		//Loop is called in regular intervalls (set by loopratehz)
        {
            return true;	//Return value is not used
        }
        public override bool Exit()
		//Exit called when plugin is terminated (usually when Mission Planner is exiting)
        {
            return true;	//Return value is not used
        }
    }
    public class Tower
    {
        public string Name;
        public double Lat;
        public double Long;
        public double Height;
        public double Range;

        public Tower(string name, double lat, double @long, double height, double range)
        {
            Name = name;
            Lat = lat;
            Long = @long;
            Height = height;
            Range = range;
        }

    }
}