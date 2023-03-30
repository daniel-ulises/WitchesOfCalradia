using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace WitchesOfCalradia
{
    public class HealingHerbMissionView : MissionView
    {

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Q))
            {
                EatHealingHerb();
            }
        }

        private void EatHealingHerb()
        {
            Agent agent = Agent.Main;
            ItemRoster items = MobileParty.MainParty.ItemRoster;
            ItemObject healingHerb = MBObjectManager.Instance.GetObject<ItemObject>("woc_healing_herb");

            if (!(Mission.Mode is MissionMode.Battle or MissionMode.Stealth or MissionMode.Duel)) return;

            if (items.GetItemNumber(healingHerb) == 0)
            {
                InformationManager.DisplayMessage(new InformationMessage("You ran out of herbs, go visit a witch to restock"));
            }
            else if (agent.Health == agent.BaseHealthLimit)
            {
                InformationManager.DisplayMessage(new InformationMessage("The herb wouldn't have any effect... You decide not to eat it."));
            }
            else if (agent.BaseHealthLimit <= 10)
            {
                InformationManager.DisplayMessage(new InformationMessage("You're too weak, the plant would kill you"));
            }
            else if ((agent.BaseHealthLimit - 50) <= 10)
            {
                items.AddToCounts(healingHerb, -1);
                agent.BaseHealthLimit = 10;
                InformationManager.DisplayMessage(new InformationMessage(
                    ($"You gather your last bit of strenght to eat a healing herb.\nYour maximum health is now {agent.BaseHealthLimit}")
                    ));
            }
            else
            {
                items.AddToCounts(healingHerb, -1);
                agent.BaseHealthLimit -= 45;
                agent.Health = agent.BaseHealthLimit;
                InformationManager.DisplayMessage(new InformationMessage(
                    ($"You restored your health, but you suffer side effects!\nYour maximum health is now {agent.BaseHealthLimit}")
                    ));
            }

        }
    }
}