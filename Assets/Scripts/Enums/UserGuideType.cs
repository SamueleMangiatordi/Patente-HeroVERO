using UnityEngine;
// UserGuideType.cs
public enum UserGuideType
{
    None, // A default value, useful for unassigned or initial states
    Accelerate,
    Decelerate,
    Turn,
    TurnSignalError,
    TurnSignalErrorExceeded,
    OffRoad,
    CarResetPosition,
    // Add all your specific user guide identifiers here
}
