﻿using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;
using RotationSolver.Localization;
using RotationSolver.UI;
using RotationSolver.Updaters;

namespace RotationSolver.Commands
{
    public static partial class RSCommands
    {
        static DateTime _lastClickTime = DateTime.MinValue;
        static bool _lastState;

        internal static unsafe bool CanDoAnAction(bool isGCD)
        {
            if (!_lastState || !DataCenter.State)
            {
                _lastState = DataCenter.State;
                return false;
            }
            _lastState = DataCenter.State;

            if (!Player.Available) return false;

            //Do not click the button in random time.
            if (DateTime.Now - _lastClickTime < TimeSpan.FromMilliseconds(new Random().Next(
                (int)(Service.Config.GetValue(PluginConfigFloat.ClickingDelayMin) * 1000), (int)(Service.Config.GetValue(PluginConfigFloat.ClickingDelayMax) * 1000)))) return false;
            _lastClickTime = DateTime.Now;

            if (!isGCD && ActionUpdater.NextAction is IBaseAction act1 && act1.IsRealGCD) return false;

            return true;
        }
        internal static DateTime _lastUsedTime = DateTime.MinValue;
        internal static uint _lastActionID;
        public static void DoAction()
        {
            var wrong = new Random().NextDouble() < Service.Config.GetValue(PluginConfigFloat.MistakeRatio) && ActionUpdater.WrongAction != null;
            var nextAction = wrong ? ActionUpdater.WrongAction : ActionUpdater.NextAction;
            if (nextAction == null) return;

            if (wrong)
            {
                Svc.Toasts.ShowError(string.Format(LocalizationManager.RightLang.ClickingMistakeMessage, nextAction));
                ControlWindow.Wrong = nextAction;
                ControlWindow.DidTime = DateTime.Now;
            }

#if DEBUG
            //if (nextAction is BaseAction acti)
            //    Svc.Log.Debug($"Will Do {acti}");
#endif

            if (Service.Config.GetValue(PluginConfigBool.KeyBoardNoise))
            {
                PreviewUpdater.PulseActionBar(nextAction.AdjustedID);
            }

            if (nextAction.Use())
            {
                OtherConfiguration.RotationSolverRecord.ClickingCount++;

                _lastActionID = nextAction.AdjustedID;
                _lastUsedTime = DateTime.Now;

                if (nextAction is BaseAction act)
                {
                    if (Service.Config.GetValue(PluginConfigBool.KeyBoardNoise))
                        Task.Run(() => PulseSimulation(nextAction.AdjustedID));

                    if (act.ShouldEndSpecial) ResetSpecial();
#if DEBUG
                    //Svc.Log.Debug($"{act}, {act.Target.Name}");
#endif
                    //Change Target
                    if (act.Target != null && act.Target != Player.Object && !DataCenter.IsManual
                        && (Service.Config.GetValue(PluginConfigBool.SwitchTargetFriendly) || ((Svc.Targets.Target?.IsNPCEnemy() ?? true)
                        || Svc.Targets.Target?.GetObjectKind() == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Treasure)
                        && act.Target.IsNPCEnemy()))
                    {
                        Svc.Targets.Target = act.Target;
                    }
                }
            }
        }

        static bool started = false;
        static async void PulseSimulation(uint id)
        {
            if (started) return;
            started = true;
            try
            {
                for (int i = 0; i < new Random().Next(Service.Config.GetValue(PluginConfigInt.KeyBoardNoiseMin),
                    Service.Config.GetValue(PluginConfigInt.KeyBoardNoiseMax)); i++)
                {
                    PreviewUpdater.PulseActionBar(id);
                    var time = Service.Config.GetValue(PluginConfigFloat.ClickingDelayMin) +
                        new Random().NextDouble() * (Service.Config.GetValue(PluginConfigFloat.ClickingDelayMax) - Service.Config.GetValue(PluginConfigFloat.ClickingDelayMin));
                    await Task.Delay((int)(time * 1000));
                }
            }
            catch (Exception ex)
            {
                Svc.Log.Warning(ex, "Pulse Failed!");
            }
            finally { started = false; }
            started = false;
        }

        internal static void ResetSpecial()
        {
            DoSpecialCommandType(SpecialCommandType.EndSpecial, false);
        }
        private static void CancelState()
        {
            if (DataCenter.State) DoStateCommandType(StateCommandType.Cancel);
        }

        static float _lastCountdownTime = 0;
        internal static void UpdateRotationState()
        {
            if (ActionUpdater.AutoCancelTime != DateTime.MinValue &&
                (!DataCenter.State || DataCenter.InCombat))
            {
                ActionUpdater.AutoCancelTime = DateTime.MinValue;
            }

            var target = DataCenter.AllHostileTargets.FirstOrDefault(t => t.TargetObjectId == Player.Object.ObjectId);

            if (Svc.Condition[ConditionFlag.LoggingOut])
            {
                CancelState();
            }
            else if (Service.Config.GetValue(PluginConfigBool.AutoOffWhenDead)
                && Player.Available
                && Player.Object.CurrentHp == 0)
            {
                CancelState();
            }
            else if (Service.Config.GetValue(PluginConfigBool.AutoOffCutScene)
                && Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent])
            {
                CancelState();
            }
            else if (Service.Config.GetValue(PluginConfigBool.AutoOffBetweenArea)
                && (Svc.Condition[ConditionFlag.BetweenAreas]
                || Svc.Condition[ConditionFlag.BetweenAreas51]))
            {
                CancelState();
            }
            //Cancel when pull
            else if (Service.CountDownTime == 0 && _lastCountdownTime > 0.2f)
            {
                _lastCountdownTime = 0;
                CancelState();
            }
            //Auto manual on being attacked by someone.
            else if (Service.Config.GetValue(PluginConfigBool.StartOnAttackedBySomeone)
                && target != null
                && !target.IsDummy())
            {
                if (!DataCenter.State)
                {
                    DoStateCommandType(StateCommandType.Manual);
                }
            }
            //Auto start at count Down.
            else if (Service.Config.GetValue(PluginConfigBool.StartOnCountdown)
                && Service.CountDownTime > 0)
            {
                _lastCountdownTime = Service.CountDownTime;
                if (!DataCenter.State)
                {
                    DoStateCommandType(StateCommandType.Auto);
                }
            }
            //Cancel when after combat.
            else if (ActionUpdater.AutoCancelTime != DateTime.MinValue
                && DateTime.Now > ActionUpdater.AutoCancelTime)
            {
                CancelState();
                ActionUpdater.AutoCancelTime = DateTime.MinValue;
            }

            //Auto switch conditions.
            else if (DataCenter.RightSet.SwitchCancelConditionSet?.IsTrue(DataCenter.RightNowRotation) ?? false)
            {
                CancelState();
            }
            else if (DataCenter.RightSet.SwitchManualConditionSet?.IsTrue(DataCenter.RightNowRotation) ?? false)
            {
                if (!DataCenter.State)
                {
                    DoStateCommandType(StateCommandType.Manual);
                }
            }
            else if (DataCenter.RightSet.SwitchAutoConditionSet?.IsTrue(DataCenter.RightNowRotation) ?? false)
            {
                if (!DataCenter.State)
                {
                    DoStateCommandType(StateCommandType.Auto);
                }
            }
        }
    }
}
