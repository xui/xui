#pragma warning disable IDE1006 // Naming Styles

using Web4.Events.Subsets;

namespace Web4.Events;

public interface Aliases
{
    public interface Animation : IAnimationEvent { }
    public interface Composition : ICompositionEvent { }
    public interface DeviceMotion : IDeviceMotionEvent { }
    public interface DeviceOrientation : IDeviceOrientationEvent { }
    public interface Drag : IDragEvent { }
    public interface Focus : IFocusEvent { }
    public interface HashChange: IHashChangeEvent { }
    public interface Input<T> : IInputEvent<T> { }
    public interface Keyboard : IKeyboardEvent { }
    public interface Mouse : IMouseEvent { }
    public interface Pointer: IPointerEvent { }
    public interface Progress : IProgressEvent { }
    public interface Submit : ISubmitEvent { }
    public interface Touch : ITouchEvent { }
    public interface Transition : ITransitionEvent { }
    public interface Wheel : IWheelEvent { }
    public interface Subsets
    {
        public interface Angles : IAngles { }
        public interface Animation : IAnimation { }
        public interface Buttons : IButtons { }
        public interface ClientXY : IClientXY { }
        public interface Coordinates : ICoordinates { }
        public interface Data : IData { }
        public interface DataTransfer : IDataTransfer { }
        public interface Deltas : IDeltas { }
        public interface Detail : IDetail { }
        public interface DeviceMotion : IDeviceMotion { }
        public interface DeviceOrientation : IDeviceOrientation { }
        public interface Error : IError { }
        public interface HashChange : IHashChange { }
        public interface IsComposing : IIsComposing { }
        public interface Keys : IKeys { }
        public interface ModifierAlt : IModifierAlt { }
        public interface ModifierCtrl : IModifierCtrl { }
        public interface ModifierMeta : IModifierMeta { }
        public interface Modifiers : IModifiers { }
        public interface ModifierShift : IModifierShift { }
        public interface MovementXY : IMovementXY { }
        public interface OffsetXY : IOffsetXY { }
        public interface PageXY : IPageXY { }
        public interface Pointer : IPointer { }
        public interface Pressures : IPressures { }
        public interface Progress : IProgress { }
        public interface RelatedTarget: IRelatedTarget { }
        public interface ScreenXY : IScreenXY { }
        public interface Submitter : ISubmitter { }
        public interface Target : ITarget { }
        public interface Target<T> : ITarget<T> { }
        public interface Tilts : ITilts { }
        public interface Touches : ITouches { }
        public interface WidthHeight : IWidthHeight { }
        public interface X : IX { }
        public interface XY : IXY { }
        public interface Y : IY { }
    }
}

#pragma warning restore IDE1006 // Naming Styles
