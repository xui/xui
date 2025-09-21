readonly record struct Syntax() : IRender
{
    public Html Render() => $"""
        <section id="zero-syntax">
            <div>
                <img src="img/zero_syntax.svg" />
            </div>
            <article>
                <h2>
                    Simplify
                </h2>
                <h1>
                    Learn <strong>zero</strong> new syntax
                </h1>
                <p>
                    It's just regular HTML with regular C# expressions sprinkled inside 
                    regular <code>$" "</code> string interpolations.  
                    If you already know C# then you already know ZeroScript.  
                    If you know TypeScript then you're already 90% there.
                </p>
                <p>
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
                    tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam,
                    quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo
                    consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse
                    cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat
                    non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
                </p>
                <p>
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod
                    tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam,
                    quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo
                    consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse
                    cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat
                    non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
                </p>
            </article>
        </section>
        """;
}
