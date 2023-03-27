using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace WitchesOfCalradia
{
    public class WitchesOfCalradiaBehavior : CampaignBehaviorBase
    {
        ItemObject _healingHerb;
        public override void RegisterEvents()
        {
            CampaignEvents.OnWorkshopChangedEvent.AddNonSerializedListener(this, OnWorkshopChangedEvent);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, LocationCharactersAreReadyToSpawn);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);

        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            _healingHerb = MBObjectManager.Instance.GetObject<ItemObject>("woc_healing_herb");
            AddDialogs(starter);
        }

        private void AddDialogs(CampaignGameStarter starter)
        {

      
            starter.AddPlayerLine("tavernkeeper_talk_ask_witches", "tavernkeeper_talk", "tavernkeeper_witches", "I am looking for the town witch, do you know where she is?", null, null);
            starter.AddDialogLine("tavernkeeper_talk_witches", "tavernkeeper_witches", "close_window", "Witches? If I knew I would make sure she burns next to you. Get our of here you barbarian!",
                () => (Settlement.CurrentSettlement.Culture.StringId == "vlandia" || Settlement.CurrentSettlement.Culture.StringId == "empire") ? true : false, null);
            starter.AddDialogLine("tavernkeeper_talk_witches", "tavernkeeper_witches", "tavernkeeper_talk_witch", "Yes, I've seen her roaming around the town.", null, null);
            starter.AddPlayerLine("tavernkeeper_talk_witches", "tavernkeeper_talk_witch", "close_window", "Thank you, I will look for her.", null, null);


            starter.AddDialogLine("herb_witch", "start", "herb_witch", "Hello traveller, what do you need?", 
                () => CharacterObject.OneToOneConversationCharacter == Settlement.CurrentSettlement.Culture.FemaleBeggar, null);
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
            Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;

            if (!(CampaignMission.Current.Location == locationWithId && CampaignTime.Now.IsDayTime)) return; 
            if(Settlement.CurrentSettlement.Culture.StringId == "vlandia" || Settlement.CurrentSettlement.Culture.StringId == "empire") return;

            CharacterObject femaleBeggar = Settlement.CurrentSettlement.Culture.FemaleBeggar;
            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(femaleBeggar.Race, "_settlement");

            foreach (Workshop workshop in settlement.Town.Workshops)
            {
                if (workshop.IsRunning);
                {
                    int num;
                    unusedUsablePointCount.TryGetValue(workshop.Tag, out num);
                    if (num > 0)
                    {
                        LocationCharacter locationCharacter = new LocationCharacter(
                            new AgentData(new SimpleAgentOrigin(femaleBeggar))
                            .Monster(monsterWithSuffix)
                            .Age(MBRandom.RandomInt(60, 80)),
                            new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                            workshop.Tag, true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);

                        locationWithId.AddCharacter(locationCharacter);
                    }
                }
            }
        }

        private void DailyTick(Town town)
        {
            //throw new NotImplementedException();
        }

        private void OnWorkshopChangedEvent(Workshop workshop, Hero oldOwningHero, WorkshopType workshopType)
        {
            throw new NotImplementedException();
        }

        public override void SyncData(IDataStore dataStore)
        {
            //throw new System.NotImplementedException();
        }
    }
}