using UnityEngine;
// UserGuideType.cs
public enum UserGuideType
{
    None, // A default value, useful for unassigned or initial states
    Accelerate,
    Decelerate,
    Turn,
    TurnSignal,
    TurnSignalError,
    TurnSignalErrorExceeded,
    OffRoad,
    CarResetPosition,
    CarHitted,
    SignDetails,
    RightOfWayNotRespected,
    StopSignNotRespected,
    FirstQuestionWrongAnswer,
    FirstQuestionCorrectAnswer,
    FirstQuestion,
    //!!ALWAYS PUT NEW TYPE AT THE AND OF THE LIST!! Otherwise the index will change and break existing references
}
