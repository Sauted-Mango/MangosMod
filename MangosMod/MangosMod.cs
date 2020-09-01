using BepInEx;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;


namespace MangosMod
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin(modAuthorCompiled, modName, modVersion)]
    public class MMod : BaseUnityPlugin
    {
        public const string authordiscord = "@sautedmango#4698";
        public const string authoremail = "sautedmango@gmail.com";
        public const string modName = "MangosMod";
        public const string modAuthor = "SautedMango";
        public const string modAuthorCompiled = "com." + modAuthor + "." + modName;
        //version format: [x.y.z] x=major release, y=feature addition, z=bugfix
        public const string modVersion = "6.2.3";

        public void Awake()
        {
            Debug.Log("Loaded " + modName + " " + modVersion);
            On.RoR2.Console.Awake += (orig, self) =>
            {
                CommandHelper.RegisterCommands(self);
                orig(self);
            };

            if (!RoR2Application.isModded)
            {
                RoR2Application.isModded = true;
            }

            Chat.onChatChanged += Chat_onChatChanged;
        }

        public void Update()//updates every frame
        {

        }

        [ConCommand(commandName = "mm", flags = ConVarFlags.ExecuteOnServer, helpText = "List information about the mod")]
        private static void MM(ConCommandArgs args)
        {
            Chat.AddMessage(
                "<style=cEvent>Author: <style=cShrine>" +
                modAuthor +
                "\n<style=cEvent>Mod: <style=cShrine>" +
                modName +
                "\n<style=cEvent>Version: <style=cShrine>" +
                modVersion +
                "\n<style=cEvent>-Contact Info- " +
                "\n<style=cEvent>Discord: <style=cShrine>" +
                authordiscord +
                "\n<style=cEvent>Email: <style=cShrine>" +
                authoremail +
                "</style></style></style></style></style></style></style></style></style></style></style>"
                );
        }

        [ConCommand(commandName = "mmhelp", flags = ConVarFlags.ExecuteOnServer, helpText = "List all commands")]
        private static void MMHelp(ConCommandArgs args)
        {
            Debug.Log(
                "All Commands:\n" +
                "<color=#5050ff>mm <style=cEvent>[lists information about the mod]</style></color>\n" +
                "<color=#5050ff>mmhelp <style=cEvent>[lists all available commands]</style></color>\n" +
                "<color=#5050ff>mmitems <color=#00fff6>{item name/id} {amount} {player}(optional) <style=cEvent>[gives items to a player]</style></color></color>\n" +
                "<color=#5050ff>mmitemslist <style=cEvent>[lists all items in ROR2]</style></color>\n" +
                "<color=#5050ff>mmtime <color=#00fff6>{int >0} <style=cEvent>[modifies the time scale depending on the integer given, 0=pause, 1=normal]</style></color></color>\n" +
                "<color=#5050ff>mm$ <color=#00fff6>{int >0} {player}(optional) <style=cEvent>[gives money to a player]</style></color></color>\n" +
                "<color=#5050ff>mmspawnas <color=#00fff6>{body} {player}(optional) <style=cEvent>[changes a player to a different character, !! DOES NOT MODIFY HEALTH !!]</style></color></color>\n" +
                "<color=#5050ff>mmspawnlist <style=cEvent>[lists all available bodies, and their alternate names]</style></color>\n" +
                "<color=#5050ff>mmequip <color=#00fff6>{name/id} {amount} {player}(optional) <style=cEvent>[gives equipment to a player]</style></color></color>\n" +
                "<color=#5050ff>mmequiplist <style=cEvent>[lists all available equipment]</style></color>"
                );
        }

        [ConCommand(commandName = "mmitems", flags = ConVarFlags.None, helpText = "Gives the specified item {name/id} {amount} {player}(optional)")]
        private static void MMItems(ConCommandArgs args)
        {
            string indexString = ArgsHelper.GetValue(args.userArgs, 0);
            string countString = ArgsHelper.GetValue(args.userArgs, 1);
            string playerString = ArgsHelper.GetValue(args.userArgs, 2);
            NetworkUser player = GetNetUserFromString(playerString);
            Inventory inventory = player != null ? player.master.inventory : args.sender.master.inventory;
            int itemCount = 1;

            if (!int.TryParse(countString, out itemCount))
                itemCount = 1;
            
            int itemIndex = 0;
            ItemIndex itemType = ItemIndex.Syringe;

            if (int.TryParse(indexString, out itemIndex))
            {

                if (itemIndex < (int)ItemIndex.Count && itemIndex >= 0)
                {
                    itemType = (ItemIndex)itemIndex;
                    inventory.GiveItem(itemType, itemCount);
                }

            }

            else if (Enum.TryParse<ItemIndex>(indexString, true, out itemType))
            {
                inventory.GiveItem(itemType, itemCount);
            }

            else
            {
                Debug.Log("Invalid arguments. <color=#5050ff>mmitems <color=#00fff6>{item name/id} {amount} {player}(optional) <style=cEvent>[gives items to a player]</style></color></color>");
            }

        }

        private static NetworkUser GetNetUserFromString(string playerString)
        {
            int result = 0;

            if (playerString != "")
            {

                if (int.TryParse(playerString, out result))
                {

                    if (result < NetworkUser.readOnlyInstancesList.Count && result >= 0)
                    {
                        return NetworkUser.readOnlyInstancesList[result];
                    }

                    Debug.Log("That player index does not exist");
                    return null;
                }

                else
                {

                    foreach (NetworkUser n in NetworkUser.readOnlyInstancesList)
                    {

                        if (n.userName.Equals(playerString, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return n;
                        }

                    }

                    Debug.LogFormat("That player, {0} ,does not exist", playerString);
                    return null;
                }

            }

            return null;
        }

        [ConCommand(commandName = "mmitemslist", flags = ConVarFlags.ExecuteOnServer, helpText = "Lists all items: 'id - name'")]
        private static void MMItemsList(ConCommandArgs args)
        {
            StringBuilder text = new StringBuilder();

            foreach (ItemIndex item in ItemCatalog.allItems)
            {
                int index = (int)item;
                string line = string.Format("{0} - {1}", index, item);
                text.AppendLine(line);
            }

            Debug.Log(text.ToString());
        }

        [ConCommand(commandName = "mmtime", flags = ConVarFlags.None | ConVarFlags.ExecuteOnServer, helpText = "Time acceleration or deceleration")]
        private static void MMTime(ConCommandArgs args)
        {
            string scaleString = ArgsHelper.GetValue(args.userArgs, 0);
            float scale = 1f;

            if (args.Count == 0)
            {
                Debug.Log(Time.timeScale);
                return;
            }

            if (float.TryParse(scaleString, out scale))
            {
                Time.timeScale = scale;
                Debug.Log(args.sender.userName + " set time scale to " + scale);
                Chat.AddMessage("<style=cWorldEvent>" + args.sender.userName + " set time scale to " + scale + "</style>");
            }

            else
            {
                Debug.Log("Invalid arguments, example: 'mmtime {int} (0=pause,1=normal)'");
            }

            NetworkWriter networkWriter = new NetworkWriter();
            networkWriter.StartMessage(101);
            networkWriter.Write((double)Time.timeScale);
            networkWriter.FinishMessage();
            NetworkServer.SendWriterToReady(null, networkWriter, QosChannelIndex.time.intVal);
        }

        [NetworkMessageHandler(msgType = 101, client = true, server = false)]
        private static void HandleTimeScale(NetworkMessage netMsg)
        {
            NetworkReader reader = netMsg.reader;
            Time.timeScale = (float)reader.ReadDouble();
        }

        public class ArgsHelper
        {

            public static string GetValue(List<string> args, int index)
            {

                if (index < args.Count && index >= 0)
                {
                    return args[index];
                }

                return "";
            }

        }

        [ConCommand(commandName = "mm$", flags = ConVarFlags.ExecuteOnServer, helpText = "Gives specified player some money")]
        private static void MMMoney(ConCommandArgs args)
        {

            if (args.Count == 0)
            {
                Debug.Log("You need to enter a valid int. mm$ {int >0} {player}(optional)");
                return;
            }

            string moneyString = ArgsHelper.GetValue(args.userArgs, 0);
            string playerString = ArgsHelper.GetValue(args.userArgs, 1);
            NetworkUser player = GetNetUserFromString(playerString);
            CharacterMaster master = player != null ? player.master : args.sender.master;
            uint result;

            if (uint.TryParse(moneyString, out result))
            {
                Chat.AddMessage("<style=cWorldEvent>" + args.sender.userName + " gave themselves $" + result + ".</style>");
                master.GiveMoney(result);
            }

        }

        [ConCommand(commandName = "mmspawnas", flags = ConVarFlags.ExecuteOnServer, helpText = "Respawn the specified player using the specified body prefab. ")]
        private static void MMSpawnAs(ConCommandArgs args)
        {

            if (args.Count == 0)
            {
                return;
            }

            string character = GetBodyName(args[0]);

            if (character == null)
            {
                Debug.Log("Please use mmspawnlist to view all bodies");
                return;
            }

            GameObject newBody = BodyCatalog.FindBodyPrefab(character);

            if (args.sender == null && args.Count < 2)
            {
                Debug.Log("Error");
                return;
            }

            CharacterMaster master = args.sender?.master;
            if (args.Count > 1)
            {
                NetworkUser player = GetNetUserFromString(args.userArgs, 1);

                if (player != null)
                {
                    master = player.master;
                }

                else
                {
                    Debug.Log("User not found");
                    return;
                }

            }

            if (!master.GetBody())
            {
                Debug.Log("User is dead");
                return;
            }

            master.bodyPrefab = newBody;
            Debug.Log(args.sender.userName + " is spawning as " + character);
            RoR2.ConVar.BoolConVar stage1pod = ((RoR2.ConVar.BoolConVar)(typeof(Stage)).GetFieldCached("stage1PodConVar").GetValue(null));
            bool oldVal = stage1pod.value;
            stage1pod.SetBool(false);

            master.Respawn(master.GetBody().transform.position, master.GetBody().transform.rotation);
            stage1pod.SetBool(oldVal);
        }

        internal static NetworkUser GetNetUserFromString(List<string> args, int startLocation = 0)
        {

            if (args.Count > 0 && startLocation < args.Count)
            {

                if (args[startLocation].StartsWith("\""))
                {
                    var startString = string.Join(" ", args);

                    var startIndex = startString.IndexOf('\"') + 1;
                    var length = startString.LastIndexOf('\"') - startIndex;

                    args[startLocation] = startString.Substring(startIndex, length);
                }

                if (int.TryParse(args[startLocation], out int result))
                {

                    if (result < NetworkUser.readOnlyInstancesList.Count && result >= 0)
                    {
                        return NetworkUser.readOnlyInstancesList[result];
                    }

                    Debug.Log("Player not found");
                    return null;
                }

                foreach (var n in NetworkUser.readOnlyInstancesList)
                {

                    if (n.userName.ToLower().Contains(args[startLocation].ToLower()))
                    {
                        return n;
                    }

                }

                return null;
            }

            return null;
        }

        public static string GetBodyName(string name)
        {
            string langInvar;
            int i = 0;

            foreach (var body in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {

                if (int.TryParse(name, out int iName) && i == iName || body.name.ToUpper().Equals(name.ToUpper()) || body.name.ToUpper().Replace("BODY", string.Empty).Equals(name.ToUpper()))
                {
                    return body.name;
                }

                i++;
            }

            StringBuilder s = new StringBuilder();

            foreach (var body in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                langInvar = GetLangInvar(body.baseNameToken);
                s.AppendLine(body.name + ":" + langInvar + ":" + name.ToUpper());

                if (body.name.ToUpper().Contains(name.ToUpper()) || langInvar.ToUpper().Contains(name.ToUpper()))
                {
                    return body.name;
                }

            }

            return null;
        }

        public static string GetLangInvar(string baseToken)
        {
            return RemoveSpacesAndAlike(Language.GetString(baseToken));
        }

        public static string RemoveSpacesAndAlike(string input)
        {
            return Regex.Replace(input, @"[ '-]", string.Empty);
        }

        //update list as of version 1.0
        [ConCommand(commandName = "mmspawnlist", flags = ConVarFlags.ExecuteOnServer, helpText = "List all available bodies")]
        private static void MMSpawnList(ConCommandArgs args)
        {
            Debug.Log(
                "All Available Bodies:\n" +
                "--Survivors--\n" +
                "Assassin\n" +
                "Commmando\n" +
                "Huntress\n" +
                "Engineer\n" +
                "MULT, MUL - T\n" +
                "Mercenary\n" +
                "Artificer\n" +
                "Bandit\n" +
                "Sniper\n" +
                "HAND, HAN-D\n" +
                "REX\n" +
                "Loader\n\n" +
                "--Other--\n" +
                "AncientWisp\n" +
                "ArchWisp\n" +
                "BeetleGuardAlly\n" +
                "BeetleGuard\n" +
                "Beetle\n" +
                "BeetleQueen\n" +
                "Bell\n" +
                "Bison\n" +
                "ClayBoss\n" +
                "Clayman\n" +
                "ClayBruiser\n" +
                "Commando\n" +
                "CommandoMonster\n" +
                "CommandoPerformanceTest\n" +
                "Drone1\n" +
                "Drone2\n" +
                "DroneBackup\n" +
                "DroneMissile\n" +
                "ElectricWorm\n" +
                "EngiBeamTurret\n" +
                "EngiTurret\n" +
                "EquipmentDrone\n" +
                "FlameDrone\n" +
                "Golem\n" +
                "Gravekeeper\n" +
                "GreaterWisp\n" +
                "HermitCrab\n" +
                "ImpBoss\n" +
                "Imp\n" +
                "Jellyfish\n" +
                "LemurianBruiser\n" +
                "LemurianBruiserFire\n" +
                "LemurianBruiserIce\n" +
                "LemurianBruiserPoison\n" +
                "Lemurian\n" +
                "MagmaWorm\n" +
                "MegaDrone\n" +
                "MercMonster\n" +
                "Shopkeeper\n" +
                "Spectator\n" +
                "SquidTurret\n" +
                "TitanGold\n" +
                "Titan\n" +
                "Turret1\n" +
                "UrchinTurret\n" +
                "Vagrant\n" +
                "Wisp");
        }

        [ConCommand(commandName = "mmequip", flags = ConVarFlags.ExecuteOnServer, helpText = "Give equipment to yourself or others")]
        private static void MMEquip(ConCommandArgs args)
        {
            string equipString = ArgsHelper.GetValue(args.userArgs, 0);
            string playerString = ArgsHelper.GetValue(args.userArgs, 1);
            NetworkUser player = GetNetUserFromString(playerString);
            Inventory inventory = player != null ? player.master.inventory : args.sender.master.inventory;
            int equipIndex = 0;
            EquipmentIndex equipType = EquipmentIndex.None;

            if (int.TryParse(equipString, out equipIndex))
            {
                if (equipIndex < (int)EquipmentIndex.Count && equipIndex >= -1)
                {
                    inventory.SetEquipmentIndex((EquipmentIndex)equipIndex);
                }

            }

            else if (Enum.TryParse<EquipmentIndex>(equipString, true, out equipType))
            {
                inventory.SetEquipmentIndex(equipType);
            }

            else
            {
                Debug.Log("Invalid arguments. mmequip {name/id} {amount} {player}(optional)");
            }

        }

        [ConCommand(commandName = "mmequiplist", flags = ConVarFlags.None, helpText = "Lists all equipment: 'id - name'")]
        private static void MMEquipList(ConCommandArgs args)
        {
            StringBuilder text = new StringBuilder();

            foreach (EquipmentIndex item in EquipmentCatalog.allEquipment)
            {
                int index = (int)item;
                string line = string.Format("{0} - {1}", index, item);
                text.AppendLine(line);
            }

            Debug.Log(text.ToString());
        }

        //CommandHelper
        //Credit goes to Wildbook for this piece (https://github.com/wildbook)
        //(they are not affiliated with "MangosMod" and do not support it in any way)
        public class CommandHelper
        {
            public static void RegisterCommands(RoR2.Console self)
            {
                var types = typeof(CommandHelper).Assembly.GetTypes();
                var catalog = self.GetFieldValue<IDictionary>("concommandCatalog");
                foreach (var methodInfo in types.SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)))
                {
                    var customAttributes = methodInfo.GetCustomAttributes(false);
                    foreach (var attribute in customAttributes.OfType<ConCommandAttribute>())
                    {
                        var conCommand = Reflection.GetNestedType<RoR2.Console>("ConCommand").Instantiate();
                        conCommand.SetFieldValue("flags", attribute.flags);
                        conCommand.SetFieldValue("helpText", attribute.helpText);
                        conCommand.SetFieldValue("action", (RoR2.Console.ConCommandDelegate)Delegate.CreateDelegate(typeof(RoR2.Console.ConCommandDelegate), methodInfo));
                        catalog[attribute.commandName.ToLower()] = conCommand;
                    }

                }

            }

        }

        private static Regex ParseChatLog => new Regex(@"<color=#[0-9a-f]{6}><noparse>(?<name>.*?)</noparse>:\s<noparse>(?<message>.*?)</noparse></color>");
        private void Chat_onChatChanged()
        {

            if (!Chat.readOnlyLog.Any())
            {
                return;
            }

            var chatLog = Chat.readOnlyLog;
            var match = ParseChatLog.Match(chatLog.Last());
            var playerName = match.Groups["name"].Value.Trim();
            var message = match.Groups["message"].Value.Trim();
            Debug.Log($"Chatlog={chatLog.Last()}, RMName={playerName}, RMMessage={message}");

            if (!string.IsNullOrWhiteSpace(playerName))
            {
                bool containsMM = message.StartsWith("mm");
                if (containsMM == true)
                {
                    Chat.AddMessage(
                        "<style=cEvent>Author: <style=cShrine>" +
                        modAuthor +
                        "\n<style=cEvent>Mod: <style=cShrine>" +
                        modName +
                        "\n<style=cEvent>Version: <style=cShrine>" +
                        modVersion +
                        "\n<style=cEvent>-Contact Info- " +
                        "\n<style=cEvent>Discord: <style=cShrine>" +
                        authordiscord +
                        "\n<style=cEvent>Email: <style=cShrine>" +
                        authoremail +
                        "</style></style></style></style></style></style></style></style></style></style></style>"
                        );
                }

                bool containsMMHELP = message.StartsWith("mmhelp");

                if (containsMMHELP == true)
                {
                    Chat.AddMessage(
                        "All Commands:\n" +
                        "<color=#5050ff>mm<style=cEvent> [lists information about the mod]</style></color>\n" +
                        "<color=#5050ff>mmhelp<style=cEvent> [lists all available commands]</style></color>\n" +
                        "<color=#5050ff>mmitems<color=#00fff6>,{item name/id},{amount},{player}(optional)<style=cEvent> [gives items to a player]</style></color></color>\n" +
                        "<color=#5050ff>mmitemslist<style=cEvent> [lists available items in the game]</style></color>\n" +
                        "<color=#5050ff>mmtime<color=#00fff6>,{int >0}<style=cEvent> [modifies the time scale depending on the integer given, 0=pause, 1=normal]</style></color></color>\n" +
                        "<color=#5050ff>mm$<color=#00fff6>,{int >0},{player}(optional)<style=cEvent> [gives money to a player]</style></color></color>\n" +
                        "<color=#5050ff>mmspawnas<color=#00fff6>,{body},{player}(optional)<style=cEvent> [changes a player to a different character, !! DOES NOT MODIFY HEALTH !!]</style></color></color>\n" +
                        "<color=#5050ff>mmspawnlist<style=cEvent> [lists all available bodies, and their alternate names]</style></color>\n" +
                        "<color=#5050ff>mmequip<color=#00fff6>,{name/id},{amount},{player}(optional)<style=cEvent> [gives equipment to a player]</style></color></color>\n" +
                        "<color=#5050ff>mmequiplist<style=cEvent> [lists available equipment in the game]</style></color>"
                        );
                }

                bool containsMMGIVEITEMS = message.StartsWith("mmitems");

                if (containsMMGIVEITEMS == true)
                {
                    //boilerplate arg splitter
                    string[] argsarray = message.Split(',');
                    string indexString = argsarray.GetValue(1).ToString();
                    string countString = argsarray.GetValue(2).ToString();
                    string playerString = argsarray.GetValue(3).ToString();
                    Chat.AddMessage($"{indexString} + {countString} + {playerString}");
                    //boilerplate arg splitter
                    NetworkUser player = GetNetUserFromString(playerString);
                    NetworkUser ogplayer = GetNetUserFromString(playerName);
                    Inventory inventory = player != null ? player.master.inventory : ogplayer.master.inventory;
                    int itemCount = 1;

                    if (!int.TryParse(countString, out itemCount))
                        itemCount = 1;

                    int itemIndex = 0;
                    ItemIndex itemType = ItemIndex.Syringe;

                    if (int.TryParse(indexString, out itemIndex))
                    {
                        if (itemIndex < (int)ItemIndex.Count && itemIndex >= 0)
                        {
                            itemType = (ItemIndex)itemIndex;
                            inventory.GiveItem(itemType, itemCount);
                        }

                    }

                    else if (Enum.TryParse<ItemIndex>(indexString, true, out itemType))
                    {
                        inventory.GiveItem(itemType, itemCount);
                    }

                    else
                    {
                        Chat.AddMessage("Invalid arguments. <color=#5050ff>mmitems <color=#00fff6>{item name/id} {amount} {player}(optional) <style=cEvent>[gives items to a player]</style></color></color>");
                    }

                }

                bool containsMMLISTITEMS = message.StartsWith("mmitemslist");

                if (containsMMLISTITEMS == true)
                {
                    Chat.AddMessage("Check the console for full list");
                    StringBuilder text = new StringBuilder();
                    foreach (ItemIndex item in ItemCatalog.allItems)
                    {
                        int index = (int)item;
                        string line = string.Format("{0} - {1}", index, item);
                        text.AppendLine(line);
                    }

                    Debug.Log(text.ToString());
                }

                bool containsMMTIME = message.StartsWith("mmtime");

                if (containsMMTIME == true)
                {
                    //boilerplate arg splitter
                    string[] argsarray2 = message.Split(',');
                    string scaleString = argsarray2.GetValue(1).ToString();
                    //boilerplate arg splitter
                    float scale = 1f;

                    if (scaleString == "")
                    {
                        Debug.Log(Time.timeScale);
                        return;
                    }

                    if (float.TryParse(scaleString, out scale))
                    {
                        Time.timeScale = scale;
                        Chat.AddMessage("<style=cWorldEvent>" + "Time scale set to " + scale + "</style>");
                    }

                    else
                    {
                        Chat.AddMessage("Invalid arguments, <color=#5050ff>mmtime<color=#00fff6>,{int >0} <style=cEvent>[modifies the time scale depending on the integer given, 0=pause, 1=normal]</style></color></color>");
                    }

                    NetworkWriter networkWriter = new NetworkWriter();
                    networkWriter.StartMessage(101);
                    networkWriter.Write((double)Time.timeScale);
                    networkWriter.FinishMessage();
                    NetworkServer.SendWriterToReady(null, networkWriter, QosChannelIndex.time.intVal);
                }

                bool containsMMMONEY = message.StartsWith("mm$");

                if (containsMMMONEY == true)
                {
                    //boilerplate arg splitter
                    string[] argsarray3 = message.Split(',');
                    string moneyString = argsarray3.GetValue(1).ToString();
                    string playerString = argsarray3.GetValue(2).ToString();
                    //boilerplate arg splitter

                    if (moneyString == "0")
                    {
                        Chat.AddMessage("You need to enter a valid int. <color=#5050ff>mm$<color=#00fff6>,{int >0},{player}(optional)<style=cEvent> [gives money to a player]</style></color></color>");
                        return;
                    }

                    NetworkUser player = GetNetUserFromString(playerString);
                    NetworkUser ogplayer = GetNetUserFromString(playerName);
                    CharacterMaster master = player != null ? player.master : ogplayer.master;
                    uint result;

                    if (uint.TryParse(moneyString, out result))
                    {
                        Chat.AddMessage("<style=cWorldEvent>" + ogplayer + " created $" + result + ".</style>");
                        master.GiveMoney(result);
                    }
                }

                bool containsMMSPAWNAS = message.StartsWith("mmspawnas");

                if (containsMMSPAWNAS == true)
                {
                    //boilerplate arg splitter
                    string[] argsarray4 = message.Split(',');
                    string bodyString = argsarray4.GetValue(1).ToString();
                    string playerString2 = argsarray4.GetValue(2).ToString();
                    //boilerplate arg splitter

                    if (bodyString == "")
                    {
                        return;
                    }

                    var character = Character.GetCharacter(bodyString);

                    if (character == null)
                    {
                        Chat.AddMessage("Could not spawn " + character.body + ".");
                        return;
                    }

                    NetworkUser player2 = GetNetUserFromString(playerString2);
                    NetworkUser ogplayer2 = GetNetUserFromString(playerName);
                    CharacterMaster master2 = player2 != null ? player2.master : ogplayer2.master;

                    if (master2.lostBodyToDeath)
                    {
                        Chat.AddMessage(playerString2 + " is <color=#8c0c0c>dead</color> and cannot respawn.");
                        return;
                    }

                    GameObject newBody = BodyCatalog.FindBodyPrefab(character.body);

                    if (newBody == null)
                    {
                        List<string> array = new List<string>();
                        foreach (var item in BodyCatalog.allBodyPrefabs)
                        {
                            array.Add(item.name);
                        }

                        string list = string.Join("\n", array);
                        Chat.AddMessage("Could not spawn as " + character.body + ". mangomod_spawn_as {body}\n" + list);
                        return;
                    }

                    master2.bodyPrefab = newBody;
                    master2.Respawn(master2.GetBody().transform.position, master2.GetBody().transform.rotation);
                    Chat.AddMessage("<style=cWorldEvent>" + playerName + " has spawned as " + character.body + "</style>");
                }

                bool containsMMSPAWNLIST = message.StartsWith("mmspawnlist");

                if (containsMMSPAWNLIST == true)
                {
                    Chat.AddMessage("Check the terminal for full list of bodies");
                    Debug.Log(
                        "All Available Bodies:\n" +
                        "--Survivors--\n" +
                        "Assassin\n" +
                        "Commmando\n" +
                        "Huntress\n" +
                        "Engineer\n" +
                        "MULT, MUL - T\n" +
                        "Mercenary\n" +
                        "Artificer\n" +
                        "Bandit\n" +
                        "Sniper\n" +
                        "HAND, HAN-D\n" +
                        "REX\n" +
                        "Loader\n\n" +
                        "--Other--\n" +
                        "AncientWisp\n" +
                        "ArchWisp\n" +
                        "BeetleGuardAlly\n" +
                        "BeetleGuard\n" +
                        "Beetle\n" +
                        "BeetleQueen\n" +
                        "Bell\n" +
                        "Bison\n" +
                        "ClayBoss\n" +
                        "Clayman\n" +
                        "ClayBruiser\n" +
                        "Commando\n" +
                        "CommandoMonster\n" +
                        "CommandoPerformanceTest\n" +
                        "Drone1\n" +
                        "Drone2\n" +
                        "DroneBackup\n" +
                        "DroneMissile\n" +
                        "ElectricWorm\n" +
                        "EngiBeamTurret\n" +
                        "EngiTurret\n" +
                        "EquipmentDrone\n" +
                        "FlameDrone\n" +
                        "Golem\n" +
                        "Gravekeeper\n" +
                        "GreaterWisp\n" +
                        "HermitCrab\n" +
                        "ImpBoss\n" +
                        "Imp\n" +
                        "Jellyfish\n" +
                        "LemurianBruiser\n" +
                        "LemurianBruiserFire\n" +
                        "LemurianBruiserIce\n" +
                        "LemurianBruiserPoison\n" +
                        "Lemurian\n" +
                        "MagmaWorm\n" +
                        "MegaDrone\n" +
                        "MercMonster\n" +
                        "Shopkeeper\n" +
                        "Spectator\n" +
                        "SquidTurret\n" +
                        "TitanGold\n" +
                        "Titan\n" +
                        "Turret1\n" +
                        "UrchinTurret\n" +
                        "Vagrant\n" +
                        "Wisp");
                }

                bool containsMMEQUIP = message.StartsWith("mmequip");

                if (containsMMEQUIP == true)
                {
                    //boilerplate arg splitter
                    string[] argsarray6 = message.Split(',');
                    string equipString = argsarray6.GetValue(1).ToString();
                    string playerString3 = argsarray6.GetValue(2).ToString();
                    //boilerplate arg splitter
                    NetworkUser player3 = GetNetUserFromString(playerString3);
                    NetworkUser ogplayer3 = GetNetUserFromString(playerName);
                    Inventory inventory = player3 != null ? player3.master.inventory : ogplayer3.master.inventory;
                    int equipIndex = 0;
                    EquipmentIndex equipType = EquipmentIndex.None;

                    if (int.TryParse(equipString, out equipIndex))
                    {
                        if (equipIndex < (int)EquipmentIndex.Count && equipIndex >= -1)
                        {
                            inventory.SetEquipmentIndex((EquipmentIndex)equipIndex);
                        }

                    }

                    else if (Enum.TryParse<EquipmentIndex>(equipString, true, out equipType))
                    {
                        inventory.SetEquipmentIndex(equipType);
                    }

                    else
                    {
                        Chat.AddMessage("Invalid arguments. <color=#5050ff>mmequip<color=#00fff6>,{name/id},{amount},{player}(optional)<style=cEvent> [gives equipment to a player]</style></color></color>");
                    }

                }

                bool containsMMEQUIPLIST = message.StartsWith("mmequiplist");

                if (containsMMEQUIPLIST == true)
                {
                    Chat.AddMessage("Check the console for full list");
                    StringBuilder text2 = new StringBuilder();

                    foreach (EquipmentIndex item in EquipmentCatalog.allEquipment)
                    {
                        int index = (int)item;
                        string line = string.Format("{0} - {1}", index, item);
                        text2.AppendLine(line);
                    }

                    Debug.Log(text2.ToString());
                }

            }

        }

    //}

        public class Character
        {
            public string body;
            public string master;
            public List<string> aliases;
            public Character(string _body, string _master, string[] _alises)
            {
                body = _body;
                master = _master;
                aliases = new List<string>(_alises);
            }

            public bool IsMatch(string name)
            {

                if (body.Equals(name, StringComparison.OrdinalIgnoreCase) || master.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                foreach (var alias in aliases)
                {
                    if (name.Equals(alias, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                }

                return false;
            }

            public static Character GetCharacter(string name)
            {
                foreach (var character in characters)
                {
                    if (character.IsMatch(name))
                        return character;
                }

                return new Character(name, name.Remove(-4) + "Master", new string[] { "" });
            }

            public static List<Character> characters = new List<Character>() {
            //survivors
            new Character("AssassinBody", "AssassinMaster", new string[] { "Assassin"}),
            new Character("CommandoBody", "CommandoMaster", new string[] { "Commando"}),
            new Character("HuntressBody", "HuntressMaster", new string[] { "Huntress"}),
            new Character("EngiBody", "EngiMaster", new string[] { "Engineer"}),
            new Character("ToolbotBody", "ToolbotMaster", new string[] { "MULT", "MUL-T"}),
            new Character("MercBody", "MercMaster", new string[] { "Merc", "Mercenary"}),
            new Character("MageBody", "MageMaster", new string[] { "Mage", "Artificer"}),
            new Character("BanditBody", "BanditMaster", new string[] { "Bandit"}),
            new Character("SniperBody", "SniperMaster", new string[] { "Sniper"}),
            new Character("HANDBody", "HANDMaster", new string[] { "HAND", "HAN-D"}),
            new Character("TreebotBody", "TreebotMaster", new string[] { "REX"}),
            new Character("LoaderBody", "LoaderMaster", new string[] { "Loader"}),
            //others
            new Character("AncientWispBody", "AncientWispMaster", new string[] { "AncientWisp" }),
            new Character("ArchWispBody", "ArchWispMaster", new string[] { "ArchWisp" }),
            new Character("BeetleGuardAllyBody", "BeetleGuardAllyMaster", new string[] { "BeetleGuardAlly"}),
            new Character("BeetleGuardBody", "BeetleGuardMaster", new string[] { "BeetleGuard"}),
            new Character("BeetleBody", "BeetleMaster", new string[] { "Beetle"}),
            new Character("BeetleQueen2Body", "BeetleQueenMaster", new string[] { "BeetleQueen"}),
            new Character("BellBody", "BellMaster", new string[] { "Bell"}),
            new Character("BisonBody", "BisonMaster", new string[] { "Bison"}),
            new Character("ClayBossBody", "ClayBossMaster", new string[] { "ClayBoss"}),
            new Character("ClayBody", "ClaymanMaster", new string[] { "Clayman"}),
            new Character("ClayBruiserBody", "ClayBruiserMaster", new string[] { "ClayBruiser"}),
            new Character("CommandoBody", "CommandoMaster", new string[] { "Commando"}),
            new Character("CommandoBody", "CommandoMonsterMaster", new string[] { "CommandoMonster"}),
            new Character("CommandoPerformanceTestBody", "CommandoMonsterMaster", new string[] { "CommandoPerformanceTest"}),
            new Character("Drone1Body", "Drone1Master", new string[] { "Drone1"}),
            new Character("Drone2Body", "Drone2Master", new string[] { "Drone2"}),
            new Character("BackupDroneBody", "DroneBackupMaster", new string[] { "DroneBackup"}),
            new Character("MissileDroneBody", "DroneMissileMaster", new string[] { "DroneMissile"}),
            new Character("ElectricWormBody", "ElectricWormMaster", new string[] { "ElectricWorm"}),
            new Character("EngiBeamTurretBody", "EngiBeamTurretMaster", new string[] { "EngiBeamTurret"}),
            new Character("EngiTurretBody", "EngiTurretMaster", new string[] { "EngiTurret"}),
            new Character("EquipmentDroneBody", "EquipmentDroneMaster", new string[] { "EquipmentDrone"}),
            new Character("FlameDroneBody", "FlameDroneMaster", new string[] { "FlameDrone"}),
            new Character("GolemBody", "GolemMaster", new string[] { "Golem"}),
            new Character("GravekeeperBody", "GravekeeperMaster", new string[] { "Gravekeeper"}),
            new Character("GreaterWispBody", "GreaterWispMaster", new string[] { "GreaterWisp"}),
            new Character("HermitCrabBody", "HermitCrabMaster", new string[] { "HermitCrab"}),
            new Character("ImpBossBody", "ImpBossMaster", new string[] { "ImpBoss"}),
            new Character("ImpBody", "ImpMaster", new string[] { "Imp"}),
            new Character("JellyfishBody", "JellyfishMaster", new string[] { "Jellyfish"}),
            new Character("LemurianBruiserBody", "LemurianBruiserMaster", new string[] { "LemurianBruiser"}),
            new Character("LemurianBruiserBody", "LemurianBruiserMasterFire", new string[] { "LemurianBruiserFire"}),
            new Character("LemurianBruiserBody", "LemurianBruiserMasterIce", new string[] { "LemurianBruiserIce"}),
            new Character("LemurianBruiserBody", "LemurianBruiserMasterPoison", new string[] { "LemurianBruiserPoison"}),
            new Character("LemurianBody", "LemurianMaster", new string[] { "Lemurian"}),
            new Character("MagmaWormBody", "MagmaWormMaster", new string[] { "MagmaWorm"}),
            new Character("MegaDroneBody", "MegaDroneMaster", new string[] { "MegaDrone"}),
            new Character("MercBody", "MercMonsterMaster", new string[] { "MercMonster"}),
            new Character("ShopkeeperBody", "ShopkeeperMaster", new string[] { "Shopkeeper"}),
            new Character("SpectatorBody", "SpectatorMaster", new string[] { "Spectator"}),
            new Character("SquidTurretBody", "SquidTurretMaster", new string[] { "SquidTurret"}),
            new Character("TitanGoldBody", "TitanGoldMaster", new string[] { "TitanGold"}),
            new Character("TitanBody", "TitanMaster", new string[] { "Titan"}),
            new Character("Turret1Body", "Turret1Master", new string[] { "Turret1"}),
            new Character("UrchinTurretBody", "UrchinTurretMaster", new string[] { "UrchinTurret"}),
            new Character("VagrantBody", "VagrantMaster", new string[] { "Vagrant"}),
            new Character("WispBody", "WispMaster", new string[] { "Wisp" })
            };

        }

    }

}