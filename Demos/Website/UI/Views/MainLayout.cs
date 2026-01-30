partial class UI
{
    public Html Wat(ViewModel vm) => MainLayout(vm);

    protected Html MainLayout(ViewModel vm) => $"""
        <div></div>
    """;
        // <html>
        // <head>
        //     <!-- Zero script-refs.  Such fast. -->

        //     <link rel="stylesheet" href="/css/preflight.css" />
        //     <link rel="stylesheet" href="/css/site.css" />
        //     <meta name="viewport" content="width=device-width, user-scalable=no">
        // </head>
        // <body>
        //     <nav>
        //     </nav>

        //     {new Header()}
        //     {new VSCode()}

        //     <div id="cards">
        //         {new JavaScript(vm)}
        //         {new Syntax()}
        //         {new Api()}
        //         {new Hooks()}
        //         {new Blocking()}
        //         {new Seo()}
        //         {new Hydration()}
        //         {new VirtualDom()}
        //         {new Latency()}
        //         {new Allocations()}
        //         {new Benchmarks()}
        //         {new HotReloads()}
        //         {new Repl()}
        //         {new Pages()}
        //         {new Sense()}
        //         {new Roadmap()}
        //     </div>
        // </body>
        // </html>
        // """;
}
