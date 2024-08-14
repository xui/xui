partial class UI : UI<ViewModel>
{
    public UI() : base()
    {
    }

    public override void MapPages()
    {
        MapPage("/", Index);
        MapPage("/zero-pages", ZeroPages);
        MapPage("/zero-javascript", ZeroJavaScript);
    }

    void Index(Context context)
    {
        context.ViewModel.Name = "Twas clicked";
        context.ViewModel.ShowAdditional = false;
    }

    void ZeroPages(Context context)
    {
        context.ViewModel.Name = "Twas clicked";
        context.ViewModel.ShowAdditional = true;
    }

    void ZeroJavaScript(Context context)
    {
        context.ViewModel.Name = "OK, back to normal: Rylan Barnes";
        context.ViewModel.ShowAdditional = true;
    }

}