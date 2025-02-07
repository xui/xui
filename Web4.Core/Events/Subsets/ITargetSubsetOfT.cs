using System.Drawing;
using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ITargetSubset<T>
        {
            const string Format = "target";

            /// <summary>
            /// A reference to the object to which the event was originally dispatched.
            /// </summary>
            EventTarget<T> Target { get; }
        }

        public interface EventTarget<T>
        {
            public string ID { get; }
            public string Name { get; }
            public string Type { get; }
            public bool? Checked { get; }
            public T Value { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Func<Target<string>, string> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous<string, int>(
                    GetArgName(expression), 
                    listener, 
                    null, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<bool>, bool> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous<bool, int>(
                    GetArgName(expression), 
                    listener, 
                    null, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<int>, int> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous(
                    GetArgName(expression), 
                    listener, 
                    listener, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<long>, long> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous(
                    GetArgName(expression), 
                    listener, 
                    listener, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<float>, float> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous(
                    GetArgName(expression), 
                    listener, 
                    listener, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<double>, double> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous(
                    GetArgName(expression), 
                    listener, 
                    listener, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<decimal>, decimal> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous(
                    GetArgName(expression), 
                    listener, 
                    listener, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<DateTime>, DateTime> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous(
                    GetArgName(expression), 
                    listener, 
                    listener, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<DateOnly>, DateOnly> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous(
                    GetArgName(expression), 
                    listener, 
                    listener, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<TimeOnly>, TimeOnly> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous(
                    GetArgName(expression), 
                    listener, 
                    listener, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<Color>, Color> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous<Color, int>(
                    GetArgName(expression), 
                    listener, 
                    null, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);

        public bool AppendFormatted(
            Func<Target<Uri>, Uri> listener, 
            string? format = null, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendAmbiguous<Uri, int>(
                    GetArgName(expression), 
                    listener, 
                    null, 
                    formatForAttribute: format,
                    formatForListener: format ?? Target.Format, 
                    expression: expression);
    }
}