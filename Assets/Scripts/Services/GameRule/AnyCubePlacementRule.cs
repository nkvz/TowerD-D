public class AnyCubePlacementRule : IPlacementRule
{
    public bool CanPlace(Cube dragged, Cube target) => dragged != null && target != null;
}
