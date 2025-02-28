﻿using ECommons.ExcelServices;
using Lumina.Excel.GeneratedSheets;
using RotationSolver.Basic.Traits;

namespace RotationSolver.Basic.Rotations;

/// <summary>
/// The custom rotation.
/// </summary>
public interface ICustomRotation : ITexture
{
    /// <summary>
    /// The average count of not recommend members using.
    /// </summary>
    double AverageCountOfLastUsing { get; }

    /// <summary>
    /// The max count of not recommend members using.
    /// </summary>
    int MaxCountOfLastUsing { get; }

    /// <summary>
    /// The average count of not recommend members using.
    /// </summary>
    double AverageCountOfCombatTimeUsing { get; }

    /// <summary>
    /// The max count of not recommend members using.
    /// </summary>
    int MaxCountOfCombatTimeUsing { get; }

    /// <summary>
    /// Whether show the status in the formal page.
    /// </summary>
    bool ShowStatus { get; }

    /// <summary>
    /// Is this rotation valid.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Why this rotation is not valid.
    /// </summary>
    string WhyNotValid { get; }

    /// <summary>
    /// The class job about this rotation.
    /// </summary>
    ClassJob ClassJob { get; }

    /// <summary>
    /// All jobs.
    /// </summary>
    Job[] Jobs { get; }

    /// <summary>
    /// The game version in writing.
    /// </summary>
    string GameVersion { get; }

    /// <summary>
    /// The name of this rotation.
    /// </summary>
    string RotationName { get; }

    /// <summary>
    /// Configurations about this rotation.
    /// </summary>
    IRotationConfigSet Configs { get; }

    /// <summary>
    /// The type of medicine.
    /// </summary>
    MedicineType MedicineType { get; }

    /// <summary>
    /// All base action.
    /// </summary>
    IBaseAction[] AllBaseActions { get; }

    /// <summary>
    /// All action including base and item.
    /// </summary>
    IAction[] AllActions { get; }

    /// <summary>
    /// All traits.
    /// </summary>
    IBaseTrait[] AllTraits { get; }

    /// <summary>
    /// All bool properties.
    /// </summary>
    PropertyInfo[] AllBools { get; }

    /// <summary>
    /// All byte properties.
    /// </summary>
    PropertyInfo[] AllBytesOrInt { get; }

    /// <summary>
    /// All time methods.
    /// </summary>
    PropertyInfo[] AllFloats { get; }


    internal IAction ActionHealAreaGCD { get; }
    internal IAction ActionHealAreaAbility { get; }
    internal IAction ActionHealSingleGCD { get; }
    internal IAction ActionHealSingleAbility { get; }
    internal IAction ActionDefenseAreaGCD { get; }
    internal IAction ActionDefenseAreaAbility { get; }
    internal IAction ActionDefenseSingleGCD { get; }
    internal IAction ActionDefenseSingleAbility { get; }
    internal IAction ActionMoveForwardGCD { get; }
    internal IAction ActionMoveForwardAbility { get; }
    internal IAction ActionMoveBackAbility { get; }
    internal IAction ActionSpeedAbility { get; }
    internal IAction EsunaStanceNorthGCD { get; }
    internal IAction EsunaStanceNorthAbility { get; }
    internal IAction RaiseShirkGCD { get; }
    internal IAction RaiseShirkAbility { get; }
    internal IAction AntiKnockbackAbility { get; }

    /// <summary>
    /// Try to use this rotation.
    /// </summary>
    /// <param name="newAction">the next action.</param>
    /// <param name="gcdAction">the next gcd action.</param>
    /// <returns>succeed</returns>
    bool TryInvoke(out IAction newAction, out IAction gcdAction);

    /// <summary>
    /// This is an <seealso cref="ImGui"/> method for display the rotation status on Window.
    /// </summary>
    void DisplayStatus();

    /// <summary>
    /// It occur when territory changed or rotation changed.
    /// </summary>
    void OnTerritoryChanged();
}
