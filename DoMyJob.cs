
using Styx.Common;
using Styx.CommonBot;
using Styx.Plugins;
using Styx.WoWInternals.TradeSkills;
using Styx.WoWInternals.WoWObjects;
using Styx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Styx.CommonBot.Profiles;

namespace DoMyJob
{
    public class DoMyJob : HBPlugin
    {
        private int oldLevel, mLevel, mMaxLevel;
        private double DefaultTradeskillLevelByLevel;
        private bool startupCheck;
        private static List<MiningNodes> _mNodes = new List<MiningNodes>();
        private static WoWObject _currentMiningNode;
        public static BotBase Current { get; }
        public string saveUsedBot, saveUsedProfile;

        public override string Author { get { return "HaraldKutt"; } }

        public override string Name { get { return "DoMyJob -  a profession Plugin"; } }

        public override Version Version { get { return new Version(1,0); } }
        public override string ButtonText { get { return "Settings"; } }
        public override bool WantButton { get { return true; } }

        public LocalPlayer Me { get { return StyxWoW.Me; } }
        public TradeSkill MiningTradeSkill { get { return TradeSkill.GetTradeSkill(SkillLine.Mining);  } }
        public TradeSkill HerbalismTradeSkill { get { return TradeSkill.GetTradeSkill(SkillLine.Herbalism); } }
        public static string usedBot { get { return BotManager.Current.Name.ToUpper(); } }



        public override void Pulse()
        {
            // if any of this is true, then we won't run the code   
            if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld || Me == null || !Me.IsValid || Me.Combat)
            {
                return;
            }
            if (!LevelChanged(Me.Level)) { return; } // runs one time at startup after that on Levelchange
            if (!(mLevel < GetDefaultLevelByLevel(Me.Level))) { return; } //check this because Player may Level manually or HB gathers stuff
            SaveUsedBotAndProfile();

            var gatherBot = BotManager.Instance.Bots.Where(kvp => kvp.Value.GetType().Name == "GatherbuddyBot").Select(kvp => kvp.Value).FirstOrDefault();

            if (gatherBot != null && (BotManager.Current.Name != "Gatherbuddy2"))
            {    
                cbb("Gatherbuddy2", "test");
            }
            else infoLog("Gatherbuddy null and not Found");
            
            //getSkillLevel

        }



        public static void cbb(string botbase, string profile)
        {
            foreach (var Base in BotManager.Instance.Bots.Where(Base => Base.Key.ToLower().Contains(botbase.ToLower())))
            {
                var @base = Base;

                ThreadPool.QueueUserWorkItem(o => {
                    TreeRoot.Stop();
                    TreeRoot.WaitForStop();
                    BotManager.Instance.SetCurrent(@base.Value);
                    //ProfileManager.LoadNew(Path.Combine(Path.GetDirectoryName(ProfileManager.XmlLocation), profile + ".xml"));
                    ProfileManager.LoadNew("Plugins/DoMyJob/mining/140.xml", true);
                    TreeRoot.Start();
                });
                break;
            }
        }

        public void SaveUsedBotAndProfile()
        {
            saveUsedBot = BotManager.Current.ToString();
            saveUsedProfile = ProfileManager.CurrentProfile.ToString();
            infoLog("Bot: {0} and Profile: {1} saved", saveUsedBot, saveUsedProfile);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            infoLog("Enabled!");
            startupCheck = true;
            oldLevel = Me.Level; // Do a xml savefile check later on here
            /* The Profession List Thingy */
            // CheckMiningSkill;
            // CheckHerbalismSkill;
            if( Me.GetSkill(SkillLine.Mining)!= null && MiningTradeSkill.MaxLevel != 0)
            {
                // MOVE this to serperate File later
                mLevel = MiningTradeSkill.Level;
                mMaxLevel = MiningTradeSkill.MaxLevel;
                infoLog("Found Mining! Yeah! Level: {0} von {1}", mLevel, mMaxLevel);
                _mNodes.Clear();
                //Classic
                _mNodes.Add(new MiningNodes() { ID = 1731,      Name = "Copper Vein" });
                _mNodes.Add(new MiningNodes() { ID = 181248,    Name = "Copper Vein" }); // Ghostlands
                _mNodes.Add(new MiningNodes() { ID = 1732,      Name = "Tin Vein" });
                _mNodes.Add(new MiningNodes() { ID = 181249,    Name = "Tin Vein" }); // Ghostlands
                _mNodes.Add(new MiningNodes() { ID = 1733,      Name = "Silver Vein" });
                _mNodes.Add(new MiningNodes() { ID = 1734,      Name = "Gold Vein" });
                _mNodes.Add(new MiningNodes() { ID = 1735,      Name = "Iron Deposite" });
                _mNodes.Add(new MiningNodes() { ID = 2040,      Name = "Mithril Deposite" });
                _mNodes.Add(new MiningNodes() { ID = 2047,      Name = "Truesilver Deposite" });
                _mNodes.Add(new MiningNodes() { ID = 165658,    Name = "Dark Iron Deposite" });
                _mNodes.Add(new MiningNodes() { ID = 324,       Name = "Small Thorium Vein" });
                _mNodes.Add(new MiningNodes() { ID = 175404,    Name = "Rich Thorium Vein" });
                _mNodes.Add(new MiningNodes() { ID = 123848,    Name = "Ooze Covered Thorium Vein" });
                _mNodes.Add(new MiningNodes() { ID = 177388,    Name = "Ooze Covered Rich Thorium Vein" });
                //BC
                //LK
                //Cata
                //Panda
                //WoD
                //Legion
            }
        }
        public override void OnDisable()
        {
            base.OnDisable();
            infoLog("Disabled!");
        }

        public static void infoLog(string message, params object[] args)
        {
            Logging.Write(System.Windows.Media.Colors.White, "[DMJ] " + message, args);
        }
        public static void debugLog(string message, params object[] args)
        {
            Logging.WriteQuiet("[DMJ] " + message, args);
        }

        private bool LevelChanged(int MyCurLevel)
        {
            if ((oldLevel == MyCurLevel) && (startupCheck == false))
            {
                return false;
            }
            if (oldLevel > MyCurLevel || oldLevel < 1 || oldLevel > 110)
            {
                infoLog("Something went terribly wrong - stored Level {0} > Current Level {1}", oldLevel, MyCurLevel);
                return false;
            }
            if (oldLevel < MyCurLevel || ((oldLevel == MyCurLevel) && startupCheck))
            {
                if (startupCheck)
                infoLog("Proceed Startup check."); 
                else
                infoLog("We leveld up. Do the profession task."); 
                return true;
            }
                return false;


        }
        bool IsEntryAMiningNode(uint id)
        {
            foreach (var item in _mNodes)
            {
                if (item.ID == id)
                {
                    return true;
                }

            }
            return false;
        }
        public static bool MoveBot
        {
            get
            {
                return usedBot.Contains("QUEST") || usedBot.Contains("GATHER") || usedBot.Contains("GRIND") || usedBot.Contains("ARCH");
            }
        }
        private double GetDefaultLevelByLevel(int PlayerLevel)
        {
            DefaultTradeskillLevelByLevel = PlayerLevel * 7.27272727; //in fact its 7.27......
            infoLog("Default TradeskillLevel: {0} at Playerlevel: {1}", DefaultTradeskillLevelByLevel, PlayerLevel);
            return Math.Round(DefaultTradeskillLevelByLevel,0);  
        }
    }
    public class MiningNodes
    {
        public uint ID { get; set; }
        public string Name { get; set; }
    }
}
