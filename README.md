# Brouter3
An awesome convention-based router for Blazor.

App.razor

```razor
<LayoutView Layout="@typeof(MainLayout)">
    <SBrouter Root="home">
        <NotFound>
            <div class="main">
                <h1 class="title">404</h1>
                <div class="description">There is nothing here.</div>
            </div>
        </NotFound>
    </SBrouter>
</LayoutView>
```
