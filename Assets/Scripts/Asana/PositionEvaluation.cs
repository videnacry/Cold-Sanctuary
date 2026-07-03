/// <summary>
/// How well a single body part position was executed during an asana attempt.
/// A list of these is returned in PaletteResult.payload by AsanaEvaluator.
/// TeacherNPC reads this list to choose its feedback line.
/// </summary>
public struct PositionEvaluation
{
    public BodyPart          part;
    public PositionQuality   quality;
    public float             statGap;       // how much is missing (0 if Correct)
    public BodyStatDimension limitingDim;   // which dimension is the bottleneck
}

public enum PositionQuality
{
    Correct,             // stats sufficient — position executed well
    DirectionOkLowStat,  // intent correct, stats insufficient — productive discomfort
    Impossible,          // gap too large — body can't reach the position at all
}
