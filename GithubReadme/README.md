![https://i.imgur.com/CkSehPu.png](https://i.imgur.com/CkSehPu.png)

<h1>

 [GITHUB](https://github.com/war3i4i/Marketplace)
</h1>

![https://i.imgur.com/dBf99Od.png](https://i.imgur.com/dBf99Od.png)

Like my mods? Support me:
Paypal: war3spells@gmail.com 
### IF YOU WANT TO USE MOD IN SINGLEPLAYER IT CAN PROVIDE 50% OF FUNCTIONALITY WHILE USING THAT ON CLIENTSIDE IF YOU HAVE OPTION TURNED ON IN MAIN CONFIG
## Mod adds different NPCs and Unique mechanics to server so admins can configure them from serverside with no need to restart server for applying settings:

<details>
  <summary><b><span style="color:aqua;font-weight:200;font-size:20px">
    Patchnotes
</span></b></summary>


| Version     |     | Changes                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
|-------------|:----|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 8.7.0       |     | Leaderboard system added with custom Achievements system<br/>Added players tag system<br/>Traders now can sell Skill EXP<br/>Added new folder: AdditionalConfigs with Quests/Dialogues/Territories folders where you can put additional .cfg files for corresponding NPC type (so you don't flood your main config file and split / manage it better)<br/>Added Color attribute to dialogues<br/>Added new dialogues commands / conditions<br/>Added <image=link> tag for server info<br/>Now if server info tag named [OnPlayerFirstSpawn] it will show UI when player first joins server<br/>Bugfixes / optimizations<br/>Transmogrification is now a free feature<br/>Added Transmog to DistancedUI<br/>Now you can use ! sign before dialogue condition to simply reverse it<br/>Added gradients for Territory System colors                                                                                                                                                                                                                                                                                                                                      |
| 8.6.3       |     | Posted / updated NPC Dialogues + Territory System guides on site<br/>Added config option to specify banker interest items (All by default)<br/>Added config option to set mailbox item wait time<br/>Fixed gambler code<br/>Now working with wackydb 2.0 (beta) cloned items                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| 8.6.0       |     | New system added: Mailbox<br/>Finished NPC Dialogues system<br/>Bugfixes<br/>Fixed Banker interest not working<br/>Now Marketplace can use SOME of its features locally on client (to enable set config option to true on clientside)<br/>New Quest Restriction - Time: value, allows quest to be time limited<br/>Added NPC font support for chinese symbols and other languages special symbols<br/>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| 8.5.0       |     | New system added: NPC Dialogue (guide soon)<br/>New system added: Item Mocking (guide soon)<br/>Fixed banker multiplier bug<br/>Fixed KGchat text overflow                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| 8.4.0       |     | Player Territories removed. Please do not install this version until you replace Player Territories module on something else (Azumatt wards / e.t.c) (TerritoryDatabase is same and working, just not the players one)<br/>Added KGchat as part of marketplace. Its enabled by default but you can turn it off in Main config on serverside. You can replace KGchat emojis in BepInEx/Config/MarketplaceEmojis. You will find spritesheet_original.png there, change pics on what you need and rename it to spritesheet.png<br/>Added 2 new fields to fashion UI: Periodic Sound + Periodic Sound Time<br/>Added new quest event: NpcText<br/>Optimized mod by rewriting it almost from scratch. Now mod is open-source, check: https://github.com/war3i4i/Marketplace for code<br/>Added API for territories so other mods may use it (check github)<br/>NPC's now won't show up in hammer menu if Debug Mode is turned off<br/><br/>Transmogrification system access has changed (now transmogrification is a separated DLL). If you bought Transmog access before this patch please contact me in discord KG#7777 so i can send you mod to enable Transmog         |
| 8.3.3       |     | Added Groups API for Kill type quests                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| 8.3.2       |     | Quest descriptions now may have \n as new line<br/>Territory minimap text fix<br/>Fixed NPC save/load UI problems<br/>Fixed Teleporter map names showup                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| 8.3.0       |     | Updated for new Valheim version<br/>Bugfixes<br/>Added  Marketplace_GOBLIN, Marketplace_SKELETON, Marketplace_QUESTBOARD, Marketplace_TELEPORTER, Marketplace_DEFAULTNPC as separated models that you can use to override NPC model                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 8.2.8       |     | All data in DO NOT TOUCH folder now decrypted. Keep in mind that you can't change that in runtime and if you edit .json file then do it on your own risk<br/>Changed NPC Save / Load UI, changed Marketplace UI, changed Premium UI<br/>Added IsVIP restriction for quests (quest will be shown only for VIP's)<br/>Fixed trader NeedToKnowMaterial items appear if player doesn't know materials<br/>Now you can buy particular amount of items from stack in Marketplace<br/>Updated KeyManager<br/>Items in Marketplace cannot be Expired anymore                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| 8.2.7       |     | Added new trader format, now Trader may have up to 5 items to exchange in left and right side, also left side items may now also have level required<br/>Quests now may have multiple targets per one quest as rewards and requirements (same format with adding)<br/>Reworked Marketplace UI visuals<br/>Fixed a bug where marketplace prevented items from being able to change rotation / roll<br/>Some code optimizations<br/>Now if you press RIGHT mouse button on "Receive Income" button in Marketplace then income will be added directly to your banker                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| 8.2.6       |     | Updated to latest Valheim live version<br/>Added new <image=link> tag for quest name to show preview image<br/>Added PutAll button to Banker<br/>Added Periodic animation to NPC Fashion UI<br/>Fixed Premium UI syncing<br/>Added new territory flag: NoCreatureDrops                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| 8.2.4       |     | Added MagicHeim API (Quest Reward Add MagicHeim EXP, Quest Restriction MagicHeim Level)<br/>Fixed compatibility issue with Marketplace Territories and Jere's ExpandWorld                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 8.2.3       |     | Fixed Jewelcrafting compatibility. <br/>Added new VFX id: 21 to Transmogrification that allows people to chooce any effect manually. <br/>Fixed player territories map showup issue                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 8.2.1       |     | Fixed quest autocomplete tag problem on most quest types. Now it properly works on all Kill, Collect, Craft, Build type quests                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| 8.2.0       |     | Now mod compatible with mistlands update<br/>Updated NPC + NPC Fashion UI's<br/>Now Marketplace also saves Crafter Name + Crafter ID<br/>Updated transmog to use ItemDataManager. After update all transmogrified items will be nullified. But because of using ItemDataManager now transmog wont disappear when you upgrade an item + will have much less bugs (armor stand ad so on)<br/>New Territory flags added: NoMist, InfiniteEitr, InfiniteStamina<br/>Small Localization update                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 8.1.2       |     | Fixed critical bug that didn't allow players to join server                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| 8.1.1       |     | Returned Quest Journal (a little changed)<br/>Fixed NPC sound reverb problem<br/>Fixed player getting skill experience while attacking NPC                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| 8.1.0       |     | <span style="color:red;">BEFORE INSTALLING 8.1.0 VERSION MAKE SURE TO REMOVE ALL ITEMS FROM MARKETPLACE SINCE AFTER UPDATE IT WILL REMOVE ITEMS OWNERSHIP FROM ALL USERS. ALSO DO THE SAME FOR ALL PLAYER CREATED TERRITORIES<br/></span>New NPC (System) Added: Transmogrification (Paid feature only)<br/>New System added: Quest Events<br/>New quest reward added: Skill_EXP<br/>New quest restriction added: NotFinished<br/>Bugfixes<br/>Now NPC Sounds are mp3 files instead of wav<br/>Now Territories with at least one color less than 0 wont be displayed on map<br/>Added tooltips on hover on any quest reward or trader item<br/>If you will write [questID=autocomplete] then quest will be considered finished without completing it in NPC UI, it will be completed immediately when your quest target is done<br/>                                                                                                                                                                                                                                                                                                                                  |
| 8.0.2       |     | Added few log lines for PremiumSystem                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| 8.0.0       |     | 1) Bugfixes<br/>2) Added new Premium System: Distanced UI that can use NPC profiles without interacting with NPCs. To use go to MarketplaceKG/PremiumSystem/ folder to  edit .cfg file. Hotkey to open UI is L. Alt + ~<br/>3) Added new NPC UI : Save/Load. Opens with C + Interact. Allows you to save NPC appearance and then load it back on another NPC. To save ALL NPCs in your location write /npc save in chat<br/>4) Replaced old localization on LocalizationManager. Now you can add your own localization. For that download file: https://pastebin.com/7z08xMQq . Place it into Valheim/BepInEx/config/ folder and name it MarketplaceAndServerNPCs.YOURLANGUAGE.yml . Then you can translate lines to make your own language localization                                                                                                                                                                                                                                                                                                                                                                                                              |
</details>

<span style="color: bisque;">
Now you can add your own localization. For that download file: <a href="MarketplaceAndServerNPCs.English.yml" download>Translation</a>.<br>Place it into Valheim/BepInEx/config/ folder and name it MarketplaceAndServerNPCs.YOURLANGUAGE.yml . Then you can translate lines to make your own language localization
</span>



<h1>

[Stonedprophet](https://www.youtube.com/@therealstonedprophet) made an amazing video-guide on this mod. You can check them out here: 
<details><summary><span style="color:yellow;font-weight:300;font-size:35px">Stonedprophet tutorials:</span></summary>
<p> 

1) https://youtu.be/5fR_9Qygkro (part one)
2) https://youtu.be/BthPUGOeaeA (part two)
3) https://youtu.be/hUU_bPCwFeE (part three)
4) https://youtu.be/ZgoeYVpEcI4 (part four)
5) https://youtu.be/xdj2CccUYhk (part five)
6) https://youtu.be/0COuBKO3Gpg (part six - Dialogues)

</p>
</details>
</h1>



_________________________________
![https://i.imgur.com/iWZO1dp.png](https://i.imgur.com/iWZO1dp.png)

<details><summary>Installation and main configs:</summary>
<p> 

1) Ship plugin to all clients and on your dedicated server
2) After server restart, new folder in BepInEx/config will be created: MarketplaceKG

![](https://i.imgur.com/EnHUG1T.png)

Each file / folder description:
1) Battlepass folder - contains battlepass configs for Free / Premium rewards and main battlepass config (battlepass name, exp step)
2) Discord Webhook folder - allows you to configure webhooks for Marketplace notifications (Quest completed, Marketplace item placed, Gambler won)
3) DO NOT TOUCH - this folder only contains encrypted marketplace related data (players messages, players income, marketplace slots and so on). DO NOT TOUCH this folder since you will lose all your marketplace data if you do so. There are none files you can / need to edit
4) MapPinsIcons - folder where you can place small-weight icons for Teleporter NPC. But there is also MarketplaceCachedTeleporterIcons folder in clientside which i recommend you to use, instead of adding icons on serverside
5) PlayerTerritories - folder with json files and .cfg for Player-made territories (Admin territories are inside TerritoryDatabase.cfg)
6) BankerProfiles.cfg - file for configuring banker NPC's
7) BufferDATABASE.cfg - file that contains all your created buffs for Buffer NPC
8) BufferProfiles.cfg - file for configuring Buffer NPC (you can choose which NPC profile has WHICH buffs from database)
9) GamblerProfiles.cfg - file for configuring Gambler NPC
10) LOGS.log - few logs for some marketplace actions (item deposit / withdraw to banker, marketplace item placed, etc)
11) MarketPlace.cfg - main config that contains small config values for various mechanics
12) QuestDATABASE.cfg - file where you have all your written quests
13) QuestProfiles.cfg - file for configuring Quest NPC (you can choose which NPC profile has WHICH quests from database)
14) ServerInfoProfiles.cfg - file for configuring ServerInfo NPC
15) TeleportHubProfiles.cfg - file for configuring Teleporter NPC
16) TerritoryDatabase.cfg - file for configuring territories
17) TraderProfiles.cfg - file for configuring Trader NPC
18) TransmogrificationProfiles.cfg - file for configuring Transmogrification NPC

</p>
</details>

<details><summary>How to spawn/change NPC:</summary>
<p> 

1) Start the game and join your server
2) Use any admin mod to enable DEBUG MODE
3) After enabling debug mode you can open your hammer and "build" NPC you want

There are two types of NPC's: Visible on map and Not Visible on map.

![](https://i.imgur.com/i4hwElW.png)

![](https://i.imgur.com/7A8rr8u.png)

![](https://i.imgur.com/IMQ7hpV.png)

The difference is only that visible on map NPC will have its Pin on map from any distance.

![](https://i.imgur.com/zlm4GR6.png)


After placing NPC in Debug Mode you can start applying few changes to it. You can open 2 menus: Main NPC UI and Fashion Menu.

![](https://i.imgur.com/K6zbBEQ.png)

Main NPC UI:

![](https://i.imgur.com/eSOXkyZ.png)

1) Top buttons - change NPC type (Marketplace, Trader, Info, Teleporter and so on)
2) Change NPC Profile - NPC profile that will hook data from your *NpcType*Profiles.cfg 
3) Override NPC Name - Change NPC name to whatever you want
4) Override NPC Model - Change NPC model to any in-game (even other mod) creature you want
5) Set Patrol Data - You can make npc walk from one spot to another, or even make a full path for it. Example: 300, 200, 305, 200. It will make your NPC walk from 300 x spot to x 305 spot (5 meters), while Z coord is always 200
6) Snap To Ground And Rotate - snaps NPC to ground and rotates it to where you look at
7) Apply - apply changes

P.S: Override NPC Model accepts ANY Character (monster) prefab (Troll, Greydwarf, Hatchling, and so on). But monsters will have their own animator.
If you want to use Overriden NPC with Player animation from fashion menu you can add @humanoid to your prefab name.
Example:
Troll@humanoid, Greydwarf@humanoid, Neck@humanoid.
That will give these creature Player animator so they will be able to use emote_wave animations and so on (crafting animations also)

Let's try it out:

Adding data:

![](https://i.imgur.com/u5L80rk.png)

Result:

![](https://i.imgur.com/kxIKSm6.png)


Now let's see Fasion Menu:
(Keep in mind that most fashion prefabs / equipment will only work on Player or Player_Female models override. Armors and such won't work on monster override models)

![](https://i.imgur.com/rqGj581.png)

1) Left Hand - left hand prefab
2) Right Hand - right hand prefab
3) Helmet Item - helmet prefab
4) Chest Item - chest prefab
5) Legs Item - legs prefab
6) Cape Item
7) Left Back Item - left back prefab
8) Right Back Item - right back prefab
9) Hair Index - hair index (1 2 3 4 5 and so on) 
10) Hair Color (#hex) - hex color for hair color, example: #ffffff
11) Skin Color (#hex) - hex color for skin color, example: #ffffff
12) Model scale - model size (works on any override model)
13) Interact animation - animation when someone interacts with NPC, example: emote_nonono
14) Greeting animation - animation when someone comes close to NPC, example: emote_thumbsup
15) Bye Animation - animation when someone leaves NPC, example: emote_wave
16) Greeting Text - text when someone comes close to NPC, example: Hello!
17) Bye Text - text when someone leaves NPC, example: Bye!
18) Crafting animation index - animation for Player and Player_Female models that turning on crafting state, there are 0 1 2 3 crafting animation states
19) Beard index - same as hair index, but for beard
20) Beard color (#hex) - hex color for beard color, example: #ffffff

Now let's write some random data:

![](https://i.imgur.com/xK0Kywc.png)

Result:

![](https://i.imgur.com/ULo443R.png)

![](https://i.imgur.com/lFzK72V.png)

Now that we learned how to spawn / edit NPC's lets try to configure some of those from serverside



</p>
</details>

<details><summary>MarketPlace.cfg (misc configs):</summary>
<p> 

![](https://i.imgur.com/48FkIqM.png)

1) ItemMarketLimit - limit of slots a player can post in Marketplace
2) BlockedPlayers - SteamID list of players that can't post items in Marketplace
3) VIPplayersList - SteamID list of players that are VIPs (less taxes)
4) MarketTaxes - taxes for Marketplace items (non-VIP users)
5) VIPplayerTaxes - taxes for Marketplace items (VIP users)
6) CanTeleportWithOre - define if players can teleport with ore in Teleporter NPC
7) MarketSellBlockedPrefabs - prefabs that players cannot sell on marketplace
8) FeedbackWebhookLink - Feedback NPC webhook link
9) ServerCurrency - currency to use in Marketplace. If you have your own prefab - analogue of Coins you can write it here
10) BankerIncomeTime - how often (HOURS) banker will give players income
11) BankerIncomeMultiplier - each #BankerIncomeTime (hours) will add income with multiplier. Example: if player has 100 coins in bank and multiplier is 0.1, then each BankerIncomeTime he will have 100 + 100 * 0.1 (110). Then 110 + 110 * 0.1 = 221. And so on
12) BankerVIPIncomeMultiplier - same as upper, but for VIP players
13) MarketSlotExpirationTime - how many hours should pass, so that player marketplace slot will expire (won't be shown in marketplace list anymore)
14) GamblerEnableWinNotifications - enable global chat win notifications when someone wins something in gambler NPC
15) AllowMultipleQuestsScore - if set to true, then if player has 2 quests with same target, upon adding quest score it will be added to BOTH quests instead of just one
16) MaxAcceptedQuests - maximum number of quests that player can have accepted at once
17) BattlepassVIPlist - SteamID list of players that are VIPs in Battlepass
18) Enable KG Chat - enable / disable KG chat
</p>
</details>

<details><summary>Trader</summary>
<p> 
Trader NPC allows you to set items to be exchanged. Item A x number will be exchanged for Item B x number.

To start with let's make our trader profile in TraderProfiles.cfg:

![](https://i.imgur.com/cYxd3gH.png)

The data format is:

ItemA, ItemA quantity, ItemB, ItemB quantity, ItemB level (If needed)

For example i want to make a trader that will trader 100 coins for 1 swordiron level 2, and trade 1 wood for 10 Rubies:

My profile will look like that:

```
[TestTrader]
Coins, 100, SwordIron, 1, 2
Wood, 1, Ruby, 10
```

Adding that to TraderProfiles.cfg

![](https://i.imgur.com/PSpqNPL.png)

(As in any other NPC you are able to add as many profiles as you want so you can have 100 different NPCs trading different items)

Now let's assign profile to our trader NPC:

![](https://i.imgur.com/BjPrHIS.png)

On interact trader UI will open:

![](https://i.imgur.com/WMFaYl4.png)

Because i have wood and coins in my inventory i can actually exchange that. On clicking big green > (arrow) button in middle i will exchange item A on item B.

Also you can add Pets as trader items. Example: Stone, 100, Wolf, 1, 5. Will exchange 100 stone on one pet wolf level 5

Let's add another profile with pets only!

```
[PetsTrader]
Stone, 100, Wolf, 1, 5
Ruby, 25, Boar, 10, 2
```

![](https://i.imgur.com/10OELul.png)

Assigning PetsTrader profile to our NPC will give us this result:

![](https://i.imgur.com/W4YHMKr.png)

Note that wolf level 5 is 4 stars because stars starts from 0 and level starts from 1. Same for Boar

On top right you have x1, x5, x10 , x100 modifier buttons so player can change exchange rate for faster trading. Note that it applies original rate so Coins, 5, Wood, 1 on exchange rate x100 will be 500 coins to 100 wood

# Since 8.2.7 Marketplace trader got one more data format you can use

New format allows you to use up to 5 Needed Items and 5 Given Items. Also with new format left-side items may also have level (quality) requirement. Format:

```
Item, Quality, Level(IF NEEDED), Item2, Quality2, Level2(IF NEEDED),.... = Item, Quality, Level (IF NEEDED), Item2, Quality2, Level2 (IF NEEDED),....
```

Example:

```
BlackMetal, 1, AxeBlackMetal, 1, 9, Coins, 25 = AxeBlackMetal, 1, 10, Wood, 123
```

^ will give you this result:

![](https://i.imgur.com/tkb8MM5.png)

Keep in mind that you can still use old format in same profile. Example:

```
[test]
SwordIron, 1, 9, Ruby, 666 = SwordIron, 1, 10
BlackMetal, 1, AxeBlackMetal,1,9, Coins, 25 = AxeBlackMetal, 1,10, Wood, 123
Coins, 0 = AxeBlackMetal, 1, 9
Coins, 0, BlackMetal, 5
```

Result will be:

![](https://i.imgur.com/eTT5SbT.png)









</p>
</details>

<details><summary>Quest System</summary>
<p> 

In order to create your own Quests you would need to focus on two file: QuestDATABASE.cfg and QuestProfiles.cfg

![](https://i.imgur.com/4l2Kshv.png)

QuestDATABASE.cfg - a file that contains ALL your created quests. Think about it as a place where all your quests getting their ID there, so later you can add that ID to QuestProfiles NPC

QuestProfiles.cfg - a file that allows you to distribute quests into NPC profiles. You may have 5 NPCs giving SAME quest, as well as 10 NPCs giving different quests

So... Let's create our own first quest! First think you should do is to create a new Quest in QuestDATABASE.cfg.

Here's the quest structure:
```
[QuestID]
QuestType
Name
Description
Quest Target Prefab , Amount, Min Level (min level works only on Kill or Collect quest in order to set minimum level or target you need to kill)
QuestRewardType: prefab, Amount, Level
In-Game Days Cooldown
QuestRequirementType: Prefab, MinLevel (only use with Skill requirement)
```

<span style="color:aqua;"> NOTE: If you want quest to be able to autocomplete (no need to speak again with npc and press "Complete" button after score is 100%), then you can write [QuestID=autocomplete]
</span>

There are 6 types of quests: Kill, Collect, Harvest, Craft, Talk and Build:
```
1) Kill - a quest where the Target is a Character (any creature) prefab. You can set minimum level of target creature to kill
2) Collect - a quest where the Target is an Item prefab. Please note that COLLECT is the only quest type that actually TAKES item from player inventory in order to be finished
3) Harvest - a quest where the Target is a Pickable prefab. Example: Pickable_Carrot, Pickable_Stone and so on. For adding score to this quest you would need to Harvest any of those "farm" prefabs
4) Craft - a quest where the Target is a Item prefab. You can set an item level that should be crafted or leave it 1
5) Talk - a quest where the Target is a full NPC name. After interacting with NPC target quest will autocomplete and rewards will be given
6) Build - a quest where the Target is a Piece prefab. Please note that prefabs that you build for quest target won't return any resources on destroy
```
Quest rewards type:
```
1) Item - a reward where the Target is an ItemDrop prefab. You can set amount and level of given item
2) Skill - a reward where the Target is Skill name. Example Skill: Run, 10. Will give +10 levels of run skill to player who finished a quest
3) Pet - a reward where the Target is a Tameable Creature prefab that will spawn already tamed. You can set amount and level of given pet
4) Skill_EXP - a reward where the Target is Skill name. Example Skill_EXP: Run, 100. Will give +100 exp of run skill to player who finished a quest
5) EpicMMO_EXP  - a reward where the Target is amount of exp. Example EpicMMO_EXP: 100. Will give +100 exp of EpicMMO skill to player who finished a quest
6) Battlepass_EXP - a reward where the Target is amount of exp. Example Battlepass_EXP: 100. Will give +100 exp of Battlepass skill to player who finished a quest
7) MH_EXP - a reward where the Target is amount of exp. Example MH_EXP: 100. Will give +100 exp of MagicHeim experience to player who finished a quest
```

Quest Requirements Types:
```
1) Skill - example: Skill: Run, 10. Will make so that only if you have skill Run at least 10 levels you can accept this quest
2) OtherQuest - example: OtherQuest: MyQuestID123. Will make so that only if you have completed quest with ID MyQuestID123 you can accept this quest
3) GlobalKey - example: GlobalKey: defeated_gdking. Will make so that quest is only acceptable if yagluth was killed on server
4) EpicMMO_Level - example: EpicMMO_Level: 20. Will make so that quest is only acceptable if player has at least 20 EpicMMO levels (other mod API)
5) HasItem - example: HasItem: SwordIron. Will make so that quest is only acceptable if player has at least 1 SwordIron in inventory
6) NotFinished - example: NotFinished: MyQuestID123. Will make so that quest is only acceptable if player has NOT finished quest with ID MyQuestID123
7) IsVIP - example: IsVIP . Will make so that quest is only acceptable if player is VIP
8) MH_Level - example: MH_Level: 20. Will make so that quest is only acceptable if player has at least 20 MagicHeim levels (other mod API)
9) Time - example: Time: 60. Will time limit quest completion to 60 seconds. If player won't complete quest in 60 seconds it will fail
```

Please note that Quest Targets, Quest Rewards and Quest Requirements may be multiple in one quest. You can add them as much as you want with | symbol. Example:


```
Item: SwordIron, 1, 5 | Pet: Wolf, 2, 10 | Skill: Run, 2 | Item: Coins, 100
```
^ quest will give 1 Iron Sword level 5, 2 Wolves level 10, +2 levels of Run skill and 100 coins

Same for requirements:
```
OtherQuest: MyQuest123 | HasItem: PickaxeIron | Skill: Run, 10
```
^ quest will be only acceptable if player has completed quest with ID MyQuest123, has at least 1 PickaxeIron in inventory and has at least 10 levels of Run skill

So... Now that we know all of these things lets create our first quest! I will create a quest where player will need to kill 10 wolves and get 100 Coins + Iron Sword level 3 as a reward with no quest requirements (i will leave it to None). I will set quest cooldown to be 10 in-game days (5 hours real time)

My quest looks like that:
```
[MyTestQuest1]
Kill
This is my first quest!
And this is my first quest description!
Wolf, 10 | Skeleton, 5
Item: SwordIron, 1, 3 | Item: Coins, 100
10
None
```
Now we can add this data to out QuestDATABASE.cfg file:

![](https://i.imgur.com/ejk2NIl.png)

After that we are able to give this quest to any NPC profile we create in QuestProfiles.cfg

I will create NPC profile named TestQuests and add my quest to it:

![](https://i.imgur.com/rhuUwUh.png)

Now let's assign this profile to our NPC:

![](https://i.imgur.com/ba3gJUh.png)

On iteract with NPC you should get your result!

![](https://i.imgur.com/lleU3rp.png)

![](https://i.imgur.com/c4FHGqG.png)

As you can see I didn't specify the Wolf target level (Wolf, 10). So it will by default be level 0 (0 stars). So killing any Wolf will be acceptable for this quest.

Let's take quest and try it out!

![](https://i.imgur.com/nVKKAud.png)

Note that Kill, Collect, Harvest quests will have a markers about target. You can disable marker in local Marketplace config on client

![](https://i.imgur.com/GQKiXZG.png)

On killing wolf i get score 1/10

![](https://i.imgur.com/RIOapFp.png)

Now let's change our quest a little. I will change Wolf, 10 to Wolf, 10, 2. This will make so that only wolves level 2 or higher (2 stars) will be acceptable for this quest 

![](https://i.imgur.com/hgInMiO.png)

As you can see our quest target in-game changed:

![](https://i.imgur.com/ZjP5S3z.png)

![](https://i.imgur.com/r47i7qA.png)

Only wolf with 2 stars and higher now acceptable as quest target. You can see that by quest marker above wolf's head

After finishing quest you can come to same NPC that gave it to you and click "Complete" button to receive rewards.

![](https://i.imgur.com/5qZiacv.png)

![](https://i.imgur.com/tlMY7jW.png)

If quest cooldown is lower than 5000 days then it will be still visible in Quest UI. Use quest cooldown 10000+ for one-time quests

Some Quick Screenshots with few other quest types:

Database:
![](https://i.imgur.com/IzGyHHV.png)

Profiles:
![](https://i.imgur.com/nJTMq4r.png)

Results:

Markers on Build quest targets
![](https://i.imgur.com/AGJ4bGI.png)

Markers on harvest + collect targets
![](https://i.imgur.com/Rr3SMac.png)


Markers on Talk Targets

![](https://i.imgur.com/Ejrhf5u.png)

Good luck with creating your own quests!
</p>
</details>

<details><summary>Quest Events</summary>
<p> 
Quest Events allows you to "attach" events and actions to particular quests created in QuestDatabase.cfg

Possible events:
```
OnAcceptQuest - when player accepts quest
OnCancelQuest - when player cancels quest
OnCompleteQuest - when player completes quest (successfully)
```

Possible actions:
```
GiveItem - example: GiveItem, SwordIron, 1, 5. Will give player 1 Iron Sword level 5
GiveQuest - example: GiveQuest, MyQuestID123. Will give player quest with ID MyQuestID123
RemoveQuest - example: RemoveQuest, MyQuestID123. Will remove quest with ID MyQuestID123
Spawn - example: Spawn, Wolf, 5, 2. Will spawn 5 wolves level 2 (near)
Teleport - example: Teleport, 100, 100, 100. Will teleport player to x100, y100, z100
Damage - example: Damage, 100. Will deal 100 damage to player
Heal - example: Heal, 100. Will heal player for 100 health
PlaySound - example: PlaySound, MySound. Will play sound MySound
NpcText - example: NpcText, MyText. Will show text MyText above closest NPC head
```

Data Format:
```
[questID]
Event: Action, arguments
```

Example:
![](https://i.imgur.com/Qcp98Rx.png)

You are not limited in using one event and action once, you can add as many same events as you want to with different actions, example:

```
[TestQuest]
OnAcceptQuest: GiveItem, SwordIron, 1, 5
OnAcceptQuest: GiveItem, Coins, 100, 1
OnAcceptQuest: Heal, 5000
```

</p>
</details>

<details><summary>Marketplace</summary>
<p> 

The only NPC that doesn't really need anything to be configured. Its working out of box.

![](https://i.imgur.com/Av5NuBe.png)

To sell items click "Sell" tab => choose item you want to sell => choose quantity / price and click "Sell"

![](https://i.imgur.com/Js9QC2r.png)

After clicking "Sell" item should appear in "BUY" tab with all other items. If you're slot owner you can click on it and "Cancel" your sell.

![](https://i.imgur.com/QKmf1Gl.png)

When someone will buy your item you will get currency in "Income: 0 (it will be bigger when you sell)". To redeem your gold just click + button (Income). Currency will be added to your inventory

Marketplace supports all Custom Item data mods, such as EpicLoot, Jewelcrafting, Professions and such


</p>
</details>

<details><summary>Banker</summary>
<p> 
Banker is an NPC that allows you to deposit / withdraw your items in bank. Also if you set Banker Income and Banker Income Time in Marketplace.cfg then each N hours (Banker Income Time) every player will get % Income to their bank resources.

To create a Banker profile go to BankerProfiles.cfg and add a new profile:

![](https://i.imgur.com/n7TZqfI.png)

I want to make a Banker profile that will accept Coins + Rubies. For that i would need to add profile [profileName] and add acceptable items on each new line

![](https://i.imgur.com/Zt1lTbw.png)

Let's assign Banker profile to our Banker NPC in-game:

![](https://i.imgur.com/dQriWbn.png)

On Interact with NPC you should see this:

![](https://i.imgur.com/KlarEFR.png)

Green number = resource amount in bank account. Bottom text = inventory amount

So if i want to deposit (put) 250 coins into bank i would need to write "250" and press "+" :

![](https://i.imgur.com/f22k5fQ.png)

![](https://i.imgur.com/SFOAvma.png)

As you can see now i have 250 coins in bank that will be kept there forever and getting income if server admin wants to be so

You may have multiple banker NPCs with different slots (resources) to keep your items in. For example you can have 1 banker that will keep your coins and another one that will keep your rubies

Think about banker as a "global" big chest with infinite space :D



</p>
</details>

<details><summary>Info </summary>
<p> 

NPC will read info from ServerInfo.cfg and display that on GUI.
Rich text markers can be applied to text you write
ServerInfo npc uses "default" profile by default. But you can add as many info profiles you want (same as Trader NPC profiles). Example below:

![](https://i.imgur.com/JSZ90if.png)

![](https://i.imgur.com/cwOiOsO.png)

![](https://i.imgur.com/MfZXnVH.png)

To add data you need to create profile with [ProfileName], and then uder it you can write info you need. Later just assign this profile to Info NPC and it will show it.
Non-profiled text will be applied to every new Info NPC with "default" profile.
</p>
</details>

<details><summary>Teleporter</summary>
<p> 

NPC acts as teleport-hub but all in one. Its profile/data controlled by BepInEx/MarketplaceKG/TeleportHubProfiles.cfg

![](https://i.imgur.com/pTjanHG.png)

![](https://i.imgur.com/MpIGCz8.png)

To Add new teleport spots you need to add them new line each with structure: Spot Name, X coord, Y coord, Z coord, Icon name

You can add Icons in BepInEx/config/MarketplaceKG/MapPinsIcons folder

![https://i.imgur.com/yZVRMLF.png](https://i.imgur.com/yZVRMLF.png)

I recommend you to use 32x32 icons. 
Also you can write ItemPrefab name instead of icon in order to use its icon as map pin
When you click Interact on Teleporter NPC with profile you will open map and it will show pins to you. After Left Mouse click on icon you will teleport to XYZ coords of spot.

![https://i.imgur.com/Hoy6Gg1.png](https://i.imgur.com/Hoy6Gg1.png)

XYZ COORDS SHOULD BE INTEGERS VALUE ONLY (5.6 <= WRONG, 5 <= good)

If you want to make teleport not instant but be more like "magic" teleport, then you can add <speed=value> parameter to teleport spot name

Example:

Spawn <speed=10>, 0,30,0

That will make teleport to spawn not instant but more magic-alike with speed of 10 meters / second

</p>
</details>

<details><summary>Gambler</summary>
<p>

An NPC that can be placed by admin. The gambler NPC requires items to activate, typically coins. The Gambler offers a list of items and a set amount of which the player can win. So for example a gambler can have ten items in the list, allow two of them to be won, and set a price to roll a chance at winning.

It is possible to combine an admin placed territory at NPC locations if you feel that is right for your server environment. This can provide a safe haven for players while interacting with NPC's. The territory area will also announce itself when entering which can add ambience to the zone. Refer to the Territories reamde for more info on setting up a territory zone.


All NPC placed characters can be altered to include looks, clothing, interactions, patrol options, greetings, animations, salutations, etc. Refer to the "how to spawn/change section" readme for more info on setting up and altering NPC's.   
<br>
<br>
<b>To add a new profile</b> you need to write [ProfileName=ItemsPerRollCount] , and then on a new line add an item list for it (<u>max 10 items</u>, first item is ITEM NEEDED TO ROLL): RollItemPrefab, RollItemCount, Item1, Item1count, Item2, Item2Count, Item3, Item3Count.....     
Item counts can be variable as seen below.
<br>

Example:

[test=2]   
Coins, 10, SwordIron, 1, Tar, 30-50, Wood, 1-100

^ This will add a profile to gambler with 2 items per roll count (he can take 2 items out of 3 in the list)   
Player will need 10 coins per roll, Items are: Sword iron (one), Tar (from 30 to 50 randomly), Wood (from 1 to 100) randomly

<br> 
More Examples:  

[gmeadows=3]<br>
Coins, 250, SpearBronze, 1, Tar, 3-5, Wood, 25, ArrowFire, 20-30, FineWood, 20, Stone, 25, ArrowWood, 20-30, Feathers, 15, MeadTasty, 3-5, TurnipStew, 2-3, ArmorTrollLeatherChest, 1, QueensJam, 3-5, FishRaw, 10, ArrowFlint, 20-30, ArmorTrollLeatherLegs, 1, Coal, 25

[gswamp=3]<br>
Coins, 500, AtgeirBronze, 1, ArrowFire, 30-50, ArrowBronze, 20-30, FineWood, 40, Stone, 50, ArrowIron, 10-20, Feathers, 20, MeadTasty, 3-5, TurnipStew, 3-5, ArmorRootChest, 1, OdinsDelight, 2-3, TeriyakiSalmon, 3-5, BoneArrow, 20-30, ArmorRootLegs, 1, Coal, 35

[gmountain=3]<br>
Coins, 1000, AtgeirIron, 1, Tar, 30-50, ArrowPoison, 50, FineWood, 60, Stone, 75, ArrowObsidian, 50, Feathers, 25, MeadTasty, 3-5, TurnipStew, 5-10, ArmorFenringChest, 1, OdinsDelight, 3-5, HoneyTeriyakiSalmonWrap, 3-5, BoneArrow, 30-50, ArmorFenringLegs, 1, Coal, 50

</p>
</details>

<details><summary>Buffer</summary>
<p> 

Buffer  
is a placeable npc that can be set in the world with pre-configured "buffs" that can be temporarily enabled on the players items. When a player interacts with the npc they can choose from what type of buff they want and on what inventory item it gets placed.



It is possible to combine an admin placed territory at NPC locations if you feel that is right for your server environment. This can provide a safe haven for players while interacting with NPC's. The territory area will also announce itself when entering which can add ambience to the zone. Refer to the Territories reamde for more info on setting up a territory zone.



All NPC placed characters can be altered to include looks, clothing, interactions, patrol options, greetings, animations, salutations, etc. Refer to the "how to spawn/change section" readme for more info on setting up and altering NPC's.   
<br>

Buffs
The Database config is a file with ALL Your buffs. Here you will need to add all buffs so later you can use them in NPC profiles that you setup.

Each buff should have a UNIQUE name (it will be its own Unique ID). Buff should have a layout like this:

[UniqueName]   
Name   
Duration (seconds)   
Buff Icon (Can be taken from monster prefab name or item prefab name)   
Price prefab name, Price count   
Buff modifiers    
Buff visual effect    
Buff group

Example:

[TestBuff]    
First buff i created    
180   
SwordIron   
Coins, 1     
ModifyAttack = 1.5   
vfx_Burning    
Combat

^ Creates buff with duration 180 sec, icon = SwordIron icon,  price = 1 coin, Modifiers are ModifyAttack x1.5,
visual effect is burning and group is Combat.    
<br>

<br>
Modifiers   
All possible modfifiers: ModifyAttack, ModifyHealthRegen, ModifyStaminaRegen, ModifyRaiseSkills, ModifySpeed, ModifyNoise,
ModifyMaxCarryWeight, ModifyStealth, RunStaminaDrain, DamageReduction   

Note: Multiple buffs can be applied at once by putting a "," between them such as;   
ModifySpeed = 1.2, ModifyNoise = 1.4

One "buff" can have nine different modifiers, and the Buff Group combines Buff modifiers into one group. This is done only for balancing, so you can make cheap buffs, normal buffs, and high-priced buffs.   
Note: If buffs are in the same group then player would be able to buy only ONE BUFF OUT OF GROUP at a time. See below there are two examples in the "exploration" group, so only one could be purchases/applied at a time.   
<br>

Profiles  
Buffs need to be applied to an NPC profile in order to work. To add a new profile you need to write [ProfileName] , and on a new line add buffer list for it (buff unique IDs from BufferDATABASE.cfg)

[MeadowsBuffs]    
TestBuff1, TestBuff2

^adds MeadowsBuffs profile to the buffer NPC with 2 buffs taken from buff database config file.

<br>
More Examples:

[Stealth]
Stealth Increase   
2400   
HelmetTrollLeather   
Coins, 300   
ModifyStealth = 5    
None   
Exploration


[Speed]   
Swiftness   
1600   
TankardOdin   
Coins, 150   
ModifySpeed = 1.5   
None   
Speed

[Run]    
Running Increase   
1800   
GlowingMushroom   
Coins, 500   
ModifyStaminaRegen = 2, ModifySpeed = 2   
vfx_GodExplosion   
Exploration

[Tenacity]    
Toughness increase     
900     
HelmetDrake     
Coins, 500     
DamageReduction = 0.30    
vfx_creature_love   
Toughness

[Assault]    
Fighting increase    
600    
FlametalOre    
Coins, 500     
ModifyAttack = 2    
vfx_fir_oldlog    
Rage


Note: you can view all the in-game VFX by using easy spawner and searching for vfx.  
some common useful ones are vfx_HealthUpgrade, vfx_lootspawn, vfx_odin_despawn, vfx_offering, vfx_perfectblock, vfx_odin_despawn

</p>
</details>

<details><summary>Territory System</summary>
<p> 

The `TerritoryDatabase.cfg` file is used to define territories or zones within your game world. Each zone can have specific attributes such as shape, position, size, color, flags, and owners. This guide will help you understand the format and options available in the configuration file.

## Format

The configuration file follows the following format:

```plaintext
[ZoneName]
Shape type (Circle, Square, Custom)
X pos, Z pos, Radius (for circle/square) or X pos, Z pos, X length, Z length (for custom zone)
Red Color, Green Color, Blue Color, Show Territory on water (True/False)
Zone Flags (separated by comma if multiple)
Owners SteamID (separated by comma if multiple)
```

## Zone Attributes

### Zone Name

Each zone entry begins with a unique `ZoneName`. This identifier is used to differentiate between different zones in the configuration file.

### Shape Type

The shape of the zone can be specified as one of the following:

- Circle: The zone is defined as a circle with a center point and a radius.
- Square: The zone is defined as a square with a center point and side length.
- Custom: The zone is defined with custom dimensions using the X and Z position coordinates, along with the X and Z lengths.

### Position and Size

Depending on the shape type, you need to specify the position and size of the zone:

- For a circle or square, provide the X and Z position coordinates and the radius (for a circle) or side length (for a square).
- For a custom zone, provide the X and Z position coordinates, as well as the X and Z lengths.

### Color and Show Territory on Water

Specify the color of the zone using RGB values (Red, Green, Blue). Additionally, indicate whether the territory should be visible on water by specifying `True` or `False` after the RGB color values.

### Zone Flags

You can assign specific flags to a zone to define its behavior and characteristics. Multiple flags can be assigned to a zone, separated by commas. Here are the available flags:

- `PushAway`: Players are pushed away from the zone boundaries.
- `NoBuild`: Building structures is not allowed within the zone.
- `NoPickaxe`: Players cannot use pickaxes within the zone.
- `NoInteract`: Interactions with objects or NPCs within the zone are disabled.
- `NoAttack`: Players cannot initiate attacks or engage in combat within the zone.
- `PvpOnly`: Forces PvP mode within the zone.
- `PveOnly`:  Forces PvE mode within the zone.
- `PeriodicHeal`: Players are periodically healed while inside the zone (only zone owners).
- `PeriodicDamage`: Players receive periodic damage while inside the zone.
- `IncreasedPlayerDamage`: Player attacks deal increased damage within the zone.
- `IncreasedMonsterDamage`: Monsters deal increased damage to players within the zone.
- `NoMonsters`: Monsters do not spawn or exist within the zone.
- `CustomEnvironment`: The zone has a custom environment specified by the environment name.
- `MoveSpeedMultiplier`: Players' movement speed is multiplied by a certain factor within the zone.
- `NoDeathPenalty`: Players do not suffer penalties upon death within the zone.
- `NoPortals`: Teleportation portals cannot be used within the zone.
- `PeriodicHealALL`: All players are periodically healed within the zone.
- `ForceGroundHeight`: The ground height is forcefully set within the zone.
- `ForceBiome`: The biome within the zone is forcefully set.
- `AddGroundHeight`: Additional ground height is added within the zone.
- `NoBuildDamage`: Structures within the zone do not take damage.
- `MonstersAddStars`: Monsters within the zone have additional stars, indicating higher difficulty.
- `InfiniteFuel`: Fuel consumption is disabled within the zone.
- `NoInteractItems`: Interactions with items within the zone are disabled.
- `NoInteractCraftingStation`: Interactions with crafting stations within the zone are disabled.
- `NoInteractItemStands`: Interactions with item stands within the zone are disabled.
- `NoInteractChests`: Interactions with chests within the zone are disabled.
- `NoInteractDoors`: Interactions with doors within the zone are disabled.
- `NoStructureSupport`: Structures within the zone do not get damaged if they are not supported.
- `NoInteractPortals`: Interactions with portals within the zone are disabled.
- `CustomPaint`: The zone has custom paint applied to it.
- `LimitZoneHeight`: The minimum height of the zone is limited.
- `NoItemLoss`: Players do not lose items upon death within the zone.
- `SnowMask`: A snow mask effect is applied within the zone.
- `NoMist`: Mist weather effects are disabled within the zone.
- `InfiniteEitr`: Eitr consumption is disabled within the zone.
- `InfiniteStamina`: Stamina consumption is disabled within the zone.
- `NoCreatureDrops`: Creatures within the zone do not drop items upon defeat.

**Note:** For the `CustomEnvironment`, `PeriodicDamage`, `PeriodicHealALL`, `PeriodicHeal`, `IncreasedMonsterDamage`, `IncreasedPlayerDamage`, `MoveSpeedMultiplier`, `ForceGroundHeight`, `AddGroundHeight`, `LimitZoneHeight`, `ForceBiome`, `MonstersAddStars`, and `CustomPaint` flags, the flag should be followed by = and the value of the flag. For example, `CustomEnvironment = Clear` or `PeriodicDamage = 10`.

`ForceBiome` accepts values:
```
Meadows = 1,
Swamp = 2,
Mountain = 4,
BlackForest = 8,
Plains = 16,
AshLands = 32,
DeepNorth = 64,
Ocean = 256,
Mistlands = 512
```

(`ForceBiome = 2` will force the biome to be swamp)

`CustomPaint` accepts values:
```
Paved = 0,
Grass = 1,
Cultivated = 2,
Dirt = 3
```

(`CustomPaint = 2` will paint the zone with the Cultivated texture)

### Owners

Specify the SteamIDs of the owners of the zone. If there are multiple owners, separate their SteamIDs with commas.

## Example

Here's an example entry in the `TerritoryDatabase.cfg` file:

```plaintext
[ExampleZone]
Square
150, 100, 800
0, 128, 255, false
NoBuild, NoInteract, PeriodicHealALL = 50
None


[ZoneWithHigherPriority@2]
Square
150, 100, 400
255, 0, 0, false
CustomEnvironment = Clear, NoAttack, NoPickaxe, PeriodicDamage = 10
None

```

All zones by default having priority 1. If you want to change priority of zone, you need to add `@` and priority number after zone name. For example, `ZoneWithHigherPriority@2` will have priority 2.
That will allow you to create zones inside zones. For example, you can create a zone with priority 1 and then create a zone with priority 2 inside it.

</p>
</details>

<details><summary>Battlepass</summary>
<p> 

Battlepass   
is a reward system for players on a server. It allows the admin to set items as rewards, and players can claim their reward when they have accumulated enough experience points. The admin will need to create quests or find some other way to award battlepass experience to the players.

The battlepass folder contains a main config, a config for free rewards, and another for premium rewards. To add players to the premium list you must enter their Steam Ids in the main marketplace.cfg file in the section "BattlepassVIPlist". Only those players will have access to premium rewards.

The main config has two options. First is the battlepass name which is a unique name. Be careful choosing the name because after changing the battlepass name it will drop all experience / rewards for the previous battlepass name, meaning all players accumulated experience will be lost if you change the name mid-season.

The second option is the battlepass experience step. This can be whatever integer value you wish. This value should correlate with the amount of experience being awarded through quests. If the experience step is set to 50 then you may wish to give smaller experience rewards from quests like 10 or 15 per quest completed. However, if you set the steps to 200 then you will need to increase the amount given for quests to accomodate.

Finally, if you want to skip a level then simply do not include the "reward level". For example, if you want to have a reward at level 2 and then the next at level 5 all you have to do is not include a reward level for the levels in between. For example, go straight from level 3 to level 7.


Format
The format for creating the rewards is the same for either free or premium. The format for entering rewards is [unique name = reward level] , followed by the reward on the next line. The format of the reward is item name,amount,item level

Example:<br>
[food is good = 1]    
Carrot,5,0


More Examples:

[reward = 1]   
ArmorTrollLeatherLegs,1,0

[reward = 2]   
ArmorTrollLeatherChest,1,0

[reward = 3]   
HelmetTrollLeather,1,0

[reward = 4]   
CapeTrollHide,1,0

[reward = 5]   
BowFineWood,1,0

[reward = 6]    
SpearChitin,1,0

[reward = 7]    
ArmorIronLegs,1,0

[reward = 8]   
ArmorIronChest,1,0

[reward = 9]   
HelmetIron,1,0

</p>
</details>

<details><summary>NPC Dialogues</summary>

### File Format

The `NpcDialogues.cfg` file is written in a simple and human-readable format. Each dialogue entry consists of a unique profile name followed by the NPC dialogue text and player options. The player options can have various attributes such as text, transition, command, icon, condition, and always visible.

Here's the structure of a dialogue entry:

```
[UniqueProfileName]
Dialogue Text
Player Option 1
Player Option 2
...
```

The player options can have the following attributes:

- `Text`: Represents the text of the player option.

- `Transition`: Specifies a transition to another dialogue.

- `Command`: Specifies the command associated with the player option.

- `Icon`: Represents an icon associated with the player option ( Can be any item prefab or icons from client folder ).

- `Condition`: Defines the condition under which the player option is available.

- `AlwaysVisible`: Indicates that the player option is always visible, regardless of conditions.

- `Color`: Represents the color of the player option text.

Dialogue may have multiple attributes split by | (pipe) character. For example:

```
[UniqueProfileName]
NPC text
Text: Option1 | Transition: UniqueProfileName2 | Command: Damage, 20 | Command: GiveItem, Coins, 50 | Icon: Hammer | Condition: NotFinished, QuestId | AlwaysVisible: false | Color: 255, 128, 0
Text: Option2 | Transition: UniqueProfileName3 | Command: Heal, 20 | Icon: SwordIron | Condition: NotFinished, QuestId | AlwaysVisible: true
```

---

### Conditions

The following conditions can be used in the `NpcDialogues.cfg` file:


- `HasItem`
    - **Usage**: `HasItem, ItemPrefab, Amount`
    - **Description**: Checks if the player has the specified amount of a particular item.

- `NotHasItem`
    - **Usage**: `NotHasItem, ItemPrefab, Amount`
    - **Description**: Checks if the player does not have the specified amount of a particular item.

- `HasBuff`
    - **Usage**: `HasBuff, BuffName`
    - **Description**: Checks if the player has the specified buff.

- `NotHasBuff`
    - **Usage**: `NotHasBuff, BuffName`
    - **Description**: Checks if the player does not have the specified buff.

- `SkillMore`
    - **Usage**: `SkillMore, SkillName, MinLevel`
    - **Description**: Checks if the player's skill level in the specified skill is greater than or equal to the minimum level.

- `SkillLess`
    - **Usage**: `SkillLess, SkillName, MaxLevel`
    - **Description**: Checks if the player's skill level in the specified skill is less to the maximum level.

- `GlobalKey`
    - **Usage**: `GlobalKey, KeyName`
    - **Description**: Checks if the specified global key is active.

- `NotGlobalKey`
    - **Usage**: `NotGlobalKey, KeyName`
    - **Description**: Checks if the specified global key is not active.

- `IsVIP`
    - **Usage**: `IsVIP`
    - **Description**: Checks if the player is a VIP.

- `NotIsVIP`
    - **Usage**: `NotIsVIP`
    - **Description**: Checks if the player is not a VIP.

- `HasQuest`
    - **Usage**: `HasQuest, QuestID`
    - **Description**: Checks if the player has the specified quest.

- `NotHasQuest`
    - **Usage**: `NotHasQuest, QuestID`
    - **Description**: Checks if the player does not have the specified quest.

- `QuestProgressDone`
    - **Usage**: `QuestProgressDone, QuestID`
    - **Description**: Checks if quest progress is done (max score).

- `QuestProgressNotDone`
    - **Usage**: `QuestProgressNotDone, QuestID`
    - **Description**:  Checks if quest progress is not done (max score).

- `QuestFinished`
    - **Usage**: `QuestFinished, QuestID`
    - **Description**: Checks if the specified quest is already finished.

- `QuestNotFinished`
    - **Usage**: `QuestNotFinished, QuestID`
    - **Description**: Checks if the specified quest is not finished yet.

Please note that you can use these conditions within the player options of your dialogue entries to control the availability and visibility of options based on specific game conditions or player states.

Feel free to refer to this documentation for further clarification or provide more examples if needed.

Please note that you should replace the placeholder values (`UniqueProfileName`, `Dialogue Text`, `Player options`, `Text`, `Transition`, `Command`, `Icon`, `Condition`, `AlwaysVisible`, `QuestId`, `ItemPrefab`, `amount`, `BuffName`, `SkillName`, `MinLevel`, `somekey`) with actual values relevant to your game and dialogues.

---

### Commands

The following commands can be used in the `NpcDialogues.cfg` file:

- `OpenUI`: Opens a specific NPC type profile UI.
    - **Usage**: `OpenUI, NPC Type, Profile Name`
    - **Description**: Opens the UI associated with a particular NPC type profile.
    - **Possible NPC Types**: Marketplace, Trader, Info, Teleporter, Feedback, Banker, Gambler, Quests, Buffer, Transmog

- `FinishQuest`: Finishes a quest.
    - **Usage**: `FinishQuest, QuestID`
    - **Description**: Finishes the specified quest.

- `PlaySound`: Plays a sound.
    - **Usage**: `PlaySound, SoundName`
    - **Description**: Plays the specified sound.

- `GiveQuest`: Gives a quest to the player.
    - **Usage**: `GiveQuest, QuestID`
    - **Description**: Gives the player the specified quest.

- `GiveItem`: Gives an item to the player.
    - **Usage**: `GiveItem, ItemPrefab, Amount, Level`
    - **Description**: Gives the player a specified number of items of a certain level.

- `RemoveItem`: Removes items from the player's inventory.
    - **Usage**: `RemoveItem, ItemPrefab, Amount`
    - **Description**: Removes a specified number of items from the player's inventory.

- `Spawn`: Spawns creatures nearby.
    - **Usage**: `Spawn, CreaturePrefab, Amount, Level`
    - **Description**: Spawns a specified number of creatures of a certain level near the player.

- `Teleport`: Teleports the player to a specific location.
    - **Usage**: `Teleport, X, Y, Z`
    - **Description**: Teleports the player to the specified coordinates.

- `RemoveQuest`: Removes a quest from the player.
    - **Usage**: `RemoveQuest, QuestID`
    - **Description**: Removes the specified quest from the player's quest log.

- `Damage`: Inflicts damage on the player.
    - **Usage**: `Damage, Value`
    - **Description**: Damages the player by the specified value.

- `Heal`: Restores health to the player.
    - **Usage**: `Heal, Value`
    - **Description**: Restores the player's health by the specified value.

- `GiveBuff`: Gives a buff to the player.
    - **Usage**: `GiveBuff, BuffID`
    - **Description**: Gives the player the specified buff.

- `AddPin`: Adds a pin to the player's map.
    - **Usage**: `AddPin, Name, X, Y, Z`
    - **Description**: Adds the specified pin to the player's map.

- `PingMap`: Pings a location on the player's map.
    - **Usage**: `PingMap, Name, X, Y, Z`
    - **Description**: Pings the specified location on the player's map.

Please note that you can use these commands within the player options of your dialogue entries to trigger specific actions or behaviors based on the player's choices.

You can use **multiple** commands and conditions in a single player option by separating them with | (pipe) character.

---

# Dialogue exampes:

```
[default]
Welcome to the village!
Text: Hello there! What brings you to our peaceful village?
Text: How can I assist you today?
Text: Tell me more about this village | Command: OpenUI, Info, VillageInfoProfile | Icon: village_icon
Text: I'm looking for work | Transition: JobOptions | Icon: job_icon

[JobOptions]
Available job options:
Text: We have various job opportunities available. What type of work are you interested in?
Text: Farming | Command: OpenUI, Quests, Job | Icon: Hoe | Condition: HasItem, Hoe, 1 
Text: Fishing | Command: OpenUI, Quests, FishingJob | Icon: Fish1 | Condition: SkillMore, Fishing, 10
```

Then just attach initial (in our case default) dialogue to NPC UI

</details>

<details><summary><span style="color:crimson;font-weight:200;font-size:18px">Transmogrification</span></summary>
<p> 

Transmogrification is a system that allows your players to give their equipment any other item appearance in game.

As server admin you can configure which npc / profile will give which skins to use.

In order to start configuring the system go to marketplace folder and open TransmogrificationProfiles.cfg.

Data Format:
```
[ProfileName]
SkinPrefab, Price Prefab, Price Amount, Skip TypeCheck true/false, Special VFX ID (optional)
```
To add more items to profile add them on new line.
Example:

```
[TestProfile]
SwordIron, Coins, 10, false
SwordIron, Coins, 20, false, 2
SwordIron, Coins, 50, false, 20
SwordIronFire, Ruby, 10, false
SwordIronFire, Ruby, 20, false, 2
SwordIronFire, Ruby, 50, false, 20
```

^ This profile will give NPC 6 items to use as skins, usual IronSword, IronSword with VFX ID 2, IronSword with VFX ID 20, FireSword, FireSword with VFX ID 2, FireSword with VFX ID 20.

Note that if VFX id is 21 then players will be able to chooce vfx manually on item.

1) Assigning profile to NPC:
![](https://i.imgur.com/AZVMocc.png)
2) Open UI by interacting with NPC to see result:
![](https://i.imgur.com/tbbWD7j.png)
In Left side you can choose item from your inventory you want to transmogrify and then choose an item in right window

IF YOU SET SKIP TYPECHECK TO TRUE, YOU WILL BE ABLE TO USE ANY ITEM AS SKIN, EVEN IF IT IS NOT EQUIPMENT. THIS WILL CAUSE SOME ISSUES WITH SOME ITEMS, SO USE IT ONLY IF YOU KNOW WHAT YOU ARE DOING.

Also skip typecheck will allow you to set 2-handed weapon as skin for 1-handed weapon and vice versa. Or it will allow you to use Trophy as skin:

![](https://i.imgur.com/T8QmpJm.png)

![](https://i.imgur.com/Sd4Xsdo.png)

As you noticed there are 20 VFX's marketplace can give you. To use them after typecheck skip true/false write VFX ID you want to use.

Effect names by default:
```
mpasn_transmog_eff1: Azure Ashes
mpasn_transmog_eff2: Burning Low
mpasn_transmog_eff3: Cyan Wrap
mpasn_transmog_eff4: Ice Age
mpasn_transmog_eff5: Angel Touch
mpasn_transmog_eff6: Purple Flame
mpasn_transmog_eff7: Burning High
mpasn_transmog_eff8: Turbulence
mpasn_transmog_eff9: Radiation
mpasn_transmog_eff10: Loki's Anger
mpasn_transmog_eff11: Phantom
mpasn_transmog_eff12: Golden Age
mpasn_transmog_eff13: Ice Menace
mpasn_transmog_eff14: Cyan Breathe
mpasn_transmog_eff15: Lightning Strike
mpasn_transmog_eff16: Tranquility
mpasn_transmog_eff17: Magic Arise
mpasn_transmog_eff18: Water
mpasn_transmog_eff19: Energy Flow
mpasn_transmog_eff20: Lightning Menace
```

Lets try to affect out Cheat Sword with transmog:
1) Choose item
![](https://i.imgur.com/SDJsDOh.png)
2) Choose skin
![](https://i.imgur.com/DSkdimb.png)

(press little square icon in right bottom)

4) Done:
![](https://i.imgur.com/STsZbGs.png)
5) Out item looks like that now:
![](https://i.imgur.com/T4Ss9IB.png)
6) When you equip item you will see that its appearance changed, as well now it has VFX. All weapon stats are same, as well as animation of attacks and so on:
![](https://i.imgur.com/apOXM30.png)
7) If you want to remove transmog from item - choose an item in UI and press "Clear" button

</p>
</details>

## ALL OPTIONS / PROFILES / NPCs DATA ARE AUTO-RELOADED IN SERVER RUNTIME WITHOUT RESTART

## ![https://i.imgur.com/5ZHfxlo.png](https://i.imgur.com/5ZHfxlo.png)

## To install mod place MarketPlaceRevamped.dll into Client Plugins folder AND Server Plugins Folder


![https://i.imgur.com/gTTJ9HJ.png](https://i.imgur.com/gTTJ9HJ.png)

For Questions or Comments, find KG#7777 ![https://i.imgur.com/CPYNjXV.png](https://i.imgur.com/CPYNjXV.png)﻿ in the Odin Plus Team Discord:
[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/5gXNxNkUBt)