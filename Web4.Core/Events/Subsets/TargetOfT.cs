using System.Drawing;
using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Target<T>
        {
            const string Format = "target";

            /// <summary>
            /// A reference to the object to which the event was originally dispatched.
            /// </summary>
            EventTarget<T> Target { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.Target<string>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<string>, string> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous<string, int>(
                GetArgName(expression), 
                listener, 
                null, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<bool>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<bool>, bool> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous<bool, int>(
                GetArgName(expression), 
                listener, 
                null, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<int>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<int>, int> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(
                GetArgName(expression), 
                listener, 
                listener, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<long>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<long>, long> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(
                GetArgName(expression), 
                listener, 
                listener, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<float>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<float>, float> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(
                GetArgName(expression), 
                listener, 
                listener, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<double>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<double>, double> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(
                GetArgName(expression), 
                listener, 
                listener, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<decimal>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<decimal>, decimal> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(
                GetArgName(expression), 
                listener, 
                listener, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<DateTime>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<DateTime>, DateTime> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(
                GetArgName(expression), 
                listener, 
                listener, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<DateOnly>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<DateOnly>, DateOnly> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(
                GetArgName(expression), 
                listener, 
                listener, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<TimeOnly>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<TimeOnly>, TimeOnly> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous(
                GetArgName(expression), 
                listener, 
                listener, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<Color>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<Color>, Color> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous<Color, int>(
                GetArgName(expression), 
                listener, 
                null, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<Uri>> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => composer.WriteEventListener(
                ref this,
                listener,
                format: format ?? Event.Subsets.Target.Format,
                expression: expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<Uri>, Uri> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendAmbiguous<Uri, int>(
                GetArgName(expression), 
                listener, 
                null, 
                formatForAttribute: format,
                formatForListener: format ?? Event.Subsets.Target.Format, 
                expression: expression);
}