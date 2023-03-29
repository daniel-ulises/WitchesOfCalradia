using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace WitchesOfCalradia
{
    public class WitchesOfCalradiaBehavior : CampaignBehaviorBase
    {
        ItemObject _healingHerb;
        CharacterObject _herbWitch;
        MBReadOnlyList<Village> _villageList = new MBReadOnlyList<Village>();
        Village _witchLocation;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, LocationCharactersAreReadyToSpawn);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            _healingHerb = MBObjectManager.Instance.GetObject<ItemObject>("woc_healing_herb");
            _herbWitch = MBObjectManager.Instance.GetObject<CharacterObject>("woc_witch_character");
            for (int i = 1; i < Village.All.Count; i++)
            {
                if (Village.All[i].Bound.Culture.StringId is "vlandia" or "empire" || Village.All[i].IsCastle) continue;
                _villageList.Add(Village.All[i]);

            }

            _witchLocation = WitchLocation();
            AddDialogs(starter);
        }

        private void AddDialogs(CampaignGameStarter starter)
        {

            starter.AddPlayerLine("tavernkeeper_talk_ask_witches", "tavernkeeper_talk", "tavernkeeper_witches", "I am looking for the witch, have you heard about her whereabouts?", null, null);
            starter.AddDialogLine("tavernkeeper_talk_witches", "tavernkeeper_witches", "close_window", "Witches? If I knew I would make sure she burns next to you. Get our of here you barbarian!",
                () => (Settlement.CurrentSettlement.Culture.StringId == "vlandia" || Settlement.CurrentSettlement.Culture.StringId == "empire"), null);
            starter.AddDialogLine("tavernkeeper_talk_witches", "tavernkeeper_witches", "tavernkeeper_talk_witch", "I head a caravan of traders mentioned something about a witch in a village nearby. You may find her in a neighbouring village.", 
                () => (Settlement.CurrentSettlement.Name == _witchLocation.TradeBound.Name), null);
            starter.AddDialogLine("tavernkeeper_talk_witches", "tavernkeeper_witches", "tavernkeeper_talk_witch_negative", "No, I have not hear anything recently.", null, null);
            starter.AddPlayerLine("tavernkeeper_talk_witches", "tavernkeeper_talk_witch", "close_window", "Thank you for the information.", null, null);


            starter.AddDialogLine("herb_witch", "start", "herb_witch", "Hello traveller, what do you need?", 
                () => CharacterObject.OneToOneConversationCharacter == _herbWitch, null);
            starter.AddPlayerLine("herb_witch_buy", "herb_witch", "herb_witch_bought", "I am looking for herbs", null, 
                () => { 
                    Hero.MainHero.ChangeHeroGold(-800);
                    MobileParty.MainParty.ItemRoster.AddToCounts(_healingHerb, 5);
                }, 100,
                (out TextObject explanation) => {
                    if (Hero.MainHero.Gold < 800)
                        {
                            explanation = new TextObject("You do not have enough denars, traveller.");
                            return false;
                        }
                    explanation = TextObject.Empty;
                    return true;
                });
            starter.AddDialogLine("herb_witch_thanks", "herb_witch_bought", "end", "Do not take many of them, they will weaken you.", null, null);
            starter.AddPlayerLine("herb_witch_buy_refuse", "herb_witch", "herb_witch_decline", "Not right now", null, null);
            starter.AddDialogLine("herb_witch_buy_bye", "herb_witch_decline", "end", "You will come back...", null, null);
        }

        private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            Location locationWithId = settlement.LocationComplex.GetLocationWithId("village_center");
            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(_herbWitch.Race, "_settlement");
            
            if (CampaignMission.Current.Location == locationWithId && settlement.Name == _witchLocation.Name)
            {
                LocationCharacter witchCharacter = new LocationCharacter(
                    new AgentData(new SimpleAgentOrigin(_herbWitch))
                    .Monster(monsterWithSuffix)
                    .Age(MBRandom.RandomInt(70, 100)),
                    new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                   "npc-common", true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);

                locationWithId.AddCharacter(witchCharacter);
            }

        }

        private Village WitchLocation()
        {
            return _witchLocation = _villageList[MBRandom.RandomInt(0, _villageList.Count)];

        }


        private void DailyTick()
        {
            WitchLocation();
        }

        public override void SyncData(IDataStore dataStore)
        {
            //
        }
    }
}