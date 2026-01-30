using System.Drawing;
using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4;

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Angles> listener, 
        string? format = Angles.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Angles, Task> listener, 
        string? format = Angles.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Animation> listener, 
        string? format = Animation.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Animation, Task> listener, 
        string? format = Animation.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Buttons> listener, 
        string? format = Buttons.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Buttons, Task> listener, 
        string? format = Buttons.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<ClientXY> listener, 
        string? format = ClientXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<ClientXY, Task> listener, 
        string? format = ClientXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Coordinates> listener, 
        string? format = Coordinates.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Coordinates, Task> listener, 
        string? format = Coordinates.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<CurrentTarget> listener, 
        string? format = CurrentTarget.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<CurrentTarget, Task> listener, 
        string? format = CurrentTarget.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Data> listener, 
        string? format = Data.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Data, Task> listener, 
        string? format = Data.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<DataTransfer> listener, 
        string? format = DataTransfer.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<DataTransfer, Task> listener, 
        string? format = DataTransfer.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Deltas> listener, 
        string? format = Deltas.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Deltas, Task> listener, 
        string? format = Deltas.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Detail> listener, 
        string? format = Detail.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Detail, Task> listener, 
        string? format = Detail.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<DeviceMotion> listener, 
        string? format = DeviceMotion.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<DeviceMotion, Task> listener, 
        string? format = DeviceMotion.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<DeviceOrientation> listener, 
        string? format = DeviceOrientation.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<DeviceOrientation, Task> listener, 
        string? format = DeviceOrientation.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Error> listener, 
        string? format = Error.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Error, Task> listener, 
        string? format = Error.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<HashChange> listener, 
        string? format = HashChange.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<HashChange, Task> listener, 
        string? format = HashChange.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<IsComposing> listener, 
        string? format = IsComposing.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<IsComposing, Task> listener, 
        string? format = IsComposing.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Keys> listener, 
        string? format = Keys.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Keys, Task> listener, 
        string? format = Keys.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Length> listener, 
        string? format = Event.Subsets.Length.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Length, Task> listener, 
        string? format = Event.Subsets.Length.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<ModifierAlt> listener, 
        string? format = ModifierAlt.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<ModifierAlt, Task> listener, 
        string? format = ModifierAlt.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<ModifierCtrl> listener, 
        string? format = ModifierCtrl.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<ModifierCtrl, Task> listener, 
        string? format = ModifierCtrl.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<ModifierMeta> listener, 
        string? format = ModifierMeta.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<ModifierMeta, Task> listener, 
        string? format = ModifierMeta.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Modifiers> listener, 
        string? format = Modifiers.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Modifiers, Task> listener, 
        string? format = Modifiers.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<ModifierShift> listener, 
        string? format = ModifierShift.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<ModifierShift, Task> listener, 
        string? format = ModifierShift.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<MovementXY> listener, 
        string? format = MovementXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<MovementXY, Task> listener, 
        string? format = MovementXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<OffsetXY> listener, 
        string? format = OffsetXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<OffsetXY, Task> listener, 
        string? format = OffsetXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<PageXY> listener, 
        string? format = PageXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<PageXY, Task> listener, 
        string? format = PageXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Persisted> listener, 
        string? format = Persisted.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Persisted, Task> listener, 
        string? format = Persisted.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Pointer> listener, 
        string? format = Pointer.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Pointer, Task> listener, 
        string? format = Pointer.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Pressures> listener, 
        string? format = Pressures.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Pressures, Task> listener, 
        string? format = Pressures.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<PreventDefault> listener, 
        string? format = PreventDefault.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<PreventDefault, Task> listener, 
        string? format = PreventDefault.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Progress> listener, 
        string? format = Progress.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Progress, Task> listener, 
        string? format = Progress.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<RelatedTarget> listener, 
        string? format = RelatedTarget.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<RelatedTarget, Task> listener, 
        string? format = RelatedTarget.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<ScreenXY> listener, 
        string? format = ScreenXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<ScreenXY, Task> listener, 
        string? format = ScreenXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Skipped> listener, 
        string? format = Skipped.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Skipped, Task> listener, 
        string? format = Skipped.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<States> listener, 
        string? format = States.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<States, Task> listener, 
        string? format = States.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Submitter> listener, 
        string? format = Submitter.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Submitter, Task> listener, 
        string? format = Submitter.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Target> listener, 
        string? format = Target.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Target, Task> listener, 
        string? format = Target.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Target<string>> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Target<bool>> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Target<int>> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    // TODO: I think converting `Func<Event, T>` to `Action<Event>` (e => listener(e)) is allocating.  
    // Verify this and fix it.  There are 4 below, and one more `e => c++` at the bottom.
    // How interesting.  Long, float, double, and decimal must use the signature
    // `Func<Target<T>, T>` instead of `Action<Target<T>>` or else the 
    // "call is ambiguous" due to these types' ability to cast to other types.
    public bool AppendFormatted(
        Func<Target<long>, long> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(e => listener(e), format, expression);
    public bool AppendFormatted(
        Func<Target<float>, float> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(e => listener(e), format, expression);
    public bool AppendFormatted(
        Func<Target<double>, double> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(e => listener(e), format, expression);
    public bool AppendFormatted(
        Func<Target<decimal>, decimal> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(e => listener(e), format, expression);
    public bool AppendFormatted(
        Action<Target<DateTime>> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Target<DateOnly>> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Target<TimeOnly>> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Target<Color>> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Target<Uri>> listener,
        string? format = Target.Format,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Tilts> listener, 
        string? format = Tilts.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Tilts, Task> listener, 
        string? format = Tilts.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Touches> listener, 
        string? format = Touches.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Touches, Task> listener, 
        string? format = Touches.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<View> listener, 
        string? format = View.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<View, Task> listener, 
        string? format = View.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<WidthHeight> listener, 
        string? format = WidthHeight.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<WidthHeight, Task> listener, 
        string? format = WidthHeight.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<XY> listener, 
        string? format = XY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<XY, Task> listener, 
        string? format = XY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}