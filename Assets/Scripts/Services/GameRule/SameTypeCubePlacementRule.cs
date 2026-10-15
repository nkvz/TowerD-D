public class SameTypeCubePlacementRule : IPlacementRule
{
    public bool CanPlace(Cube dragged, Cube target)
    {
        return dragged != null && target != null && dragged.Config != null && target.Config != null && dragged.Config.Type == target.Config.Type;
    }
}
