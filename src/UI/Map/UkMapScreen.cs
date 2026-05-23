namespace Westminster.UI.Map;

public sealed class UkMapScreen
{
    public UkMapPresentation BuildPresentation(UkMapViewModel viewModel)
    {
        return UkMapPresentationBuilder.Build(viewModel);
    }
}
