using System.Reflection;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Rendering;

namespace Brouter3;

public partial class SBrouter : ComponentBase, IDisposable
{
    private static readonly char[] _QueryOrHashStartChar = { '?', '#' };



    private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>();
    private Type _matchedPage;
    private string _path;


    [Inject] private NavigationManager _navManager { get; set; }
    [Inject] private INavigationInterception _navInterception { get; set; }



    [Parameter] public Assembly PagesAssembly { get; set; } = default!;
    [Parameter] public RenderFragment NotFound { get; set; }
    [Parameter] public string Root { get; set; }



    protected override void OnInitialized()
    {
        base.OnInitialized();

        _navManager.LocationChanged += NavManagerLocationChanged;

        ExtractPages();
        CreatePathInfo();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender is false) return;

        await _navInterception.EnableNavigationInterceptionAsync();

        Match();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        var seq = 0;
        builder.OpenComponent<CascadingValue<SBrouter>>(seq++);
        builder.AddAttribute(seq++, "Name", "Brouter");
        builder.AddAttribute(seq++, "Value", this);

        if (_matchedPage is null)
        {
            builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(builder2 => builder2.AddContent(seq, NotFound)));
        }
        else
        {
            builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(builder2 =>
            {
                builder2.OpenComponent(seq++, _matchedPage);
                builder2.CloseComponent();
            }));
        }

        builder.CloseComponent();
    }



    private void ExtractPages()
    {
        var assembly = PagesAssembly ?? Assembly.GetEntryAssembly();

        foreach (var type in assembly.ExportedTypes)
        {
            if (typeof(IComponent).IsAssignableFrom(type))
            {
                if (type.Name.EndsWith("Page"))
                {
                    var path = type.Name.Replace("Page", "").ToLower();
                    _pages.Add(path, type);
                }
            }
        }
    }

    private void NavManagerLocationChanged(object sender, LocationChangedEventArgs e)
    {
        CreatePathInfo();
        Match();
    }

    private void CreatePathInfo()
    {
        var path = _navManager.ToBaseRelativePath(_navManager.Uri);
        var firstIndex = path.IndexOfAny(_QueryOrHashStartChar);
        path = firstIndex < 0 ? path : path.Substring(0, firstIndex);
        path = $"/{path}";
        _path = path;
    }

    private void Match()
    {
        _matchedPage = null;
        var path = _path.Trim('/').ToLower();

        if (Root is not null && path == "")
        {
            _navManager.NavigateTo(Root);
            return;
        }

        _pages.TryGetValue(path, out _matchedPage);

        StateHasChanged();
    }

    

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing is false) return;
    }
}
